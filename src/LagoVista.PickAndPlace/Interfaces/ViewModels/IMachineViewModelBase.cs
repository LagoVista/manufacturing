// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 61269aef7cb89f5d8dc4c81d07e1132683aa9e3a7d579a2d0a686b478fd9321c
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;


namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface IMachineViewModelBase : IViewModel
    {
        IMachine Machine { get; }

        IMachineRepo MachineRepo { get; }
        Manufacturing.Models.Machine MachineConfiguration { get; }
        RelayCommand SaveMachineConfigurationCommand { get; }
        RelayCommand ReloadMachineCommand { set; }
    }
}
