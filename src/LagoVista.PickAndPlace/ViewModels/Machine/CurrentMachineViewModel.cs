using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class CurrentMachineViewModel : ViewModelBase, ICurrentMachineViewModel
    {
        public CurrentMachineViewModel(IMachineRepo machineRepo)
        {
            machineRepo.MachineChanged += MachineRepo_MachineChanged;
            Machine = machineRepo.CurrentMachine;
            SaveCommand = new RelayCommand(() => machineRepo.SaveCurrentMachineAsync());
        }

        private void MachineRepo_MachineChanged(object sender, IMachine e)
        {
            Machine = e;
        }

        IMachine _machine;
        public IMachine Machine
        {
            get => _machine;
            set => Set(ref _machine, value);
        }

        public RelayCommand SaveCommand { get; }
    }
}
