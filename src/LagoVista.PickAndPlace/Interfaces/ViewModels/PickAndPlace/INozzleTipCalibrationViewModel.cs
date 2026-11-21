// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 46b50cbd1c1a85354a4e0fcea8d90709665198e28a685de04d99d5b4a27493f1
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface INozzleTipCalibrationViewModel
    {
        void CircleLocation(Point2D<double> center, double diameter, Point2D<double> stdDeviation);
        Task StartAsync();
        Task PreviewBottomCameraAsync();
        Task ApplyRunoutVisionPreviewAsync();
        Task SaveRunoutVisionAsync();
        void RevertRunoutVision();
        IMachine RunoutMachine { get; }
        FiducialCircleObservation RunoutVideoObservation { get; }
        NozzleRunoutVisionTuning RunoutVisionTuning { get; }
        bool CanStartRunout { get; }
        bool CanTuneRunoutVision { get; }
        bool RunoutPreviewBusy { get; }
        bool HasRunoutVisionDraft { get; }
        bool HasRunoutVisionPreview { get; }
        string RunoutCameraStatus { get; }
        string RunoutVisionStatus { get; }
        string RunoutSearchSummary { get; }
        IReadOnlyList<MachineToolHead> AvailableRunoutHeads { get; }
        MachineToolHead SelectedRunoutHead { get; set; }
        ObservableCollection<string> CalibrationUpdates { get; }
        int TargetAngle { get; }
        int CurrentCalibrationAngle { get; }
        string Status { get; }
        string ToolHeadName { get; }
        string NozzleName { get; }
        bool RunoutRunning { get; }
        bool RunoutFinished { get; }
        ObservableCollection<FeederCalibrationFlowStep> RunoutSteps { get; }
        NozzleRunoutResult CandidateResult { get; }
        string CandidateZero { get; }
        string RunoutSummary { get; }
    }
}
