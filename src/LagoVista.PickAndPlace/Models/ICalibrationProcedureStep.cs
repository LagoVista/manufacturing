using System.ComponentModel;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Minimal shared contract for a live, independently executable
    /// calibration procedure step (including future T009/T003 steps).
    /// </summary>
    public interface ICalibrationProcedureStep : INotifyPropertyChanged
    {
        string Status { get; }
    }
}
