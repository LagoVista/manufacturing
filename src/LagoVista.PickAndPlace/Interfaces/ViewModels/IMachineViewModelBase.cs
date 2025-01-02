using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;


namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface IMachineViewModelBase : IViewModel
    {
        IMachine Machine { get; }
        Manufacturing.Models.Machine MachineConfiguration { get; }
        RelayCommand SaveMachineConfigurationCommand { get; }
        RelayCommand ReloadMachineCommand { set; }
    }
}
