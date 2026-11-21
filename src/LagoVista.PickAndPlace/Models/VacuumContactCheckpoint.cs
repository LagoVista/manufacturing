namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Deterministic permission for an already-configured nozzle descent:
    /// the operator acknowledges fixture/PickHeight safety at the start,
    /// then the runner verifies current machine, XY, safe Z and head.
    /// This does NOT replace the distinct post-descent sealed-nozzle
    /// observation required for sensor-based part-present evidence.
    /// </summary>
    public static class VacuumContactCheckpoint
    {
        public static bool CanDescend(int selectedHead, bool machineAcknowledged) =>
            machineAcknowledged && (selectedHead == 1 || selectedHead == 2);

        public static bool CanCaptureSealed(int selectedHead, int confirmedHead) =>
            (selectedHead == 1 || selectedHead == 2) &&
            selectedHead == confirmedHead;
    }
}
