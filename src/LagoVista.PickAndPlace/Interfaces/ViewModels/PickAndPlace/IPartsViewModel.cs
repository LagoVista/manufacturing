// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 046bd882e6052d2f786ab43c62b91a1913f9f40ac2a71300a903f5c643e5d0a1
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.ViewModels.PickAndPlace;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IPartsViewModel : IMachineViewModelBase
    {
        IStripFeederViewModel StripFeederViewModel { get; }
        IAutoFeederViewModel AutoFeederViewModel { get; }

        ObservableCollection<AvailablePart> AvailableParts { get; }

        RelayCommand RefreshAvailablePartsCommand { get; }
        void RefreshAvailableParts();
    }
}
