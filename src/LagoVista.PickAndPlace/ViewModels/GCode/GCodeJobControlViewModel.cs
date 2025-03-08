using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;

namespace LagoVista.PickAndPlace.ViewModels.GCode
{
    public partial class GCodeJobControlViewModel : MachineViewModelBase
    {
        IMachineRepo _machineRepo;

        public GCodeJobControlViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));

            SendGCodeFileCommand = new RelayCommand(SendGCodeFile, CanSendGcodeFile);
            StartProbeCommand = new RelayCommand(StartProbe, CanProbe);
            StartProbeHeightMapCommand = new RelayCommand(StartHeightMap, CanProbeHeightMap);
            StopCommand = new RelayCommand(StopJob, CanStopJob);
            PauseCommand = new RelayCommand(PauseJob, CanPauseJob);

            HomingCycleCommand = new RelayCommand(HomingCycle, CanHomeAndReset);
            HomeViaOriginCommand = new RelayCommand(HomeViaOrigin, CanHomeAndReset);
            SetAbsoluteWorkSpaceHomeCommand = new RelayCommand(SetAbsoluteWorkSpaceHome, CanMoveToWorkspaceHome); ;
            SoftResetCommand = new RelayCommand(SoftReset, CanHomeAndReset);
            FeedHoldCommand = new RelayCommand(FeedHold, CanPauseFeed);
            CycleStartCommand = new RelayCommand(CycleStart, CanResumeFeed);

            ClearAlarmCommand = new RelayCommand(ClearAlarm, CanClearAlarm);

            ConnectCommand = new RelayCommand(Connect, CanChangeConnectionStatus);

            EmergencyStopCommand = new RelayCommand(EmergencyStop, CanSendEmergencyStop);

            LaserOnCommand = new RelayCommand(LaserOn, CanManipulateLaser);
            LaserOffCommand = new RelayCommand(LaserOff, CanManipulateLaser);

            SpindleOnCommand = new RelayCommand(SpindleOn, CanManipulateSpindle);
            SpindleOffCommand = new RelayCommand(SpindleOff, CanManipulateSpindle);

            GotoFavorite1Command = new RelayCommand(GotoFavorite1, CanMove);
            GotoFavorite2Command = new RelayCommand(GotoFavorite2, CanMove);
            SetFavorite1Command = new RelayCommand(SetFavorite1, CanMove);
            SetFavorite2Command = new RelayCommand(SetFavorite2, CanMove);

            GotoWorkspaceHomeCommand = new RelayCommand(GotoWorkspaceHome, CanMove);
            SetWorkspaceHomeCommand = new RelayCommand(SetWorkspaceHome, CanMove);

            _machineRepo.CurrentMachine.PropertyChanged += _machine_PropertyChanged;
            //Machine.Settings.PropertyChanged += _machine_PropertyChanged;
            _machineRepo.CurrentMachine.HeightMapManager.PropertyChanged += HeightMapManager_PropertyChanged;
            _machineRepo.CurrentMachine.GCodeFileManager.PropertyChanged += GCodeFileManager_PropertyChanged;
        }

        public GCodeJobControlViewModel() : base(null)
        {

        }
    }
}
