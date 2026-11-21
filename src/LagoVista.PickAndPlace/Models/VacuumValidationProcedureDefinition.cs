using LagoVista.Manufacturing.Models.Operations;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Operator-supervised proof of the two vacuum channels. Uses the
    /// already configured MachineFiducial only as an XY test location,
    /// with existing camera/tool offsets and each head's PickHeight.
    /// It performs no fiducial calibration or coordinate changes.
    /// </summary>
    public static class VacuumValidationProcedureDefinition
    {
        public static MachineOperationDefinition Create()
        {
            var definition = new MachineOperationDefinition
            {
                Key = "dual-head-vacuum-calibration",
                Version = 1,
                Title = "Dual-Head Vacuum Normalization and Validation"
            };
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key = "readiness",
                Title = "Confirm homed idle machine, safe Z and machine fiducial",
                Instructions = "Operator verifies configured test spot is safe, tool offsets and configured pick heights are known, machine is homed, and both pumps are off.",
                RequiredVerification = MachineOperationVerificationKind.OperatorConfirmed
            });
            foreach (var head in new[] { "left", "right" })
            {
                definition.Steps.Add(new MachineOperationStepDefinition
                {
                    Key = head + "-idle",
                    Title = head + ": observe baseline, pumps off",
                    Instructions = "Vented nozzle, fresh stable raw pressure and inactive-head baseline. Candidate 0 percent.",
                    RequiredVerification = MachineOperationVerificationKind.MachineObserved
                });
                definition.Steps.Add(new MachineOperationStepDefinition
                {
                    Key = head + "-open",
                    Title = head + ": verify pump with unobstructed nozzle",
                    Instructions = "BEFORE any head XY/Z positioning: enable only the selected head pump, wait for actual pressure to settle, verify a fresh stable open-nozzle pressure window and the inactive opposite channel. Controller pump acknowledgment is not pressure-stability proof.",
                    RequiredVerification = MachineOperationVerificationKind.MachineObserved
                });
                definition.Steps.Add(new MachineOperationStepDefinition
                {
                    Key = head + "-seal",
                    Title = head + ": seal nozzle on machine fiducial",
                    Instructions = "At safe Z, position the camera at the configured MachineFiducial XY, switch to the selected tool using existing offsets, then on operator request lower to configured PickHeight. Confirm the port is obstructed before capture; no fiducial calibration.",
                    RequiredVerification = MachineOperationVerificationKind.MachineObserved
                });
                definition.Steps.Add(new MachineOperationStepDefinition
                {
                    Key = head + "-release",
                    Title = head + ": retract, vent and verify idle return",
                    Instructions = "Return to safe Z, stop pump/valve; verify both sensors and opposite-head isolation.",
                    RequiredVerification = MachineOperationVerificationKind.MachineObserved
                });
            }
            definition.Steps.Add(new MachineOperationStepDefinition
            {
                Key = "both-safe",
                Title = "Verify shutdown and review unsaved candidate thresholds",
                Instructions = "Both heads at safe Z, both pumps/solenoids off; do not persist any calibration before explicit operator approval.",
                RequiredVerification = MachineOperationVerificationKind.MachineObserved
            });
            return definition;
        }
    }
}
