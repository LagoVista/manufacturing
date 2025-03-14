using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.GCode;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using System;

namespace LagoVista.PickAndPlace.ViewModels.GCode
{
    public  class GCodeJobControlViewModel : MachineViewModelBase, IGCodeJobControlViewModel
    {
        public GCodeJobControlViewModel(IMachineRepo machineRepo, IProbingManager probingManager, IHeightMapManager heightMapManager, IGCodeFileManager gcodeFileManager) : base(machineRepo)
        {
            GCodeFileManager = gcodeFileManager ?? throw new ArgumentNullException(nameof(gcodeFileManager));
            HeightMapManager = heightMapManager ?? throw new ArgumentNullException(nameof(heightMapManager));
            ProbingManager = probingManager ?? throw new ArgumentNullException(nameof(probingManager));

            StopCommand = new RelayCommand(StopJob, CanStopJob);
            PauseCommand = new RelayCommand(PauseJob, CanPauseJob);

            HomingCycleCommand = new RelayCommand(MachineRepo.CurrentMachine.HomingCycle, CanHomeAndReset);
            HomeViaOriginCommand = new RelayCommand(MachineRepo.CurrentMachine.HomeViaOrigin, CanHomeAndReset);
            SetAbsoluteWorkSpaceHomeCommand = new RelayCommand(SetAbsoluteWorkSpaceHome, CanMoveToWorkspaceHome); ;
            SoftResetCommand = new RelayCommand(MachineRepo.CurrentMachine.SoftReset, CanHomeAndReset);
            FeedHoldCommand = new RelayCommand(MachineRepo.CurrentMachine.FeedHold, CanPauseFeed);
            CycleStartCommand = new RelayCommand(MachineRepo.CurrentMachine.CycleStart, CanResumeFeed);

            ClearAlarmCommand = new RelayCommand(MachineRepo.CurrentMachine.ClearAlarm, CanClearAlarm);

            EmergencyStopCommand = new RelayCommand(MachineRepo.CurrentMachine.EmergencyStop, CanSendEmergencyStop);

            LaserOnCommand = new RelayCommand(MachineRepo.CurrentMachine.LaserOn, CanManipulateLaser);
            LaserOffCommand = new RelayCommand(MachineRepo.CurrentMachine.LaserOff, CanManipulateLaser);

            SpindleOnCommand = new RelayCommand(MachineRepo.CurrentMachine.SpindleOn, CanManipulateSpindle);
            SpindleOffCommand = new RelayCommand(MachineRepo.CurrentMachine.SpindleOff, CanManipulateSpindle);

            GotoFavorite1Command = new RelayCommand(MachineRepo.CurrentMachine.GotoFavorite1, CanMove);
            GotoFavorite2Command = new RelayCommand(MachineRepo.CurrentMachine.GotoFavorite2, CanMove);
            SetFavorite1Command = new RelayCommand(MachineRepo.CurrentMachine.SetFavorite1, CanMove);
            SetFavorite2Command = new RelayCommand(MachineRepo.CurrentMachine.SetFavorite2, CanMove);

            GotoWorkspaceHomeCommand = new RelayCommand(MachineRepo.CurrentMachine.GotoWorkspaceHome, CanMove);
            SetWorkspaceHomeCommand = new RelayCommand(MachineRepo.CurrentMachine.SetAbsoluteWorkSpaceHome, CanMove);


            MachineRepo.MachineChanged += (s, a) => MachineRepo.CurrentMachine.PropertyChanged += _machine_PropertyChanged;
        }


        public bool IsCreatingHeightMap { get { return MachineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeightMap; } }
        public bool IsProbingHeight { get { return MachineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeight; } }
        public bool IsRunningJob { get { return MachineRepo.CurrentMachine.Mode == OperatingMode.SendingGCodeFile; } }

