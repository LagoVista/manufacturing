using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    /// <summary>Independent T003 procedure, modeled after nozzle-runout modal.</summary>
    public interface ICameraToolPickupCalibrationViewModel
    {
        Task StartAsync();
        ObservableCollection<FeederCalibrationFlowStep> CalibrationSteps { get; }
        bool CalibrationRunning { get; }
        bool CalibrationFinished { get; }
        bool SaveVerifiedOffset { get; set; }
        string PickupZText { get; set; }
        string ReturnZText { get; set; }
        string HeldPartInspectionZText { get; set; }
        string HeadIndexText { get; set; }
        string Status { get; }
        string CandidateSummary { get; }
        string ToolHeadName { get; }
    }
}
