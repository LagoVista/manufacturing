using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class NozzleRunoutProcedureDefinitionTests
    {
        [Test]
        public void DefinesOrderedMachineAndOpticalEvidenceStages()
        {
            var procedure=NozzleRunoutProcedureDefinition.Create();
            Assert.That(procedure.Key,Is.EqualTo("nozzle-rotation-runout"));
            Assert.That(procedure.Steps.Count,Is.EqualTo(15));
            Assert.That(procedure.Steps[0].Key,Is.EqualTo("position"));
            Assert.That(procedure.Steps.Skip(1).Take(12).Select(s=>s.Key),
                Is.EqualTo(NozzleRunoutMath.SweepAngles().Select(x=>"angle-"+x)));
            Assert.That(procedure.Steps[13].Key,Is.EqualTo("high-point"));
            Assert.That(procedure.Steps[14].Key,Is.EqualTo("result"));
            Assert.That(procedure.Steps[0].RequiredVerification,
                Is.EqualTo(MachineOperationVerificationKind.MachineObserved));
            Assert.That(procedure.Steps.Skip(1).All(s=>
                s.RequiredVerification==MachineOperationVerificationKind.VisionVerified),Is.True);
            Assert.That(procedure.Steps.Any(s=>s.RequiresSavedEvidence),Is.False);
        }

        [Test]
        public void CannotCompleteAnAngleWithoutFreshVisionEvidence()
        {
            var s=new MachineOperationSession(NozzleRunoutProcedureDefinition.Create(),
                new MachineOperationTarget{MachineId="test-machine"},
                DateTimeOffset.UtcNow);
            s.Start(DateTimeOffset.UtcNow);
            s.RecordEvidence(new MachineOperationEvidence{
                RunId=s.Snapshot.RunId,StepKey="position",Name="Safe camera position",
                Verification=MachineOperationVerificationKind.MachineObserved,
                CapturedAtUtc=DateTimeOffset.UtcNow});
            s.CompleteActiveStep("position",DateTimeOffset.UtcNow);
            Assert.Throws<InvalidOperationException>(()=>
                s.CompleteActiveStep("angle-0",DateTimeOffset.UtcNow));
        }
    }
}
