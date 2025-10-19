// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f39fe8000a54c67057551d1d5385fb5d88a009b64ed1b64003a37eadbba180d7
// IndexVersion: 0
// --- END CODE INDEX META ---
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
