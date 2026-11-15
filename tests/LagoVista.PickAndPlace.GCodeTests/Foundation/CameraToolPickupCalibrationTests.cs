using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class CameraToolPickupCalibrationTests
    {
        private static Point2D<double> XY(double x, double y) =>
            new Point2D<double>(x, y);

        private static CameraToolPickupObservation Observation(double x, double y,
            DateTimeOffset finished, DateTimeOffset captured) =>
            new CameraToolPickupObservation
            {
                HeadIndex = 1, NozzleId = "N14", FixtureId = "black-holder-white-plate",
                PartRelativeToNozzleMm = XY(x, y),
                VacuumConfirmed = true, StableVision = true, NozzleCalibrated = true,
                MotionFinishedUtc = finished, CapturedUtc = captured
            };

        [Test]
        public void FirstPickupCorrectsStoredHeadOffsetInPartMinusNozzleDirection()
        {
            var now = new DateTimeOffset(2026, 11, 15, 12, 0, 0, TimeSpan.Zero);
            var old = XY(22.7, 65);
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(old,
                Observation(-0.30, 0.15, now.AddSeconds(-1), now.AddMilliseconds(-100)),
                now, out var candidate, out var reason), Is.True, reason);
            Assert.That(candidate.ProposedOffsetMm.X, Is.EqualTo(22.4).Within(0.000001));
            Assert.That(candidate.ProposedOffsetMm.Y, Is.EqualTo(65.15).Within(0.000001));
            Assert.That(candidate.SecondPickupJogMm.X, Is.EqualTo(-0.30).Within(0.000001));
            Assert.That(old.X, Is.EqualTo(22.7), "Never mutate the stored offset before verification.");
        }

        [Test]
        public void SecondPickupUsesBoundedRelativeJogWithOriginalOffsetStillActive()
        {
            var now = DateTimeOffset.UtcNow;
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(XY(22.7, 65),
                Observation(-0.3, 0.15, now.AddSeconds(-2), now.AddSeconds(-1)),
                now, out var candidate, out var reason), Is.True, reason);
            Assert.That(CameraToolPickupCalibrationMath.TryPlanSecondPickup(
                candidate, XY(165, 128), XY(330, 355), out var jog, out reason), Is.True, reason);
            Assert.That(jog.X, Is.EqualTo(-0.3).Within(0.000001));
            Assert.That(jog.Y, Is.EqualTo(0.15).Within(0.000001));
            Assert.That(CameraToolPickupCalibrationMath.TryPlanSecondPickup(
                candidate, XY(304, 300), XY(330, 355), out _, out reason), Is.False,
                "Physical XY must account for old selected-tool offset and G92.");
            Assert.That(reason, Does.Contain("physical XY"));
        }

        [Test]
        public void FreshSecondPickupAcceptsSmallResidual()
        {
            var now = DateTimeOffset.UtcNow;
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(XY(22.7, 65),
                Observation(-0.3, 0.15, now.AddSeconds(-2.4), now.AddSeconds(-2)),
                now, out var candidate, out _), Is.True);
            Assert.That(CameraToolPickupCalibrationMath.TryVerify(candidate,
                Observation(0.04, -0.05, now.AddSeconds(-1), now.AddMilliseconds(-200)),
                now, out var residual, out var reason), Is.True, reason);
            Assert.That(residual, Is.LessThan(CameraToolPickupCalibrationMath.VerificationToleranceMm));
        }

        [Test]
        public void MissingVacuumOrRunoutOrStaleCameraCannotProposeAnOffset()
        {
            var now = DateTimeOffset.UtcNow;
            var observed = Observation(0.2, 0.1, now.AddSeconds(-1), now);
            observed.VacuumConfirmed = false;
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(XY(22, 65), observed,
                now, out _, out _), Is.False);
            observed.VacuumConfirmed = true;
            observed.NozzleCalibrated = false;
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(XY(22, 65), observed,
                now, out _, out _), Is.False);
            observed.NozzleCalibrated = true;
            observed.CapturedUtc = now.AddSeconds(-10);
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(XY(22, 65), observed,
                now, out _, out _), Is.False);
        }

        [Test]
        public void RejectsLargeCorrectionWithoutASecondPick()
        {
            var now = DateTimeOffset.UtcNow;
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(XY(22, 65),
                Observation(2.1, 0, now.AddSeconds(-1), now), now,
                out var candidate, out var reason), Is.False);
            Assert.That(candidate, Is.Null);
            Assert.That(reason, Does.Contain("limit"));
        }

        [Test]
        public void ChangedNozzleOrUnverifiedResidualPreservesFirstPassAsUnsaved()
        {
            var now = DateTimeOffset.UtcNow;
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(XY(22, 65),
                Observation(-0.4, 0.2, now.AddSeconds(-2), now.AddSeconds(-1.5)),
                now, out var candidate, out _), Is.True);
            var second = Observation(0.15, 0, now.AddSeconds(-0.9), now.AddMilliseconds(-100));
            Assert.That(CameraToolPickupCalibrationMath.TryVerify(candidate, second, now,
                out _, out _), Is.False);
            second.PartRelativeToNozzleMm = XY(0.01, 0.02);
            second.NozzleId = "N24";
            Assert.That(CameraToolPickupCalibrationMath.TryVerify(candidate, second, now,
                out _, out _), Is.False);
            second.NozzleId = "N14";
            second.MotionFinishedUtc = now.AddSeconds(-2);
            Assert.That(CameraToolPickupCalibrationMath.TryVerify(candidate, second, now,
                out _, out _), Is.False);
        }

        [Test]
        public void ProcedureHasTwoSeparatePickupsAndCommitRequiresSavedEvidence()
        {
            var d = CameraToolPickupProcedureDefinition.Create();
            Assert.That(d.Key, Is.EqualTo("camera-tool-two-pass-pickup"));
            Assert.That(d.Steps.Count, Is.EqualTo(16));
            Assert.That(d.Steps.Select(s => s.Key).ToArray(), Is.EqualTo(new[]
            {
                "readiness","known-location","first-target","nozzle-reference","first-approach","first-pick",
                "first-inspect","candidate","replace-part","second-target",
                "second-approach","second-pick","second-inspect","verify",
                "safe-camera-mode","commit-offset"
            }));
            Assert.That(d.Steps.Last().RequiresSavedEvidence, Is.True);
            Assert.That(d.Steps.Single(s=>s.Key=="second-inspect").RequiredVerification,
                Is.EqualTo(MachineOperationVerificationKind.VisionVerified));
            Assert.That(d.Steps.Take(15).All(s => !s.RequiresSavedEvidence), Is.True);

            var now = DateTimeOffset.UtcNow;
            var session = new MachineOperationSession(d,
                new MachineOperationTarget { MachineId = "test" }, now);
            session.Start(now);
            foreach (var step in d.Steps.Take(15))
            {
                session.RecordEvidence(new MachineOperationEvidence
                {
                    RunId = session.Snapshot.RunId, StepKey = step.Key,
                    Name = step.Key, CapturedAtUtc = now,
                    Verification = step.RequiredVerification
                });
                session.CompleteActiveStep(step.Key, now);
            }
            session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = session.Snapshot.RunId, StepKey = "commit-offset",
                Name = "Unsaved calibration", CapturedAtUtc = now,
                Verification = MachineOperationVerificationKind.MachineObserved,
                Saved = false
            });
            Assert.Throws<InvalidOperationException>(() =>
                session.CompleteActiveStep("commit-offset", now));
        }
    }
}
