// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3cd4bf9f96b1e6aaf51488bbe31f4f0344f38b05589160f49fc3a9765088c8ba
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PhotonFeederViewModel : MachineViewModelBase, IPhotonFeederViewModel
    {
        IPhotonProtocolHandler _protocolHandler;
        private Dictionary<byte, FeederSearchResult> _feederDiscoverResults = new Dictionary<byte, FeederSearchResult>();

        private class FeederSearchResult
        {
            public byte Slot { get; set; }
            public bool Found { get; set; }
            public string Address { get; set; }
        }

        public PhotonFeederViewModel(IMachineRepo machineRepo, IPhotonProtocolHandler protocolHandler, IRestClient restClient, ILogger logger) : base(machineRepo)
        {
            _protocolHandler = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
            DiscoverFeedersCommand = CreatedMachineConnectedCommand(Discover);
        }

        public override async Task InitAsync()
        {
            await base.InitAsync();
        }

        private void CurrentMachine_LineReceived(object sender, string e)
        {
            var regExp = new Regex("^rs485-reply: (?'payload'[0-9A-F]+)$");
            var match = regExp.Match(e);
            if (match.Success)
            {
                try
                {
                    var responsePayload = match.Groups[1].Value;
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
            if (!MachineRepo.CurrentMachine.Connected)
            {
                MachineRepo.CurrentMachine.AddStatusMessage(StatusMessageTypes.Info, "Machine is not connected, can not discover feeders.");
                return;
            }

            DiscoveredFeeders.Clear();
            SlotSearchIndex = 0;
            new Thread(() =>
            {
                _feederDiscoverResults.Clear();
                MachineRepo.CurrentMachine.LineReceived += CurrentMachine_LineReceived;
                byte idx = 1;
                while (idx <= SlotsToSearch)
                {
                    _completed.Reset();
                    var gcode = _protocolHandler.GenerateGCode(LumenSupport.FeederCommands.GetId, idx);
                    _feederDiscoverResults.Add(gcode.PacketId, new FeederSearchResult() { Slot = idx });
                    MachineRepo.CurrentMachine.SendCommand(gcode.GCode);
                    var attemptCount = 0;
                    while (!_completed.IsSet && ++attemptCount < 200)
                        _completed.Wait(5);

                    Debug.WriteLine("Attempt Count: " + attemptCount);

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

                MachineRepo.CurrentMachine.LineReceived -= CurrentMachine_LineReceived;
            }).Start();
        }

        public async Task<InvokeResult> InitializeFeederAsync(byte slotIndex, string feederId)
        {
            var retryCount = 5;
            while (retryCount > 0)
            {
                var gcode = _protocolHandler.GenerateGCode(LumenSupport.FeederCommands.Initialize, slotIndex, feederId);

                MachineRepo.CurrentMachine.SendCommand(gcode.GCode);

                MachineRepo.CurrentMachine.LineReceived += CurrentMachine_LineReceived_Advance;
                _completed.Reset();
                int attemptCount = 0;
                while (!_completed.IsSet && ++attemptCount < 200)
                {
                    _completed.Wait(1);
                    await Task.Delay(5);
                }

                MachineRepo.CurrentMachine.LineReceived -= CurrentMachine_LineReceived_Advance;

                if (_completed.IsSet)
                    return InvokeResult.Success;

                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"could not initialize feeder, will retry.  Retry count {retryCount}");
                retryCount++;
            }

            Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not initailzed feeder.");
            return InvokeResult.FromError("Could not inialize feeder.");

        }

        public async Task<InvokeResult> AdvanceFeed(byte slotIndex, double mm)
        {
            var gcode = _protocolHandler.GenerateGCode(LumenSupport.FeederCommands.MoveFeedForward, slotIndex, new byte[1] { Convert.ToByte(mm * 10) });
            MachineRepo.CurrentMachine.SendCommand(gcode.GCode);

            MachineRepo.CurrentMachine.LineReceived += CurrentMachine_LineReceived_Advance;
            _completed.Reset();
            int attemptCount = 0;
            while (!_completed.IsSet && ++attemptCount < 200)
            {
                _completed.Wait(1);
                await Task.Delay(5);
            }

            MachineRepo.CurrentMachine.LineReceived -= CurrentMachine_LineReceived_Advance;

            return InvokeResult.Success;
        }


        private void CurrentMachine_LineReceived_Advance(object sender, string e)
        {
            var regExp = new Regex("^rs485-reply: (?'payload'[0-9A-F]+)$");
            var match = regExp.Match(e);
            if (match.Success)
            {
                try
                {
                    var responsePayload = match.Groups[1].Value;
                    var parsed = _protocolHandler.ParseResponse(responsePayload);
                    Debug.WriteLine($"From: {parsed.FromAddress} -> {parsed.ToAddress} - Status:  {parsed.Status}");
                    _completed.Set();
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


        public async Task<InvokeResult> RetractFeed(byte slotIndex, double mm)
        {
            var gcode = _protocolHandler.GenerateGCode(LumenSupport.FeederCommands.MoveFeedBackward, slotIndex, new byte[1] { Convert.ToByte(mm * 10) });
            MachineRepo.CurrentMachine.SendCommand(gcode.GCode);

            int attemptCount = 0;
            while (!_completed.IsSet && ++attemptCount < 200)
                _completed.Wait(1);
            await Task.Delay(5);

            return InvokeResult.Success;
        }

        private ObservableCollection<PhotonFeeder> _discoveredFeeders = new ObservableCollection<PhotonFeeder>();
        public ObservableCollection<PhotonFeeder> DiscoveredFeeders
        {
            get => _discoveredFeeders;
            set => Set(ref _discoveredFeeders, value);
        }

        PhotonFeeder _selectedPhotonFeeder;
        public PhotonFeeder SelectedPhotonFeeder
        {
            get => _selectedPhotonFeeder;
            set => Set(ref _selectedPhotonFeeder, value);
        }

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

        private string _selectedAutoFeederTemplateId = "-1";
        public string SelectedAutoFeederTemplateId
        {
            get => _selectedAutoFeederTemplateId;
            set
            {
                Set(ref _selectedAutoFeederTemplateId, value);
            }
        }

        List<AutoFeederTemplateSummary> _autoFeederTemplates;
        public List<AutoFeederTemplateSummary> AutoFeederTemplates
        {
            get => _autoFeederTemplates;
            set => Set(ref _autoFeederTemplates, value);
        }

        public RelayCommand<int> NextPartCommand { get; }
        public RelayCommand DiscoverFeedersCommand { get; }
        public RelayCommand AddFeederCommand { get; }
    }
}
