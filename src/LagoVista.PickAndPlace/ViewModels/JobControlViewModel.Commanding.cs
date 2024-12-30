using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels
{
    public partial class JobControlViewModel
    {
        private void InitCommands()
        {
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

        private void GCodeFileManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.GCodeFileManager.HasValidFile))
            {
                DispatcherServices.Invoke(RefreshCommandExecuteStatus);
            }
        }

        private void RefreshCommandExecuteStatus()
        {
            ConnectCommand.RaiseCanExecuteChanged();

            ClearAlarmCommand.RaiseCanExecuteChanged();

            SendGCodeFileCommand.RaiseCanExecuteChanged();
            StartProbeCommand.RaiseCanExecuteChanged();
            StartProbeHeightMapCommand.RaiseCanExecuteChanged();
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

        public bool CanManipulateLaser()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                _machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.Marlin_Laser &&
                _machineRepo.CurrentMachine.Connected &&
                _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanManipulateSpindle()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                _machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.GRBL1_1 &&
                _machineRepo.CurrentMachine.Connected &&
                _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanMove()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                _machineRepo.CurrentMachine.Connected &&
                _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanMoveToWorkspaceHome()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
          _machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.GRBL1_1 &&
          _machineRepo.CurrentMachine.Connected &&
          _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        private void HeightMapManager_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.HeightMapManager.HeightMap) ||
               e.PropertyName == nameof(Machine.HeightMapManager.HeightMap.Status))
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
            return _machineRepo.CurrentMachine.IsInitialized &&
                (
                (_machineRepo.CurrentMachine.Settings.ConnectionType == ConnectionTypes.Serial_Port && _machineRepo.CurrentMachine.Settings.CurrentSerialPort != null && _machineRepo.CurrentMachine.Settings.CurrentSerialPort.Id != "empty")
                || (_machineRepo.CurrentMachine.Settings.ConnectionType == ConnectionTypes.Network && !String.IsNullOrEmpty(_machineRepo.CurrentMachine.Settings.IPAddress))
                || _machineRepo.CurrentMachine.Settings.MachineType == FirmwareTypes.SimulatedMachine);
        }

        public bool CanHomeAndReset()
        {
            return _machineRepo.CurrentMachine.IsInitialized && _machineRepo.CurrentMachine.Connected;
        }

        public bool CanSendGcodeFile()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                _machineRepo.CurrentMachine.GCodeFileManager.HasValidFile &&
                _machineRepo.CurrentMachine.Connected &&
                _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanClearAlarm()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                   _machineRepo.CurrentMachine.Connected &&
                   _machineRepo.CurrentMachine.Status.ToLower() == "alarm";
        }

        public bool FavoritesAvailable()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                _machineRepo.CurrentMachine.Connected
                && _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanPauseJob()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                _machineRepo.CurrentMachine.Mode == OperatingMode.SendingGCodeFile ||
                _machineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeightMap ||
                _machineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeight;
        }


        public bool CanProbeHeightMap()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                _machineRepo.CurrentMachine.Connected
                && _machineRepo.CurrentMachine.Mode == OperatingMode.Manual
                && _machineRepo.CurrentMachine.HeightMapManager.HasHeightMap;
        }

        public bool CanProbe()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                _machineRepo.CurrentMachine.Connected
                && _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanPauseFeed()
        {
            return (_machineRepo.CurrentMachine.IsInitialized &&
                        _machineRepo.CurrentMachine.Connected &&
                        _machineRepo.CurrentMachine.Status != "Hold");
        }

        public bool CanResumeFeed()
        {
            return (_machineRepo.CurrentMachine.IsInitialized &&
                        _machineRepo.CurrentMachine.Connected &&
                        _machineRepo.CurrentMachine.Status == "Hold");
        }

        public bool CanStopJob()
        {
            return _machineRepo.CurrentMachine.IsInitialized &&
                _machineRepo.CurrentMachine.Mode == OperatingMode.SendingGCodeFile ||
                _machineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeightMap ||
                _machineRepo.CurrentMachine.Mode == OperatingMode.ProbingHeight;
        }


        public bool CanSendEmergencyStop()
        {
            return _machineRepo.CurrentMachine.IsInitialized && _machineRepo.CurrentMachine.Connected;
        }

        public RelayCommand StopCommand { get; private set; }

        public RelayCommand PauseCommand { get; private set; }


        public RelayCommand HomingCycleCommand { get; private set; }
        public RelayCommand HomeViaOriginCommand { get; private set; }
        public RelayCommand SetAbsoluteWorkSpaceHomeCommand { get; private set; }

        public RelayCommand SoftResetCommand { get; private set; }

        public RelayCommand CycleStartCommand { get; private set; }
        public RelayCommand FeedHoldCommand { get; private set; }


        public RelayCommand StartProbeCommand { get; private set; }
        public RelayCommand StartProbeHeightMapCommand { get; private set; }
        public RelayCommand SendGCodeFileCommand { get; private set; }
        public RelayCommand EmergencyStopCommand { get; private set; }

        public RelayCommand ClearAlarmCommand { get; private set; }

        public RelayCommand ConnectCommand { get; private set; }

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

        public RelayCommand SuctionSolenoidCommand { get; private set;}
    }
}
