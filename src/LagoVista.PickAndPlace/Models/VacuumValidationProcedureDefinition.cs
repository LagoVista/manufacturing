using LagoVista.Manufacturing.Models.Operations;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Ordered operator-supervised proof steps, not a movement script.
    /// The concrete runner must validate safe Z and per-head pick-height
    /// before approaching the configured MachineFiducial. Never infer a
    /// contact height from the current coordinate or modify machine settings.
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
                Instructions = "Operator verifies clear nozzle, configured MachineFiducial on staging plate, known gentle contact height, and both pumps off.",
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
                    Instructions = "Only the selected head pump/valve may operate. Observe both raw channels and verify no false part classification.",
                    RequiredVerification = MachineOperationVerificationKind.MachineObserved
                });
                definition.Steps.Add(new MachineOperationStepDefinition
                {
                    Key = head + "-seal",
                    Title = head + ": seal nozzle on machine fiducial",
                    Instructions = "At safe Z, travel to configured MachineFiducial, then with operator confirmation and bounded pick height gently obscure nozzle port. Capture candidate 100 percent.",
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
