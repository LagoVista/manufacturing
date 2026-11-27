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
        Task PreviewBottomCameraAsync();
        Task RetryHistoryAsync();
        Task SaveAndVerifyAsync();
        bool FullDiagnostics { get; set; }
        string DiagnosticIterations { get; set; }
        string SharedCameraStatus { get; }
        Task ProposeSharedCameraAsync();
        Task ApproveSharedCameraAsync();
        Task ValidateSharedCameraAsync();
        Task RunFullDiagnosticsAsync();
        bool SharedOperationActive { get; }
        bool SharedCameraVerified { get; }
        bool SharedCandidateReady { get; }
        void RequestDiagnosticStop();
        bool CanSaveAndVerify { get; }
        bool SavedPickupVerified { get; }
        bool ShowSavedPickupRing { get; }
        CameraToolSavedPickupProof SavedPickupProof { get; }
        string SavedPickupStatus { get; }
        bool CanRetryHistory { get; }
        bool HistoryBusy { get; }
        string HistoryStatus { get; }
        string HistoryRunId { get; }
        string LocalHistoryPath { get; }
        string DiagnosticReport { get; }
        bool PreviewBusy { get; }
        bool CanOperate { get; }
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
