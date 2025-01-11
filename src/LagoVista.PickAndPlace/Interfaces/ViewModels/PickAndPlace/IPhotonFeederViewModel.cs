using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Models;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IPhotonFeederViewModel : IViewModel
    {
        ObservableCollection<PhotonFeeder> DiscoveredFeeders { get; }
        PhotonFeeder SelectedPhotonFeeder {get;}
        byte SlotsToSearch { get; set; }
        byte SlotSearchIndex { get; set; }
        string Status { get; set; }

        RelayCommand<int> NextPartCommand { get; }
        RelayCommand DiscoverFeedersCommand { get; }
        RelayCommand AddFeederCommand { get; }

        Task<InvokeResult> AdvanceFeed(byte slotIndex, double mm);
        Task<InvokeResult> RetractFeed(byte slotIndex, double mm);



    }
}
