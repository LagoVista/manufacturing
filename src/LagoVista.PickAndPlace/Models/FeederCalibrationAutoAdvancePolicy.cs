using LagoVista.Manufacturing.Models.Operations;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Pure, conservative transition policy for supervised feeder calibration.
    /// Auto mode may move and acquire fresh vision evidence, but it cannot
    /// confirm a part or commit an offset on the operator's behalf.
    /// </summary>
    public static class FeederCalibrationAutoAdvancePolicy
    {
        public static bool CanAdvance(
            MachineOperationRunStatus status, string activeStepKey,
            bool holeApproached, bool pocketApproached)
        {
            if (status == MachineOperationRunStatus.Pending)
                return true;
            if (status != MachineOperationRunStatus.Running)
                return false;

            switch (activeStepKey)
            {
                case "approach-origin":
                case "center-fiducial":
                case "first-tape-hole":
                    // First-hole capture is measurement only; no saved mutation.
                    return true;
                case "last-tape-hole":
                    // Move automatically, but stop before paired offset save.
                    return !holeApproached;
                case "verify-current-part":
                case "verify-previous-empty":
                    // Move camera only; operator must confirm the live image.
                    return !pocketApproached;
                default:
                    // Includes "save-origin": never commit without confirmation.
                    return false;
            }
        }
    }
}
