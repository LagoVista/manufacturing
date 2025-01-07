using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface ICurrentMachineViewModel : IMachineViewModelBase
    {
        RelayCommand SaveCommand { get; }
    }
}
