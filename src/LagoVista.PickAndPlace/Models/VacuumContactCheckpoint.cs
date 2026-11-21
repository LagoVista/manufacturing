namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Explicit, per-head operator clearance. A general machine-ready
    /// acknowledgement is NEVER permission to lower the nozzle into the
    /// fixture. Approval for Head 1 must not authorize Head 2.
    /// </summary>
    public static class VacuumContactCheckpoint
    {
        public static bool CanDescend(int selectedHead, int approvedHead,
            bool machineAcknowledged) =>
            machineAcknowledged && (selectedHead == 1 || selectedHead == 2) &&
            selectedHead == approvedHead;

        public static bool CanCaptureSealed(int selectedHead, int confirmedHead) =>
            (selectedHead == 1 || selectedHead == 2) &&
            selectedHead == confirmedHead;
    }
}
