// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5f50adcf003a84f941a169fe4e281c1b4988853232e5170b428a39975f7501d4
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface ICurrentMachineViewModel : IMachineViewModelBase
    {
        RelayCommand SaveCommand { get; }
    }
}
