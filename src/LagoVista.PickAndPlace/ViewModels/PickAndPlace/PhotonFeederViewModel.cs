using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
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
            DiscoverFeedersCommand = new RelayCommand(Discover);
        }


        public RelayCommand<int> NextPartCommand { get; }
        public RelayCommand DiscoverFeedersCommand { get; }
        public RelayCommand AddFeederCommand { get; }

        public override Task InitAsync()
        {
            return base.InitAsync();
        }

        private bool _completed;

        private Dictionary<byte, string> responses = new Dictionary<byte, string>();


        private void CurrentMachine_LineReceived(object sender, string e)
        {
            var regExp = new Regex("^rs485-reply: (?'payload'[0-9A-F]+)$");
            var match = regExp.Match(e);
            if (match.Success)
            {
                try
                {
                    var responsePayload = match.Groups[1].Value;

                    Debug.WriteLine(e);
                    var parsed = _protocolHandler.ParseResponse(responsePayload);
                    _completed = true;
                    responses.Add(parsed.PacketId, parsed.TextPayload);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
        }

        public async void Discover()
        {
            Task.Run(async () =>
            {
                _machineRepo.CurrentMachine.LineReceived += CurrentMachine_LineReceived;
                byte idx = 1;
                while (idx <= FeedersToSearch)
                {
                    var gcode = _protocolHandler.GenerateGCode(LumenSupport.FeederCommands.GetId, idx);
                    _machineRepo.CurrentMachine.SendCommand(gcode.GCode);
                    var attempt = 0;
                    while (!_completed || attempt++ > 200)
                        await Task.Delay(5);

                    if (responses.ContainsKey(gcode.PacketId))
                    {
                        DispatcherServices.Invoke(() =>
                        {
                            DiscoveredFeeders.Add(new PhotonFeeder() { Address = responses[gcode.PacketId], Slot = idx });
                        });
                    }
                    idx++;
                }

                _machineRepo.CurrentMachine.LineReceived -= CurrentMachine_LineReceived;
            });
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

        private int _feedersToSearch = 50;
        public int FeedersToSearch
        {
            get => _feedersToSearch;
            set => Set(ref _feedersToSearch, value);
        }
    }
}
