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
            Assert.That(procedure.Steps.Count,Is.EqualTo(20));
            Assert.That(procedure.Steps.Take(4).Select(s=>s.Key),
                Is.EqualTo(new[]{"readiness","camera","move-plan","position"}));
            Assert.That(procedure.Steps.Skip(4).Take(2).Select(s=>s.Key),
                Is.EqualTo(new[]{"tune-image","center-nozzle"}));
            Assert.That(procedure.Steps.Skip(6).Take(12).Select(s=>s.Key),
                Is.EqualTo(NozzleRunoutMath.SweepAngles().Select(x=>"angle-"+x)));
            Assert.That(procedure.Steps[18].Key,Is.EqualTo("high-point"));
            Assert.That(procedure.Steps[19].Key,Is.EqualTo("result"));
            Assert.That(procedure.Steps.Take(4).All(s=>
                s.RequiredVerification==MachineOperationVerificationKind.MachineObserved),Is.True);
            Assert.That(procedure.Steps.Skip(4).All(s=>
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
            foreach (var key in new[]{"readiness","camera","move-plan","position"})
            {
                s.RecordEvidence(new MachineOperationEvidence{
                    RunId=s.Snapshot.RunId,StepKey=key,Name="Verified " + key,
                    Verification=MachineOperationVerificationKind.MachineObserved,
                    CapturedAtUtc=DateTimeOffset.UtcNow});
                s.CompleteActiveStep(key,DateTimeOffset.UtcNow);
            }
            foreach(var key in new[]{"tune-image","center-nozzle"})
            {
                s.RecordEvidence(new MachineOperationEvidence{
                    RunId=s.Snapshot.RunId,StepKey=key,Name="Verified " + key,
                    Verification=MachineOperationVerificationKind.VisionVerified,
                    CapturedAtUtc=DateTimeOffset.UtcNow});
                s.CompleteActiveStep(key,DateTimeOffset.UtcNow);
            }
            Assert.Throws<InvalidOperationException>(()=>
                s.CompleteActiveStep("angle-0",DateTimeOffset.UtcNow));
        }
    }
}