        private void GCodeFileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GCodeFileManager.HasValidFile))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        private void RefreshCommandExecuteStatus()
        {
            ClearAlarmCommand.RaiseCanExecuteChanged();

            StopCommand.RaiseCanExecuteChanged();
            PauseCommand.RaiseCanExecuteChanged();

            SoftResetCommand.RaiseCanExecuteChanged();
            HomingCycleCommand.RaiseCanExecuteChanged();
            HomeViaOriginCommand.RaiseCanExecuteChanged();
            FeedHoldCommand.RaiseCanExecuteChanged();
            CycleStartCommand.RaiseCanExecuteChanged();

            EmergencyStopCommand.RaiseCanExecuteChanged();

            LaserOnCommand.RaiseCanExecuteChanged();
            LaserOffCommand.RaiseCanExecuteChanged();
            SpindleOnCommand.RaiseCanExecuteChanged();
            SpindleOffCommand.RaiseCanExecuteChanged();

            GotoFavorite1Command.RaiseCanExecuteChanged();
            GotoFavorite2Command.RaiseCanExecuteChanged();
            SetFavorite1Command.RaiseCanExecuteChanged();
            SetFavorite2Command.RaiseCanExecuteChanged();
            GotoWorkspaceHomeCommand.RaiseCanExecuteChanged();
            SetWorkspaceHomeCommand.RaiseCanExecuteChanged();
        }

        public void StopJob()
        {
            GCodeFileManager.ResetJob();
            Machine.SetMode(OperatingMode.Manual);
        }

        public void HomingCycle()
        {
            MachineRepo.CurrentMachine.HomingCycle();
        }

        public void HomeViaOrigin()
        {
            MachineRepo.CurrentMachine.HomeViaOrigin();
        }

        public void SetAbsoluteWorkSpaceHome()
        {
            MachineRepo.CurrentMachine.SetAbsoluteWorkSpaceHome();
        }


        public void PauseJob()
        {
            MachineRepo.CurrentMachine.SetMode(OperatingMode.Manual);
        }

        public void ClearAlarm()
        {
            MachineRepo.CurrentMachine.ClearAlarm();
        }



        public bool CanManipulateLaser()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                MachineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.Marlin_Laser &&
                MachineRepo.CurrentMachine.Connected &&
                MachineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanManipulateSpindle()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                MachineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.GRBL1_1 &&
                MachineRepo.CurrentMachine.Connected &&
                MachineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanMove()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                MachineRepo.CurrentMachine.Connected &&
                MachineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanMoveToWorkspaceHome()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
          MachineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.GRBL1_1 &&
          MachineRepo.CurrentMachine.Connected &&
          MachineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        private void HeightMapManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(HeightMapManager.HeightMap) ||
               e.PropertyName == nameof(HeightMapManager.HeightMap.Status))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        private void _machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MachineRepo.CurrentMachine.IsInitialized) ||
                e.PropertyName == nameof(MachineRepo.CurrentMachine.Mode) ||
                e.PropertyName == nameof(MachineRepo.CurrentMachine.Settings) ||
                e.PropertyName == nameof(MachineRepo.CurrentMachine.Status) ||
                e.PropertyName == nameof(MachineRepo.CurrentMachine.Connected) ||
                e.PropertyName == nameof(MachineRepo.CurrentMachine.Settings.CurrentSerialPort))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        public bool CanChangeConnectionStatus()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                (
                (MachineRepo.CurrentMachine.Settings.ConnectionType == ConnectionTypes.Serial_Port && MachineRepo.CurrentMachine.Settings.CurrentSerialPort != null && MachineRepo.CurrentMachine.Settings.CurrentSerialPort.Id != "empty")
                || (MachineRepo.CurrentMachine.Settings.ConnectionType == ConnectionTypes.Network && !String.IsNullOrEmpty(MachineRepo.CurrentMachine.Settings.IPAddress))
                || MachineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.SimulatedMachine);
        }

        public bool CanHomeAndReset()
        {
            return MachineRepo.CurrentMachine.IsInitialized && MachineRepo.CurrentMachine.Connected;
        }


        public bool CanClearAlarm()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                   MachineRepo.CurrentMachine.Connected &&
                   MachineRepo.CurrentMachine.Status.ToLower() == "alarm";
        }

        public bool FavoritesAvailable()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                MachineRepo.CurrentMachine.Connected
                && MachineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanPauseJob()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                MachineRepo.CurrentMachine.Mode == OperatingMode.SendingGCodeFile ||
                MachineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeightMap ||
                MachineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeight;
        }



        public bool CanProbe()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                MachineRepo.CurrentMachine.Connected
                && MachineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanPauseFeed()
        {
            return (MachineRepo.CurrentMachine.IsInitialized &&
                        MachineRepo.CurrentMachine.Connected &&
                        MachineRepo.CurrentMachine.Status != "Hold");
        }

        public bool CanResumeFeed()
        {
            return (MachineRepo.CurrentMachine.IsInitialized &&
                        MachineRepo.CurrentMachine.Connected &&
                        MachineRepo.CurrentMachine.Status == "Hold");
        }

        public bool CanStopJob()
        {
            return MachineRepo.CurrentMachine.IsInitialized &&
                MachineRepo.CurrentMachine.Mode == OperatingMode.SendingGCodeFile ||
                MachineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeightMap ||
                MachineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeight;
        }


        public bool CanSendEmergencyStop()
        {
            return MachineRepo.CurrentMachine.IsInitialized && MachineRepo.CurrentMachine.Connected;
        }


        public IProbingManager ProbingManager { get; }
        public IHeightMapManager HeightMapManager { get; }

        public IGCodeFileManager GCodeFileManager { get; }

        public RelayCommand StopCommand { get; private set; }

        public RelayCommand PauseCommand { get; private set; }


        public RelayCommand HomingCycleCommand { get; private set; }
        public RelayCommand HomeViaOriginCommand { get; private set; }
        public RelayCommand SetAbsoluteWorkSpaceHomeCommand { get; private set; }

        public RelayCommand SoftResetCommand { get; private set; }

        public RelayCommand CycleStartCommand { get; private set; }
        public RelayCommand FeedHoldCommand { get; private set; }


        
        public RelayCommand EmergencyStopCommand { get; private set; }

        public RelayCommand ClearAlarmCommand { get; private set; }

        public RelayCommand SetFavorite1Command { get; private set; }
        public RelayCommand SetFavorite2Command { get; private set; }
        public RelayCommand GotoFavorite1Command { get; private set; }
        public RelayCommand GotoFavorite2Command { get; private set; }

        public RelayCommand GotoWorkspaceHomeCommand { get; private set; }
        public RelayCommand SetWorkspaceHomeCommand { get; private set; }
        public RelayCommand LaserOnCommand { get; private set; }
        public RelayCommand LaserOffCommand { get; private set; }
        public RelayCommand SpindleOnCommand { get; private set; }
        public RelayCommand SpindleOffCommand { get; private set; }

        public RelayCommand ExhaustSolenoidCommand { get; private set; }

        public RelayCommand SuctionSolenoidCommand { get; private set; }

    }
}
