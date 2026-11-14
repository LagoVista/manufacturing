using LagoVista.Manufacturing.Models.Operations;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Reuses the canonical machine-operation container. Strings describe
    /// progress only; all movement/vision is strongly typed in the view model.
    /// </summary>
    public static class NozzleRunoutProcedureDefinition
    {
        public static MachineOperationDefinition Create()
        {
            var definition = new MachineOperationDefinition
            {
                Key="nozzle-rotation-runout",
                Version=1,
                Title="Nozzle Runout Calibration"
            };
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key="position", Title="Position over bottom camera",
                Instructions="Require homed Manual machine, safe Z and verified camera position.",
                RequiredVerification=MachineOperationVerificationKind.MachineObserved
            });

            foreach(var angle in NozzleRunoutMath.SweepAngles())
                definition.Steps.Add(new MachineOperationStepDefinition
                {
                    Key="angle-" + angle,
                    Title=$"Measure nozzle at {angle:000} degrees",
                    Instructions="Rotate the selected head and acquire a fresh stable bottom-camera circle.",
                    RequiredVerification=MachineOperationVerificationKind.VisionVerified
                });

            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key="high-point", Title="Verify maximum-Y rotational reference",
                Instructions="Revisit the highest sampled nozzle-tip Y; repeat within the accepted tolerance.",
                RequiredVerification=MachineOperationVerificationKind.VisionVerified
            });
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key="result", Title="Calculate unsaved logical-zero candidate",
                Instructions="Calculate runout compensation, but do not change firmware XY/rotary zero or placement.",
                RequiredVerification=MachineOperationVerificationKind.VisionVerified
            });
            return definition;
        }
    }
}
