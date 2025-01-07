using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class CurrentMachineViewModel : MachineViewModelBase, ICurrentMachineViewModel
    {
        public CurrentMachineViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
            SaveCommand = new RelayCommand(() => machineRepo.SaveCurrentMachineAsync());
        }


        public RelayCommand SaveCommand { get; }
    }
}
