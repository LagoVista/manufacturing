using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineUtilitiesViewModel : ViewModelBase, IMachineUtilitiesViewModel
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILogger _logger;

        public MachineUtilitiesViewModel(IMachineRepo machineRepo, ILogger logger)
        {
            Machine = machineRepo.CurrentMachine;
            machineRepo.MachineChanged += MachineRepo_MachineChanged;
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ReadLeftVacuumCommand = new RelayCommand(ReadLeftVacuum, CanExecute);
            ReadRightVacuumCommand = new RelayCommand(ReadRightVacuum, CanExecute);

            LeftVacuumOnCommand = new RelayCommand(() => _machineRepo.CurrentMachine.LeftVacuumPump = true, CanExecute);
            LeftVacuumOffCommand = new RelayCommand(() => _machineRepo.CurrentMachine.LeftVacuumPump = false, CanExecute);

            RightVacuumOnCommand = new RelayCommand(() => _machineRepo.CurrentMachine.RightVacuumPump = true, CanExecute);
            RightVacuumOffCommand = new RelayCommand(() => _machineRepo.CurrentMachine.RightVacuumPump = false, CanExecute);

            TopLightOnCommand = new RelayCommand(() => _machineRepo.CurrentMachine.TopLightOn = true, CanExecute);
            TopLightOffCommand = new RelayCommand(() => _machineRepo.CurrentMachine.TopLightOn = false, CanExecute);
            BottomLightOnCommand = new RelayCommand(() => _machineRepo.CurrentMachine.BottomLightOn = true, CanExecute);
            BottomLightOffCommand = new RelayCommand(() => _machineRepo.CurrentMachine.BottomLightOn = false, CanExecute);
        }

        public bool CanExecute()
        {
            return _machineRepo.CurrentMachine.Connected && !_machineRepo.CurrentMachine.Busy;
        }

        // https://cfsensor.com/wp-content/uploads/2022/11/XGZP6857D-Pressure-Sensor-V2.7.pdf

        public async void ReadLeftVacuum()
        {
             var result = await _machineRepo.CurrentMachine.ReadLeftVacuumAsync();
            if(result.Successful)
            {
                LeftVacuum = result.Result;
            }
        }

        public async void ReadRightVacuum()
        {
            var result = await _machineRepo.CurrentMachine.ReadRightVacuumAsync();
            if (result.Successful)
            {
                RightVacuum = result.Result;
            }
        }

        public ulong _leftVacuum;
        public ulong LeftVacuum
        {
            get => _leftVacuum;
            set => Set(ref _leftVacuum, value);
        }

        public ulong _rightVacuum;
        public ulong RightVacuum
        {
            get => _rightVacuum;
            set => Set(ref _rightVacuum, value);
        }

        private void MachineRepo_MachineChanged(object sender, IMachine e)
        {
            Machine = e;
            _machineRepo.CurrentMachine.MachineConnected += CurrentMachine_MachineConnected;
            _machineRepo.CurrentMachine.MachineDisconnected += CurrentMachine_MachineDisconnected;
        }

        void RefreshCanExecute()
        {
            ReadLeftVacuumCommand.RaiseCanExecuteChanged();
            ReadRightVacuumCommand.RaiseCanExecuteChanged();
            LeftVacuumOnCommand.RaiseCanExecuteChanged();
            LeftVacuumOffCommand.RaiseCanExecuteChanged();
            RightVacuumOnCommand.RaiseCanExecuteChanged();
            RightVacuumOffCommand.RaiseCanExecuteChanged();
            TopLightOnCommand.RaiseCanExecuteChanged();
            TopLightOffCommand.RaiseCanExecuteChanged();
            BottomLightOnCommand.RaiseCanExecuteChanged();
            BottomLightOffCommand.RaiseCanExecuteChanged();
        }

        private void CurrentMachine_MachineDisconnected(object sender, EventArgs e)
        {
            RefreshCanExecute();
        }

        private void CurrentMachine_MachineConnected(object sender, EventArgs e)
        {
            RefreshCanExecute();
        }

        public RelayCommand ReadLeftVacuumCommand { get; }
        public RelayCommand ReadRightVacuumCommand { get; }

        public RelayCommand LeftVacuumOnCommand { get; }
        public RelayCommand LeftVacuumOffCommand { get; }
        public RelayCommand RightVacuumOnCommand { get; }
        public RelayCommand RightVacuumOffCommand { get; }

        public RelayCommand TopLightOnCommand { get; }
        public RelayCommand TopLightOffCommand { get; }


        public RelayCommand BottomLightOnCommand { get; }
        public RelayCommand BottomLightOffCommand { get; }


        IMachine _machine;
        public IMachine Machine
        {
            get { return _machine; }
            set { Set(ref _machine, value); }
        }

    }
}
