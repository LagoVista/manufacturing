using System.Collections.Generic;

namespace LagoVista.Manufacturing.Models.Operations
{
    /// <summary>
    /// Presentation metadata for the first concrete operation. No G-code, motion,
    /// geometry or vision implementation is driven by these strings.
    /// </summary>
    public static class StripFeederCalibrationDefinition
    {
        public const string OperationKey = "strip-feeder-calibration";

        public static MachineOperationDefinition Create()
        {
            return new MachineOperationDefinition
            {
                Key = OperationKey,
                Version = 1,
                Title = "Strip Feeder Calibration",
                Steps = new List<MachineOperationStepDefinition>
                {
                    new MachineOperationStepDefinition
                    {
                        Key = "approach-origin",
                        Title = "Move to calculated feeder origin",
                        Instructions = "Approach the staging-plate row/column origin without the saved feeder offset.",
                        RequiredVerification = MachineOperationVerificationKind.MachineObserved
                    },
                    new MachineOperationStepDefinition
                    {
                        Key = "center-fiducial",
                        Title = "Center optical fiducial",
                        Instructions = "Jog to the white-cut corner; require fresh, stable and centered vision evidence.",
                        RequiredVerification = MachineOperationVerificationKind.VisionVerified
                    },
                    new MachineOperationStepDefinition
                    {
                        Key = "save-origin",
                        Title = "Capture and save feeder origin offset",
                        Instructions = "Confirm actual machine XY and persist the verified OriginOffset.",
                        RequiredVerification = MachineOperationVerificationKind.OperatorConfirmed,
                        RequiresSavedEvidence = true
                    },
                    new MachineOperationStepDefinition
                    {
                        Key = "first-tape-hole",
                        Title = "Locate first tape hole",
                        Instructions = "Center and measure the first applicable tape reference hole.",
                        RequiredVerification = MachineOperationVerificationKind.VisionVerified
                    },
                    new MachineOperationStepDefinition
                    {
                        Key = "last-tape-hole",
                        Title = "Locate last tape hole",
                        Instructions = "Center and measure the last hole, then save the paired first/last offsets.",
                        RequiredVerification = MachineOperationVerificationKind.VisionVerified,
                        RequiresSavedEvidence = true
                    },
                    new MachineOperationStepDefinition
                    {
                        Key = "verify-current-part",
                        Title = "Verify current part is present",
                        Instructions = "Inspect the current pocket without picking and explicitly confirm a part is present.",
                        RequiredVerification = MachineOperationVerificationKind.OperatorConfirmed
                    },
                    new MachineOperationStepDefinition
                    {
                        Key = "verify-previous-empty",
                        Title = "Verify preceding pocket is empty",
                        Instructions = "Inspect index minus one without changing inventory; confirm that pocket is empty.",
                        RequiredVerification = MachineOperationVerificationKind.OperatorConfirmed
                    }
                }
            };
        }
    }
}
