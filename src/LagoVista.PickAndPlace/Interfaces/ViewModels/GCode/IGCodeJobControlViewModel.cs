// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 532f78af1b1dbb3ad0fbe642880cb66ddc500dcad72cdb7b32e25dc6eae5298f
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.GCode
{
    public interface IGCodeJobControlViewModel : IMachineViewModelBase
    {
        RelayCommand ClearAlarmCommand { get; }
        RelayCommand CycleStartCommand { get; }
        RelayCommand EmergencyStopCommand { get; }
        RelayCommand ExhaustSolenoidCommand { get; }
        RelayCommand FeedHoldCommand { get; }
        RelayCommand GotoFavorite1Command { get; }
        RelayCommand GotoFavorite2Command { get; }
        RelayCommand GotoWorkspaceHomeCommand { get; }
        RelayCommand HomeViaOriginCommand { get; }
        RelayCommand HomingCycleCommand { get; }
        bool IsCreatingHeightMap { get; }
        bool IsProbingHeight { get; }
        bool IsRunningJob { get; }
        RelayCommand LaserOffCommand { get; }
        RelayCommand LaserOnCommand { get; }
        RelayCommand PauseCommand { get; }
        RelayCommand SetAbsoluteWorkSpaceHomeCommand { get; }
        RelayCommand SetFavorite1Command { get; }
        RelayCommand SetFavorite2Command { get; }
        RelayCommand SetWorkspaceHomeCommand { get; }
        RelayCommand SoftResetCommand { get; }
        RelayCommand SpindleOffCommand { get; }
        RelayCommand SpindleOnCommand { get; }
        RelayCommand StopCommand { get; }
        RelayCommand SuctionSolenoidCommand { get; }

        bool CanChangeConnectionStatus();
        bool CanClearAlarm();
        bool CanHomeAndReset();
        bool CanManipulateLaser();
        bool CanManipulateSpindle();
        bool CanMove();
        bool CanMoveToWorkspaceHome();
        bool CanPauseFeed();
        bool CanPauseJob();
        bool CanResumeFeed();
        bool CanSendEmergencyStop();
        bool CanStopJob();
        void ClearAlarm();
        bool FavoritesAvailable();
        void PauseJob();
        void SetAbsoluteWorkSpaceHome();

        void StopJob();

        IGCodeFileManager GCodeFileManager { get; }
        IProbingManager ProbingManager { get; }
        IHeightMapManager HeightMapManager { get; }
    }
}