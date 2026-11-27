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
        private static readonly EntityHeader Org = EntityHeader.Create("org-1", "Test Org");
        private static readonly EntityHeader Operator = EntityHeader.Create("person-1", "Operator");
        private static readonly DateTimeOffset Began =
            new DateTimeOffset(2026, 11, 22, 20, 0, 0, TimeSpan.Zero);

        private static MachineOperationSession NewRun()
        {
            var run = new MachineOperationSession(
                CameraToolPickupProcedureDefinition.Create(),
                new MachineOperationTarget { MachineId = "lumen-pnp", CameraId = "bottom-camera" },
                Began);
            run.Start(Began.AddSeconds(1));
            return run;
        }

        private static void Pass(MachineOperationSession run, int stage, string detail)
        {
            var spec = run.Definition.Steps[stage];
            var at = Began.AddSeconds(3 + stage * 3);
            run.RecordEvidence(new MachineOperationEvidence
            {
                RunId = run.Snapshot.RunId,
                StepKey = spec.Key,
                Verification = spec.RequiredVerification,
                CapturedAtUtc = at,
                Saved = false,
                Name = detail,
                Value = detail,
                Source = "T014.CameraToolPickupCalibrationViewModel",
                VisionProfileKey = spec.Key == "first-inspect" ||
                                   spec.Key == "second-inspect" ? "calibrationdisc" : null,
                PixelsPerMm = spec.Key == "first-inspect" ||
                              spec.Key == "second-inspect" ? 12.5 : (double?)null
            });
            run.CompleteActiveStep(spec.Key, at.AddSeconds(1));
        }

        [Test]
        public void MeasurementOnlyRunRetainsProofButNeverClaimsOffsetSaved()
        {
            var run = NewRun();
            for (var i = 0; i < run.Definition.Steps.Count - 1; i++)
                Pass(run, i, run.Definition.Steps[i].Key == "second-inspect"
                    ? "Second pickup residual X=0.021 Y=-0.015 mm"
                    : "Verified machine and optical evidence");
            run.BlockActiveStep("Both pickups verified; offset intentionally UNSAVED.",
                Began.AddMinutes(2));

            var record = MachineOperationRunProjection.Project(
                run.Snapshot, run.Definition, Org, Operator);
            var copy = JsonConvert.DeserializeObject<MachineOperationRunRecord>(
                JsonConvert.SerializeObject(record));

            Assert.That(copy.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Blocked));
            Assert.That(copy.Snapshot.ActiveStepKey, Is.EqualTo("commit-offset"));
            Assert.That(copy.Snapshot.Steps.Last().Status,
                Is.EqualTo(MachineOperationStepStatus.Blocked));
            Assert.That(copy.Snapshot.Steps.SelectMany(s => s.Evidence)
                .Any(e => e.Saved), Is.False);
            Assert.That(copy.Snapshot.Steps.Single(s => s.Key == "second-inspect")
                .Evidence.Single().Value, Does.Contain("X=0.021"));
            Assert.That(copy.Snapshot.LastError, Does.Contain("UNSAVED"));
        }

        [Test]
        public void FailedSecondInspectionKeepsFirstPickupAndDetectorRecoveryTrace()
        {
            var run = NewRun();
            var second = run.Definition.Steps.FindIndex(s => s.Key == "second-inspect");
            for (var i = 0; i < second; i++)
                Pass(run, i, run.Definition.Steps[i].Key == "first-inspect"
                    ? "diameterMm=7.010; offsetXmm=0.432; offsetYmm=-0.212"
                    : "Verified stage");
            run.BlockActiveStep(
                "Otsu=128; bright contours=16; radius-pass=2; circularity-pass=0; " +
                "D=7.10mm round=0.53 aspect=1.03; disc returned; vacuum OFF; safe Z.",
                Began.AddMinutes(2));

            var record = MachineOperationRunProjection.Project(
                run.Snapshot, run.Definition, Org, Operator);
            var copy = JsonConvert.DeserializeObject<MachineOperationRunRecord>(
                JsonConvert.SerializeObject(record));

            Assert.That(copy.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Blocked));
            Assert.That(copy.Snapshot.ActiveStepKey, Is.EqualTo("second-inspect"));
            Assert.That(copy.Snapshot.Steps.Single(s => s.Key == "first-inspect")
                .Evidence.Single().Value, Does.Contain("diameterMm=7.010"));
            Assert.That(copy.Snapshot.LastError, Does.Contain("circularity-pass=0"));
            Assert.That(copy.Snapshot.LastError, Does.Contain("vacuum OFF"));
            Assert.That(copy.Snapshot.Steps.SelectMany(s => s.Evidence)
                .Any(e => e.Saved), Is.False);
        }
    }
}
