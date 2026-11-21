using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public sealed class VacuumStepRailProjectionTests
    {
        [TestCase(MachineOperationStepStatus.Pending, "Queued")]
        [TestCase(MachineOperationStepStatus.Active, "Running")]
        [TestCase(MachineOperationStepStatus.Passed, "Passed")]
        [TestCase(MachineOperationStepStatus.Blocked, "Failed")]
        [TestCase(MachineOperationStepStatus.Cancelled, "Cancelled")]
        public void UsesActualEvidenceBasedMachineOperationStatus(
            MachineOperationStepStatus source, string expected)
        {
            Assert.That(VacuumStepRailProjection.FromOperationStatus(source),
                Is.EqualTo(expected));
        }

        [Test]
        public void NoEvidenceMeansProgressCannotAppearPassed()
        {
            var session = new MachineOperationSession(
                VacuumValidationProcedureDefinition.Create(),
                new MachineOperationTarget { MachineId = "machine" },
                System.DateTimeOffset.UtcNow);
            session.Start(System.DateTimeOffset.UtcNow);
            Assert.That(VacuumStepRailProjection.FromOperationStatus(
                session.Snapshot.Steps[0].Status), Is.EqualTo("Running"));
            Assert.That(VacuumStepRailProjection.FromOperationStatus(
                session.Snapshot.Steps[1].Status), Is.EqualTo("Queued"));
        }
    }
}
