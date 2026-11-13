using LagoVista.Manufacturing.Models.Operations;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Pure, conservative transition policy for supervised feeder calibration.
    /// Auto mode may move and acquire fresh vision evidence and persist
    /// validated feeder offsets after the operator starts Guided Sequence.
    /// It never confirms presence/absence of a part for the operator.
    /// </summary>
    public static class FeederCalibrationAutoAdvancePolicy
    {
        // An M400 acknowledgement means motion finished, but the serial
        // pipeline's last acknowledgement can still be in flight.
        public static bool SerialChannelQuiet(bool connected,
            bool busy, int toSend, int unacknowledged) =>
            connected && !busy && toSend == 0 && unacknowledged == 0;

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
                    // After approach and fresh centering, complete the verified
                    // paired-offset save within the same guided run.
                case "save-origin":
                    // Auto save still performs a NEW centered vision observation
                    // and validates machine location before persisting.
                    return true;
                case "verify-current-part":
                case "verify-previous-empty":
                    // Move camera only; operator must confirm the live image.
                    return !pocketApproached;
                default:
                    // Part presence and unknown operations remain manual.
                    return false;
            }
        }
    }
}
