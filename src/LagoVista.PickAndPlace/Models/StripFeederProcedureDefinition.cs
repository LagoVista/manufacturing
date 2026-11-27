using LagoVista.Manufacturing.Models.Operations;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Version 2 is the executable client-side procedure: pocket evidence
    /// is from processed rectangle frames, never an invented operator click.
    /// The original published v1 operation definition remains unchanged.
    /// The Manufacturing history API stores the supplied versioned contract.
    /// </summary>
    public enum StripFeederCalibrationScope
    {
        Full,
        FeederOrigin,
        InstalledTape
    }

    public static class StripFeederProcedureDefinition
    {
        public static MachineOperationDefinition CreateForScope(StripFeederCalibrationScope scope)
        {
            var definition = Create();
            if (scope == StripFeederCalibrationScope.Full)
                return definition;

            // Phase-specific definitions preserve the exact typed verification
            // rules. They share the existing runner and evidence projection,
            // but start at the appropriate step without replaying feeder
            // origin movements for every tape row.
            definition.Version = 3;
            if (scope == StripFeederCalibrationScope.FeederOrigin)
            {
                definition.Key = "strip-feeder-origin";
                definition.Title = "Locate and calibrate feeder origin";
                definition.Steps = definition.Steps.Take(3).ToList();
            }
            else
            {
                definition.Key = "strip-feeder-installed-tape";
                definition.Title = "Verify installed tape";
                definition.Steps = definition.Steps.Skip(3).ToList();
            }
            return definition;
        }

        public static MachineOperationDefinition Create()
        {
            var definition = StripFeederCalibrationDefinition.Create();
            definition.Version = 2;
            var present = definition.Steps.Single(s => s.Key == "verify-current-part");
            present.RequiredVerification = MachineOperationVerificationKind.VisionVerified;
            present.Instructions =
                "Require a fresh, stabilized centered component rectangle in the current pocket.";
            var empty = definition.Steps.Single(s => s.Key == "verify-previous-empty");
            empty.RequiredVerification = MachineOperationVerificationKind.VisionVerified;
            empty.Instructions =
                "Require sustained valid processed camera frames with no component rectangle in the preceding pocket.";
            return definition;
        }
    }
}
