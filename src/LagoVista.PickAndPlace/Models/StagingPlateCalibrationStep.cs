using LagoVista.Core.Models;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>Two-hole staging procedure stage; shares T008's live step rail.</summary>
    public sealed class StagingPlateCalibrationStep : ModelBase, ICalibrationProcedureStep
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
