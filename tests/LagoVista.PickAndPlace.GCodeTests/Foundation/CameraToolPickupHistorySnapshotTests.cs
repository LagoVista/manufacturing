using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    [TestFixture]
    public class CameraToolPickupHistorySnapshotTests
    {
        [Test]
        public void UnSavedT014MeasurementUsesHistoryBlockNotSavedCompletion()
        {
            var now = new DateTimeOffset(2026, 11, 22, 20, 0, 0, TimeSpan.Zero);
            var run = new MachineOperationSession(
                CameraToolPickupProcedureDefinition.Create(),
                new MachineOperationTarget { MachineId = "lumen-pnp" }, now);
            run.Start(now.AddSeconds(1));
            run.BlockActiveStep(
                "Calibration trial blocked by failed optical proof; offset unsaved.",
                now.AddSeconds(2));
            var record = MachineOperationRunProjection.Project(run.Snapshot, run.Definition,
                EntityHeader.Create("org1", "Test Org"),
                EntityHeader.Create("operator1", "Test Operator"));

            Assert.That(record.Snapshot.Status, Is.EqualTo(MachineOperationRunStatus.Blocked));
            Assert.That(record.Snapshot.ActiveStepKey, Is.EqualTo("readiness"));
            Assert.That(record.Snapshot.LastError, Does.Contain("offset unsaved"));
        }
    }
}
