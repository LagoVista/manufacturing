using LagoVista.Core.Commanding;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.GCode
{
    public interface IGCodeJobControlViewModel : IMachineViewModelBase
    {
        RelayCommand ClearAlarmCommand { get; }
        RelayCommand ConnectCommand { get; }
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
        RelayCommand SendGCodeFileCommand { get; }
        RelayCommand SetAbsoluteWorkSpaceHomeCommand { get; }
        RelayCommand SetFavorite1Command { get; }
        RelayCommand SetFavorite2Command { get; }
        RelayCommand SetWorkspaceHomeCommand { get; }
        RelayCommand SoftResetCommand { get; }
        RelayCommand SpindleOffCommand { get; }
        RelayCommand SpindleOnCommand { get; }
        RelayCommand StartProbeCommand { get; }
        RelayCommand StartProbeHeightMapCommand { get; }
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
        bool CanProbe();
        bool CanProbeHeightMap();
        bool CanResumeFeed();
        bool CanSendEmergencyStop();
        bool CanSendGcodeFile();
        bool CanStopJob();
        void ClearAlarm();
        void Connect();
        void CycleStart();
        void EmergencyStop();
        bool FavoritesAvailable();
        void FeedHold();
        void GotoFavorite1();
        void GotoFavorite2();
        void GotoWorkspaceHome();
        void HomeViaOrigin();
        void HomingCycle();
        void LaserOff();
        void LaserOn();
        void PauseJob();
        void SendGCodeFile();
        void SetAbsoluteWorkSpaceHome();
        void SetFavorite1();
        void SetFavorite2();
        void SetWorkspaceHome();
        void SoftReset();
        void SpindleOff();
        void SpindleOn();
        void StartHeightMap();
        void StartProbe();
        void StopJob();
    }
}