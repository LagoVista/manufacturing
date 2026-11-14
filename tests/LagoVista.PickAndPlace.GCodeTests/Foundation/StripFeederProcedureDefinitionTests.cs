using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class StripFeederProcedureDefinitionTests
    {
        [Test]
        public void NewVersionRequiresVisionProofForBothPocketSteps()
        {
            var definition = StripFeederProcedureDefinition.Create();
            Assert.That(definition.Version, Is.EqualTo(2));
            Assert.That(definition.Steps.Count, Is.EqualTo(7));
            Assert.That(definition.Steps.Single(s => s.Key == "verify-current-part").RequiredVerification,
                Is.EqualTo(MachineOperationVerificationKind.VisionVerified));
            Assert.That(definition.Steps.Single(s => s.Key == "verify-previous-empty").RequiredVerification,
                Is.EqualTo(MachineOperationVerificationKind.VisionVerified));
            Assert.That(definition.Steps.Single(s => s.Key == "save-origin").RequiredVerification,
                Is.EqualTo(MachineOperationVerificationKind.OperatorConfirmed));
        }

        [Test]
        public void V1PublishedDefinitionRemainsUntouched()
        {
            var version1 = StripFeederCalibrationDefinition.Create();
            Assert.That(version1.Version, Is.EqualTo(1));
            Assert.That(version1.Steps.Single(s => s.Key == "verify-current-part").RequiredVerification,
                Is.EqualTo(MachineOperationVerificationKind.OperatorConfirmed));
        }
    }
}
