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
        public static MachineOperationDefinition Create(bool includeFinalSafeHeight = false)
        {
            var definition = new MachineOperationDefinition
            {
                Key="nozzle-rotation-runout",
                Version=3,
                Title="Nozzle Runout Calibration"
            };
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key="readiness", Title="Verify machine fiducial and both staging plates",
                Instructions="Require this session's fiducial and two staging-plate calibrations.",
                RequiredVerification=MachineOperationVerificationKind.MachineObserved
            });
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key="camera", Title="Configure and start bottom-camera vision",
                Instructions="Automatically select Nozzle, enable circle detection and await fresh capture.",
                RequiredVerification=MachineOperationVerificationKind.MachineObserved
            });
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key="move-plan", Title="Verify tool and camera XY movement plan",
                Instructions="Read actual XY and reject unsafe tool offsets / camera destinations before motion.",
                RequiredVerification=MachineOperationVerificationKind.MachineObserved
            });
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key="position", Title="Position over bottom camera",
                Instructions="Require homed Manual machine, safe Z and verified camera position.",
                RequiredVerification=MachineOperationVerificationKind.MachineObserved
            });

            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key="tune-image", Title="Auto-tune and verify nozzle opening",
                Instructions="Analyze 24 fresh frames and enable a scale-verified dark-opening locator for this installed head.",
                RequiredVerification=MachineOperationVerificationKind.VisionVerified
            });
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key="center-nozzle", Title="Optically center nozzle over bottom camera",
                Instructions="Use bounded, camera-verified XY adjustments; require a fresh centered observation before rotation.",
                RequiredVerification=MachineOperationVerificationKind.VisionVerified
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
            if (includeFinalSafeHeight)
                definition.Steps.Add(new MachineOperationStepDefinition
                {
                    Key="safe-height", Title="Return to configured machine safe move height",
                    Instructions="Move to the existing machine DefaultSafeMoveHeight, verify controller Z, and stop. Do not capture or change the setting.",
                    RequiredVerification=MachineOperationVerificationKind.MachineObserved
                });
            return definition;
        }
    }
}
