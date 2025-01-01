using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IPhotonFeederViewModel : IViewModel
    {
        AutoFeeder SelectedFeeder { get;}
        ObservableCollection<AutoFeederSummary> ExistingAutoFeeders { get; }
        ObservableCollection<PhotonFeeder> DiscoveredFeeders { get; }
        RelayCommand<int> NextPartCommand { get; }
        RelayCommand DiscoverFeedersCommand { get; }
        RelayCommand AddFeederCommand { get; }
        byte SlotsToSearch { get; set; }
        byte SlotSearchIndex { get; set; }
        string Status { get; set; }
    }
}
