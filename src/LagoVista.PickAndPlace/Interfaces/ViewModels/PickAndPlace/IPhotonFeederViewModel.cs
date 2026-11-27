// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 276627f3ba76d61fb76ac9ae5104a5033a61df8787a95e26bc4d6426a7fc285e
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
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
        event EventHandler<string> OperationMessage;

        bool IsBusBusy { get; }
        bool ManualTestArmed { get; set; }
        string ManualTestStatus { get; }
        string ManualDistanceText { get; set; }
        double ManualDistance { get; }
        string GetDiscoveryStatus(byte slot);
        bool IsInitialized(byte slot, string id);
        RelayCommand IdentifyCommand { get; }
        RelayCommand InitializeCommand { get; }
        RelayCommand AdvanceCommand { get; }
        RelayCommand RetractCommand { get; }
        RelayCommand ReconcileCommand { get; }
        RelayCommand<int> NextPartCommand { get; }
        RelayCommand DiscoverFeedersCommand { get; }
        RelayCommand AddFeederCommand { get; }

        Task<InvokeResult> InitializeFeederAsync(byte slotIndex, string feederId);
        Task<InvokeResult> AdvanceFeed(byte slotIndex, double mm, string feederId = null);
        Task<InvokeResult> RetractFeed(byte slotIndex, double mm, string feederId = null);



    }
}
