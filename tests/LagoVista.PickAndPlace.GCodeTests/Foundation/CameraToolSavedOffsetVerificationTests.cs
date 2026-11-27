using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    [TestFixture]
    public class CameraToolSavedOffsetVerificationTests
    {
        private static Point2D<double> XY(double x, double y) =>
            new Point2D<double>(x, y);

        private static CameraToolPickupObservation Observation(double x, double y,
            DateTimeOffset finished, DateTimeOffset seen) =>
            new CameraToolPickupObservation
            {
                HeadIndex = 1,
                NozzleId = "N14",
                FixtureId = "known-location-7mm-cylinder",
                PartRelativeToNozzleMm = XY(x, y),
                VacuumConfirmed = true,
                StableVision = true,
                NozzleCalibrated = true,
                MotionFinishedUtc = finished,
                CapturedUtc = seen
            };

        private static CameraToolPickupCandidate Candidate(DateTimeOffset now)
        {
            Assert.That(CameraToolPickupCalibrationMath.TryPropose(
                XY(22.7, 65), Observation(0.810, 0.344,
                    now.AddSeconds(-6), now.AddSeconds(-5)),
                now.AddSeconds(-4), out var candidate, out var reason),
                Is.True, reason);
            return candidate;
        }

        [Test]
        public void PersistedThirdPickupAcceptsNozzleRelativeErrorWithinPointTwoFiveMm()
        {
            var now = DateTimeOffset.UtcNow;
            var candidate = Candidate(now);
            var third = Observation(0.240, 0.017,
                now.AddSeconds(-2), now.AddSeconds(-1));
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out var residual, out var error),
                Is.True, error);
            Assert.That(residual, Is.EqualTo(Math.Sqrt(0.240 * 0.240 + 0.017 * 0.017))
                .Within(0.0000001));
            Assert.That(residual, Is.LessThanOrEqualTo(0.25));
        }

        [Test]
        public void SavedOffsetProofRejectsAbovePointTwoFiveEvenIfVisionIsStable()
        {
            var now = DateTimeOffset.UtcNow;
            var candidate = Candidate(now);
            var third = Observation(0.251, 0.0,
                now.AddSeconds(-2), now.AddSeconds(-1));
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out var residual, out var reason),
                Is.False);
            Assert.That(residual, Is.GreaterThan(0.25));
            Assert.That(reason, Does.Contain("0.250 mm"));
        }

        [Test]
        public void SavedOffsetProofRejectsWrongHeadNozzleAndStaleThirdPickup()
        {
            var now = DateTimeOffset.UtcNow;
            var candidate = Candidate(now);
            var third = Observation(0.01, 0.01,
                now.AddSeconds(-2), now.AddSeconds(-1));
            third.NozzleId = "N24";
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out _, out _), Is.False);
            third.NozzleId = "N14";
            third.HeadIndex = 2;
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out _, out _), Is.False);
            third.HeadIndex = 1;
            third.CapturedUtc = now.AddSeconds(-10);
            Assert.That(CameraToolPickupCalibrationMath.TryVerifySavedOffsetPickup(
                candidate, third, now, out _, out _), Is.False);
        }

        [Test]
        public void SavedOffsetHistoryRequiresReceiptAndDistinctThirdPickup()
        {
            var definition = CameraToolSavedOffsetVerificationProcedure.Create();
            Assert.That(definition.Key,
                Is.EqualTo("camera-tool-saved-offset-pickup-verification"));
            Assert.That(definition.Steps.Select(s => s.Key), Is.EqualTo(new[]
            {
                "readiness", "save-offset", "read-back", "nozzle-reference",
                "target", "approach", "pick", "inspect", "verify", "return",
                "safe-camera-mode"
            }));
            Assert.That(definition.Steps.Single(s => s.Key == "save-offset")
                .RequiresSavedEvidence, Is.True);
            Assert.That(definition.Steps.Where(s => s.Key != "save-offset")
                .All(s => !s.RequiresSavedEvidence), Is.True);

            var now = DateTimeOffset.UtcNow;
            var run = new MachineOperationSession(definition,
                new MachineOperationTarget { MachineId = "lumen-jr" }, now);
            run.Start(now.AddMilliseconds(1));
            run.RecordEvidence(new MachineOperationEvidence
            {
                RunId = run.Snapshot.RunId,
                StepKey = "readiness",
                CapturedAtUtc = now.AddMilliseconds(2),
                Verification = MachineOperationVerificationKind.MachineObserved
            });
            run.CompleteActiveStep("readiness", now.AddMilliseconds(3));
            run.RecordEvidence(new MachineOperationEvidence
            {
                RunId = run.Snapshot.RunId,
                StepKey = "save-offset",
                CapturedAtUtc = now.AddMilliseconds(4),
                Verification = MachineOperationVerificationKind.MachineObserved,
                Saved = false
            });
            Assert.Throws<InvalidOperationException>(() =>
                run.CompleteActiveStep("save-offset", now.AddMilliseconds(5)));
            run.BlockActiveStep("No saved machine receipt", now.AddMilliseconds(6));
            var record = MachineOperationRunProjection.Project(
                run.Snapshot, definition,
                EntityHeader.Create("org1", "Test Org"),
                EntityHeader.Create("user1", "Test Operator"));
            Assert.That(record.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Blocked));
            Assert.That(record.Snapshot.ActiveStepKey, Is.EqualTo("save-offset"));
        }
    }
}
