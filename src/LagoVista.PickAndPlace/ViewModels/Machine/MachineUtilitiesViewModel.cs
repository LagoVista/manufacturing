using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineUtilitiesViewModel : StagingPlateNavigationViewModel, IMachineUtilitiesViewModel
    {
        

        public MachineUtilitiesViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {        
            ReadLeftVacuumCommand = CreatedMachineConnectedCommand(async () => await ReadLeftVacuumAsync());
            ReadRightVacuumCommand = CreatedMachineConnectedCommand(async () => await ReadRightVacuumAsync());

            LeftVacuumOnCommand = CreatedMachineConnectedCommand(() => MachineRepo.CurrentMachine.LeftVacuumPump = true);
            LeftVacuumOffCommand = CreatedMachineConnectedCommand(() => MachineRepo.CurrentMachine.LeftVacuumPump = false);

            RightVacuumOnCommand = CreatedMachineConnectedCommand(() => MachineRepo.CurrentMachine.RightVacuumPump = true);
            RightVacuumOffCommand = CreatedMachineConnectedCommand(() => MachineRepo.CurrentMachine.RightVacuumPump = false);

            TopLightOnCommand = CreatedMachineConnectedCommand(() => MachineRepo.CurrentMachine.TopLightOn = true);
            TopLightOffCommand = CreatedMachineConnectedCommand(() => MachineRepo.CurrentMachine.TopLightOn = false);
            BottomLightOnCommand = CreatedMachineConnectedCommand(() => MachineRepo.CurrentMachine.BottomLightOn = true);
            BottomLightOffCommand = CreatedMachineConnectedCommand(() => MachineRepo.CurrentMachine.BottomLightOn = false);
        }

        public async Task<ulong> ReadVacuumAsync()
        {
            var result = await MachineRepo.CurrentMachine.ReadVacuumAsync();
            if (result.Successful)
            {
                LeftVacuum = result.Result;
                RaisePropertyChanged(nameof(LeftVacuum));
                return LeftVacuum;
            }

            return 0;
        }

        public async Task<ulong> ReadLeftVacuumAsync()
        {
             var result = await MachineRepo.CurrentMachine.ReadLeftVacuumAsync();
            if(result.Successful)
            {
                LeftVacuum = result.Result;
                RaisePropertyChanged(nameof(LeftVacuum));
                return LeftVacuum;
            }

            return 0;            
        }

        public async Task<ulong> ReadRightVacuumAsync()
        {
            var result = await MachineRepo.CurrentMachine.ReadRightVacuumAsync();
            if (result.Successful)
            {
                RightVacuum = result.Result;
                RaisePropertyChanged(nameof(RightVacuum));

                return RightVacuum;
            }

            return 0;
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
