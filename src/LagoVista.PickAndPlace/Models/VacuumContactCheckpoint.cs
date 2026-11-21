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
            CanCaptureSealed(selectedHead, confirmedHead, false);

        /// <summary>
        /// Restore T007 automatic sampling after the verified PickHeight
        /// move. This authorizes SAMPLE ACQUISITION, not a claim of contact:
        /// the stable sensor band must still pass independent response,
        /// separation and classification validation. Guided operation
        /// continues to require head-specific visual confirmation.
        /// </summary>
        public static bool CanCaptureSealed(int selectedHead, int confirmedHead,
            bool automatic) =>
            (selectedHead == 1 || selectedHead == 2) &&
            (automatic || selectedHead == confirmedHead);
    }
}
