using LagoVista.Core.Models;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// T009's own stages use the exact shared T008 step-rail contract.
    /// </summary>
    public sealed class FiducialCalibrationStep : ModelBase, ICalibrationProcedureStep
    {
        private string _status = "Pending";
        private string _detail;
        private string _duration;
        public string Name { get; set; }
        public string Status { get => _status; set => Set(ref _status, value); }
        public string Detail { get => _detail; set => Set(ref _detail, value); }
        public string Duration { get => _duration; set => Set(ref _duration, value); }
    }
}
