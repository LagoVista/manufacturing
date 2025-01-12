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
