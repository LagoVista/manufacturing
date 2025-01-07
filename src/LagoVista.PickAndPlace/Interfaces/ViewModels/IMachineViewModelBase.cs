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
