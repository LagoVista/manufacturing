using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineUtilitiesViewModel : StagingPlateNavigationViewModel, IMachineUtilitiesViewModel
    {
        public MachineUtilitiesViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
            ReadLeftVacuumCommand = CreatedMachineConnectedCommand(ReadLeftVacuum);
            ReadRightVacuumCommand = CreatedMachineConnectedCommand(ReadRightVacuum);

            LeftVacuumOnCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.LeftVacuumPump = true);
            LeftVacuumOffCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.LeftVacuumPump = false);

            RightVacuumOnCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.RightVacuumPump = true);
            RightVacuumOffCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.RightVacuumPump = false);

            TopLightOnCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.TopLightOn = true);
            TopLightOffCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.TopLightOn = false);
            BottomLightOnCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.BottomLightOn = true);
            BottomLightOffCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.BottomLightOn = false);
        }    

        public async void ReadLeftVacuum()
        {
             var result = await _machineRepo.CurrentMachine.ReadLeftVacuumAsync();
            if(result.Successful)
            {
                LeftVacuum = result.Result;
                RaisePropertyChanged(nameof(LeftVacuum));
            }
        }

        public async void ReadRightVacuum()
        {
            var result = await _machineRepo.CurrentMachine.ReadRightVacuumAsync();
            if (result.Successful)
            {
                RightVacuum = result.Result;
                RaisePropertyChanged(nameof(RightVacuum));
            }
        }

        public ulong LeftVacuum { get; private set; }
        public ulong RightVacuum { get; private set; }


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
    }
}
