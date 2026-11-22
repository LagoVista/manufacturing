using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using Newtonsoft.Json;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    [TestFixture]
    public class CameraToolPickupHistorySnapshotTests
    {
        private static readonly EntityHeader Org =
            EntityHeader.Create("org-1", "Test Org");
        private static readonly EntityHeader Operator =
            EntityHeader.Create("operator-1", "Test Operator");
        private static readonly DateTimeOffset Began =
            new DateTimeOffset(2026, 11, 22, 20, 0, 0, TimeSpan.Zero);

        private static MachineOperationSession Create()
        {
            var session = new MachineOperationSession(
                CameraToolPickupProcedureDefinition.Create(),
                new MachineOperationTarget
                {
                    MachineId = "lumen-pnp",
                    CameraId = "bottom-camera"
                }, Began);
            session.Start(Began.AddSeconds(1));
            return session;
        }

        private static void Pass(MachineOperationSession run, int stage,
            string detail, bool saved = false)
        {
            var definition = run.Definition.Steps[stage];
            var observed = Began.AddSeconds(3 + stage * 3);
            run.RecordEvidence(new MachineOperationEvidence
            {
                RunId = run.Snapshot.RunId,
                StepKey = definition.Key,
                Verification = definition.RequiredVerification,
                CapturedAtUtc = observed,
                Saved = saved,
                Name = detail,
                Value = detail,
                Source = "T014.CameraToolPickupCalibrationViewModel",
                VisionProfileKey = definition.Key == "second-inspect"
                    ? "calibrationdisc" : null,
                PixelsPerMm = definition.Key == "second-inspect" ? 12.5 : (double?)null
            });
            run.CompleteActiveStep(definition.Key, observed.AddSeconds(1));
        }

        [Test]
        public void VerifiedMeasurementOnlyRunIsBlockedAtSaveAndNeverClaimsSavedOffset()
        {
            var run = Create();
            for (var stage = 0; stage < run.Definition.Steps.Count - 1; stage++)
                Pass(run, stage, stage == 13
                    ? "Second pickup residual X=0.021 Y=-0.015 mm" : "Verified measurement");

            run.BlockActiveStep(
                "Both pickups verified; candidate offset intentionally UNSAVED.",
                Began.AddMinutes(2));
            var record = MachineOperationRunProjection.Project(
                run.Snapshot, run.Definition, Org, Operator);
            var copy = JsonConvert.DeserializeObject<MachineOperationRunRecord>(
                JsonConvert.SerializeObject(record));

            Assert.That(copy.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Blocked));
            Assert.That(copy.Snapshot.ActiveStepKey, Is.EqualTo("commit-offset"));
            Assert.That(copy.Snapshot.Steps.Last().Status,
                Is.EqualTo(MachineOperationStepStatus.Blocked));
            Assert.That(copy.Snapshot.Steps.Last().Evidence.Count, Is.Zero);
            Assert.That(copy.Snapshot.Steps
                .SelectMany(s => s.Evidence).Any(e => e.Saved), Is.False);
            Assert.That(copy.Snapshot.Steps
                .Single(s => s.Key == "second-inspect").Evidence.Single().Value,
                Does.Contain("X=0.021"));
            Assert.That(copy.Snapshot.LastError, Does.Contain("UNSAVED"));
        }

        [Test]
        public void FailedSecondInspectionRetainsFirstPickupAndContourFailureDiagnostics()
        {
            var run = Create();
            var secondInspect = run.Definition.Steps.FindIndex(
                step => step.Key == "second-inspect");
            Assert.That(secondInspect, Is.GreaterThan(0));
            for (var stage = 0; stage < secondInspect; stage++)
            {
                Pass(run, stage, run.Definition.Steps[stage].Key == "first-inspect"
                    ? "diameterMm=7.010; offsetXmm=0.432; offsetYmm=-0.212"
                    : "Verified machine/vision step");
            }
            run.BlockActiveStep(
                "Otsu=128; bright contours=16; radius-pass=2; circularity-pass=0; " +
                "D=7.10mm round=0.53 aspect=1.03; disc returned; vacuum OFF; safe height.",
                Began.AddMinutes(2));
            var record = MachineOperationRunProjection.Project(
                run.Snapshot, run.Definition, Org, Operator);
            var copy = JsonConvert.DeserializeObject<MachineOperationRunRecord>(
                JsonConvert.SerializeObject(record));

            Assert.That(copy.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Blocked));
            Assert.That(copy.Snapshot.ActiveStepKey, Is.EqualTo("second-inspect"));
            Assert.That(copy.Snapshot.Steps
                .Single(s => s.Key == "first-inspect").Evidence.Single().Value,
                Does.Contain("diameterMm=7.010"));
            Assert.That(copy.Snapshot.LastError, Does.Contain("circularity-pass=0"));
            Assert.That(copy.Snapshot.LastError, Does.Contain("vacuum OFF"));
            Assert.That(copy.Snapshot.Steps.Last().Evidence.Count, Is.Zero);
            Assert.That(copy.Snapshot.Steps.SelectMany(s => s.Evidence)
                .Any(e => e.Saved), Is.False);
        }
    }
}
