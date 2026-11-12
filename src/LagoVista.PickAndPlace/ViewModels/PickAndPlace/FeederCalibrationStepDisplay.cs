namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    /// <summary>
    /// Read-only WPF projection of versioned calibration step data.
    /// Motion is owned by the strongly typed runner, never by this model.
    /// </summary>
    public sealed class FeederCalibrationStepDisplay
    {
        public int Number { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public string Evidence { get; set; }
        public bool IsActive { get; set; }
    }
}
