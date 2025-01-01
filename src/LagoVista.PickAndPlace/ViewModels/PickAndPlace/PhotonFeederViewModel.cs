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
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PhotonFeederViewModel : ViewModelBase, IPhotonFeederViewModel
    {
        IMachineRepo _machineRepo;
        IRestClient _restClient;
        ILogger _logger;
        IPhotonProtocolHandler _protocolHandler;

        private class FeederSearchResult
        {
            public byte Slot { get; set; }
            public bool Found { get; set; }
            public string Address { get; set; }
        }


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

        private int _currentPacketId;

        private Dictionary<byte, FeederSearchResult> _feederDiscoverResults = new Dictionary<byte, FeederSearchResult>();


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
                    _completed.Set();
                    _feederDiscoverResults[parsed.PacketId].Found = true;
                    _feederDiscoverResults[parsed.PacketId].Address = parsed.TextPayload;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.ToString());
                }
            }
            else if (e.ToLower() == "rs485-reply: timeout")
            {
                _completed.Set();
            }
        }

        ManualResetEventSlim _completed = new ManualResetEventSlim();

        public void Discover()
        {
            if (!_machineRepo.CurrentMachine.Connected)
            {
                _machineRepo.CurrentMachine.AddStatusMessage(StatusMessageTypes.Info, "Machine is not connected, can not discover feeders.");
                return;
            }

            DiscoveredFeeders.Clear();
            SlotSearchIndex = 0;
            new Thread(() =>
            {
                _feederDiscoverResults.Clear();
                _machineRepo.CurrentMachine.LineReceived += CurrentMachine_LineReceived;
                byte idx = 1;
                while (idx <= SlotsToSearch)
                {
                    _completed.Reset();
                    var gcode = _protocolHandler.GenerateGCode(LumenSupport.FeederCommands.GetId, idx);
                    _currentPacketId = gcode.PacketId;
                    _feederDiscoverResults.Add(gcode.PacketId, new FeederSearchResult() { Slot = idx });
                    _machineRepo.CurrentMachine.SendCommand(gcode.GCode);
                    var attepmtCount = 0;
                    while (!_completed.IsSet && ++attepmtCount < 200)
                        _completed.Wait(5);

                    Debug.WriteLine("Attempt Count: " + attepmtCount);

                    if (_feederDiscoverResults[gcode.PacketId].Found)
                    {
                        DispatcherServices.Invoke(() =>
                        {
                            SlotSearchIndex = idx;
                            Status = $"Found {_feederDiscoverResults[gcode.PacketId].Address} at address slot {_feederDiscoverResults[gcode.PacketId].Slot}";
                            DiscoveredFeeders.Add(new PhotonFeeder()
                            {
                                Address = _feederDiscoverResults[gcode.PacketId].Address,
                                Slot = _feederDiscoverResults[gcode.PacketId].Slot
                            });
                        });
                    }
                    else
                    {
                        DispatcherServices.Invoke(() =>
                        {
                            SlotSearchIndex = idx;
                            Status = $"No feeder at slot {_feederDiscoverResults[gcode.PacketId].Slot}";
                        });
                    }
                    idx++;
                }

                DispatcherServices.Invoke(() =>
                {
                    Status = $"Found {DiscoveredFeeders.Count} feeders.";
                });

                _machineRepo.CurrentMachine.LineReceived -= CurrentMachine_LineReceived;
            }).Start();
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

        private byte _slotsToSearch = 50;
        public byte SlotsToSearch
        {
            get => _slotsToSearch;
            set => Set(ref _slotsToSearch, value);
        }

        private byte _searchIndex;
        public byte SlotSearchIndex
        {
            get => _searchIndex;
            set => Set(ref _searchIndex, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }
    }
}
