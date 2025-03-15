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

            StopCommand = new RelayCommand(() => StopJob(), CanStopJob);
            PauseCommand = new RelayCommand(() => PauseJob(), CanPauseJob);

            HomingCycleCommand = new RelayCommand(() => Machine.HomingCycle(), CanHomeAndReset);
            HomeViaOriginCommand = new RelayCommand(Machine.HomeViaOrigin, CanHomeAndReset);
            SetAbsoluteWorkSpaceHomeCommand = new RelayCommand(SetAbsoluteWorkSpaceHome, CanMoveToWorkspaceHome); ;
            SoftResetCommand = new RelayCommand(() => Machine.SoftReset(), CanHomeAndReset);
            FeedHoldCommand = new RelayCommand(() => Machine.FeedHold(), CanPauseFeed);
            CycleStartCommand = new RelayCommand(() => Machine.CycleStart(), CanResumeFeed);

            ClearAlarmCommand = new RelayCommand(() => Machine.ClearAlarm(), CanClearAlarm);

            EmergencyStopCommand = new RelayCommand(() => Machine.EmergencyStop(), CanSendEmergencyStop);

            LaserOnCommand = new RelayCommand(() => Machine.LaserOn(), CanManipulateLaser);
            LaserOffCommand = new RelayCommand(() => Machine.LaserOff(), CanManipulateLaser);

            SpindleOnCommand = new RelayCommand(() => Machine.SpindleOn(), CanManipulateSpindle);
            SpindleOffCommand = new RelayCommand(() => Machine.SpindleOff(), CanManipulateSpindle);

            GotoFavorite1Command = new RelayCommand(() => Machine.GotoFavorite1(), CanMove);
            GotoFavorite2Command = new RelayCommand(() => Machine.GotoFavorite2(), CanMove);
            SetFavorite1Command = new RelayCommand(() => Machine.SetFavorite1(), CanMove);
            SetFavorite2Command = new RelayCommand(() => Machine.SetFavorite2(), CanMove);

            GotoWorkspaceHomeCommand = new RelayCommand(() => Machine.GotoWorkspaceHome(), CanMove);
            SetWorkspaceHomeCommand = new RelayCommand(() => Machine.SetAbsoluteWorkSpaceHome(), CanMove);

            HeightMapManager.PropertyChanged += (e, a) => RefreshCommandExecuteStatus();
            ProbingManager.PropertyChanged += (e, a) => RefreshCommandExecuteStatus();
            GCodeFileManager.PropertyChanged += (e, a) => RefreshCommandExecuteStatus(); 
        }

        protected override void MachineChanged(IMachine machine)
        {
            machine.PropertyChanged += _machine_PropertyChanged;
        }

        public bool IsCreatingHeightMap { get { return Machine.Mode == OperatingMode.ProbingHeightMap; } }
        public bool IsProbingHeight { get { return Machine.Mode == OperatingMode.ProbingHeight; } }
        public bool IsRunningJob { get { return Machine.Mode == OperatingMode.SendingGCodeFile; } }

        private void GCodeFileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(GCodeFileManager.HasValidFile))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        private void RefreshCommandExecuteStatus()
        {
            DispatcherServices.Invoke(() =>
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
            });
        }

        public void StopJob()
        {
            GCodeFileManager.ResetJob();
            Machine.SetMode(OperatingMode.Manual);
            RaiseCanExecuteChanged();
        }

        public void HomingCycle()
        {
            Machine.HomingCycle();
            RaiseCanExecuteChanged();
        }

        public void HomeViaOrigin()
        {
            Machine.HomeViaOrigin();
        }

        public void SetAbsoluteWorkSpaceHome()
        {
            Machine.SetAbsoluteWorkSpaceHome();
        }


        public void PauseJob()
        {
            Machine.SetMode(OperatingMode.Manual);
            RaiseCanExecuteChanged();
        }

        public void ClearAlarm()
        {
            Machine.ClearAlarm();
            RaiseCanExecuteChanged();
        }

        public bool CanManipulateLaser()
        {
            return Machine.IsInitialized &&
                Machine.Settings.MachineType == FirmwareTypes.Marlin_Laser &&
                Machine.Connected &&
                Machine.Mode == OperatingMode.Manual;
        }

        public bool CanManipulateSpindle()
        {
            return Machine.IsInitialized &&
                Machine.Settings.MachineType == FirmwareTypes.GRBL1_1 &&
                Machine.Connected &&
                Machine.Mode == OperatingMode.Manual;
        }

        public bool CanMove()
        {
            return Machine.IsInitialized &&
                Machine.Connected &&
                Machine.Mode == OperatingMode.Manual;
        }

        public bool CanMoveToWorkspaceHome()
        {
            return Machine.IsInitialized &&
          Machine.Settings.MachineType == FirmwareTypes.GRBL1_1 &&
          Machine.Connected &&
          Machine.Mode == OperatingMode.Manual;
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
            if (e.PropertyName == nameof(Machine.IsInitialized) ||
                e.PropertyName == nameof(Machine.Mode) ||
                e.PropertyName == nameof(Machine.Settings) ||
                e.PropertyName == nameof(Machine.Status) ||
                e.PropertyName == nameof(Machine.Connected) ||
                e.PropertyName == nameof(Machine.Settings.CurrentSerialPort))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        public bool CanChangeConnectionStatus()
        {
            return Machine.IsInitialized &&
                (
                (Machine.Settings.ConnectionType == ConnectionTypes.Serial_Port && Machine.Settings.CurrentSerialPort != null && Machine.Settings.CurrentSerialPort.Id != "empty")
                || (Machine.Settings.ConnectionType == ConnectionTypes.Network && !String.IsNullOrEmpty(Machine.Settings.IPAddress))
                || Machine.Settings.MachineType == FirmwareTypes.SimulatedMachine);
        }

        public bool CanHomeAndReset()
        {
            return Machine.IsInitialized && Machine.Connected;
        }


        public bool CanClearAlarm()
        {
            return Machine.IsInitialized &&
                   Machine.Connected &&
                   Machine.Status.ToLower() == "alarm";
        }

        public bool FavoritesAvailable()
        {
            return Machine.IsInitialized &&
                Machine.Connected
                && Machine.Mode == OperatingMode.Manual;
        }

        public bool CanPauseJob()
        {
            return Machine.IsInitialized &&
                Machine.Mode == OperatingMode.SendingGCodeFile ||
                Machine.Mode == OperatingMode.ProbingHeightMap ||
                Machine.Mode == OperatingMode.ProbingHeight;
        }



        public bool CanProbe()
        {
            return Machine.IsInitialized &&
                Machine.Connected
                && Machine.Mode == OperatingMode.Manual;
        }

        public bool CanPauseFeed()
        {
            return (Machine.IsInitialized &&
                        Machine.Connected &&
                        Machine.Status != "Hold");
        }

        public bool CanResumeFeed()
        {
            return (Machine.IsInitialized &&
                        Machine.Connected &&
                        Machine.Status == "Hold");
        }

        public bool CanStopJob()
        {
            return Machine.IsInitialized &&
                Machine.Mode == OperatingMode.SendingGCodeFile ||
                Machine.Mode == OperatingMode.ProbingHeightMap ||
                Machine.Mode == OperatingMode.ProbingHeight;
        }


        public bool CanSendEmergencyStop()
        {
            return Machine.IsInitialized && Machine.Connected;
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
