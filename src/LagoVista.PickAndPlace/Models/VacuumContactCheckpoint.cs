namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Operator-observed sealed-nozzle proof: unlike the machine travel
    /// preflight, physical contact confirmation must be fresh and scoped
    /// to the specific installed head before collecting sealed pressure.
    /// The sensor remains independently authoritative afterward.
    /// </summary>
    public static class VacuumContactCheckpoint
    {
        public static bool CanCaptureSealed(int selectedHead, int confirmedHead) =>
            (selectedHead == 1 || selectedHead == 2) &&
            selectedHead == confirmedHead;
    }
}
