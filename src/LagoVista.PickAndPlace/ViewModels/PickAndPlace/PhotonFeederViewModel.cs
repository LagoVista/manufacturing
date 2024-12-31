using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PhotonFeederViewModel : ViewModelBase, IPhotonFeederViewModel
    {
        IMachineRepo _machineRepo;
        IRestClient _restClient;
        ILogger _logger;
        IPhotonProtocolHandler _protocolHandler;

        public PhotonFeederViewModel(IMachineRepo machineRepo, IPhotonProtocolHandler protocolHandler, IRestClient restClient, ILogger logger)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _protocolHandler = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
        }


        public RelayCommand<int> NextPartCommand { get; }
        public RelayCommand DiscoverFeeders { get; }
        public RelayCommand AddFeederCommand { get; }

        public override Task InitAsync()
        {
            return base.InitAsync();
        }

        public async Task DiscoverAsync()
        {
            throw new NotImplementedException();
        }

        public async void Discover()
        {
            await DiscoverAsync();
        }

        private ObservableCollection<PhotonFeeder> _discoveredFeeders = new ObservableCollection<PhotonFeeder>();
        public ObservableCollection<PhotonFeeder> DiscoveredFeeders 
        {
            get => _discoveredFeeders;
            set => Set(ref _discoveredFeeders, value);
        }

        ObservableCollection<AutoFeederSummary> _existingAutoFeeders = new ObservableCollection<AutoFeederSummary>();
        public ObservableCollection<AutoFeederSummary> ExistingAutoFeeders
        {
            get => _existingAutoFeeders;
            set => Set(ref _existingAutoFeeders, value);
        }


        public AutoFeeder SelectedFeeder { get; private set; }
    }
}
