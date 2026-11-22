using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Interfaces;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    /// <summary>Independent T003 procedure, modeled after nozzle-runout modal.</summary>
    public interface ICameraToolPickupCalibrationViewModel
    {
        Task StartAsync();
        IMachine CalibrationMachine { get; }
        ObservableCollection<FeederCalibrationFlowStep> CalibrationSteps { get; }
        bool CalibrationRunning { get; }
        bool CalibrationFinished { get; }
        bool SaveVerifiedOffset { get; set; }
        string HeadIndexText { get; set; }
        string ConfiguredHeights { get; }
        string Status { get; }
        string BottomVisionState { get; }
        string BottomVisionMeasurements { get; }
        string CandidateSummary { get; }
        string ToolHeadName { get; }
    }
}
