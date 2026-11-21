using LagoVista.Core.Models;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>Operator-visible internal stage of one calibration procedure.</summary>
    public sealed class ExposureCalibrationStep : ModelBase
    {
        public string Name { get; set; }
        private string _status = "Pending";
        private string _detail;
        private string _duration;
        public string Status { get => _status; set => Set(ref _status, value); }
        public string Detail { get => _detail; set => Set(ref _detail, value); }
        public string Duration { get => _duration; set => Set(ref _duration, value); }
    }
}
