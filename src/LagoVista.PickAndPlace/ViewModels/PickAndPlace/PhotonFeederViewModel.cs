// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3cd4bf9f96b1e6aaf51488bbe31f4f0344f38b05589160f49fc3a9765088c8ba
// IndexVersion: 2
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
using System.Collections.Concurrent;
using LagoVista.PickAndPlace.LumenSupport;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PhotonFeederViewModel : MachineViewModelBase, IPhotonFeederViewModel
    {
        private readonly IPhotonProtocolHandler _protocolHandler;
        private readonly ConcurrentDictionary<byte, string> _initialized = new ConcurrentDictionary<byte, string>();
        private readonly ConcurrentDictionary<byte, bool> _uncertain = new ConcurrentDictionary<byte, bool>();
        private int _busy, _generation;
        private bool _armed;
        private double _distance;
        public bool IsBusBusy => Volatile.Read(ref _busy) != 0;
        public bool ManualTestArmed { get => _armed; set { Set(ref _armed, value); RaiseCanExecuteChanged(); } }
        public double ManualDistance { get => _distance; set { Set(ref _distance, value); RaiseCanExecuteChanged(); } }
        public RelayCommand IdentifyCommand { get; }
        public RelayCommand InitializeCommand { get; }
        public RelayCommand AdvanceCommand { get; }
        public RelayCommand RetractCommand { get; }
        public RelayCommand ReconcileCommand { get; }

        public PhotonFeederViewModel(IMachineRepo machineRepo, IPhotonProtocolHandler protocolHandler, IRestClient restClient, ILogger logger) : base(machineRepo)
        {
            _protocolHandler = protocolHandler ?? throw new ArgumentNullException(nameof(protocolHandler));
            DiscoverFeedersCommand = CreatedMachineConnectedCommand(Discover, () => !IsBusBusy && !Machine.Busy);
            IdentifyCommand = CreatedMachineConnectedCommand(async () => await TestSelectedAsync(FeederCommands.IdentifyFeeder), CanTest);
            InitializeCommand = CreatedMachineConnectedCommand(async () => await TestSelectedAsync(FeederCommands.Initialize), CanTest);
            AdvanceCommand = CreatedMachineConnectedCommand(async () => await TestSelectedAsync(FeederCommands.MoveFeedForward), CanMove);
            RetractCommand = CreatedMachineConnectedCommand(async () => await TestSelectedAsync(FeederCommands.MoveFeedBackward), CanMove);
            ReconcileCommand = CreatedMachineConnectedCommand(() =>
            {
                if (!CanTest()) return;
                var slot = (byte)SelectedPhotonFeeder.Slot;
                _uncertain.TryRemove(slot, out _);
                _initialized.TryRemove(slot, out _);
                ManualTestArmed = false;
                Status = $"Slot {slot}: operator confirmed tape reconciliation; initialize again.";
            }, CanTest);
        }

        private bool CanTest() => !IsBusBusy && Machine != null && Machine.Connected && !Machine.Busy &&
            ManualTestArmed && SelectedPhotonFeeder != null && SelectedPhotonFeeder.Slot > 0 &&
            SelectedPhotonFeeder.Slot < 255 && PhotonProtocolHandler.IsFeederId(SelectedPhotonFeeder.Address);
        private bool CanMove() => CanTest() && PhotonProtocolHandler.TryEncodeDistance(ManualDistance, out _) &&
            IsInitialized((byte)SelectedPhotonFeeder.Slot, SelectedPhotonFeeder.Address);

        public bool IsInitialized(byte slot, string id) => !_uncertain.ContainsKey(slot) &&
            _initialized.TryGetValue(slot, out var known) && string.Equals(id, known, StringComparison.OrdinalIgnoreCase);

        protected override void OnMachineDisconnected()
        {
            Interlocked.Increment(ref _generation);
            _initialized.Clear();
            DispatcherServices.Invoke(() =>
            {
                ManualTestArmed = false;
                SelectedPhotonFeeder = null;
                DiscoveredFeeders.Clear();
                Status = "Disconnected; feeder state unknown.";
            });
            base.OnMachineDisconnected();
        }
        protected override void MachineChanged(IMachine machine)
        {
            OnMachineDisconnected();
            base.MachineChanged(machine);
        }

        private bool Connected(IMachine machine, int generation) => machine != null && machine.Connected &&
            ReferenceEquals(machine, MachineRepo.CurrentMachine) && generation == Volatile.Read(ref _generation);

        private async Task<InvokeResult> OnBusAsync(string description, Func<IMachine, int, Task<InvokeResult>> action)
        {
            if (Interlocked.CompareExchange(ref _busy, 1, 0) != 0)
                return InvokeResult.FromError("Photon bus busy; no command sent.");
            try
            {
                var machine = MachineRepo.CurrentMachine;
                var generation = Volatile.Read(ref _generation);
                Status = description + " — pending";
                RaisePropertyChanged(nameof(IsBusBusy));
                RaiseCanExecuteChanged();
                var result = Connected(machine, generation) ? await action(machine, generation) :
                    InvokeResult.FromError("Machine disconnected.");
                Status = description + (result.Successful ? " — confirmed" : " — " + result.ErrorMessage);
                return result;
            }
            catch (Exception ex)
            {
                Status = description + " — " + ex.Message;
                return InvokeResult.FromError(ex.Message);
            }
            finally
            {
                Interlocked.Exchange(ref _busy, 0);
                RaisePropertyChanged(nameof(IsBusBusy));
                RaiseCanExecuteChanged();
            }
        }

        private async Task<PhotonResponse> ExchangeAsync(IMachine machine, int generation, PhotonCommand command, byte slot)
        {
            if (!Connected(machine, generation)) throw new InvalidOperationException("Machine disconnected or changed.");
            var completion = new TaskCompletionSource<PhotonResponse>(TaskCreationOptions.RunContinuationsAsynchronously);
            const string prefix = "rs485-reply:";
            EventHandler<string> handler = (sender, line) =>
            {
                if (line == null || !line.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return;
                var payload = line.Substring(prefix.Length).Trim();
                if (string.Equals(payload, "timeout", StringComparison.OrdinalIgnoreCase))
                {
                    completion.TrySetException(new TimeoutException("RS485 bridge timeout."));
                    return;
                }
                try
                {
                    var response = _protocolHandler.ParseResponse(payload);
                    if (PhotonProtocolHandler.MatchesResponse(response, command.PacketId, slot))
                        completion.TrySetResult(response);
                }
                catch (Exception ex) { completion.TrySetException(new FormatException("Malformed Photon reply.", ex)); }
            };
            machine.LineReceived += handler;
            try
            {
                machine.SendCommand(command.GCode);
                var clock = Stopwatch.StartNew();
                while (!completion.Task.IsCompleted && clock.ElapsedMilliseconds < 1500)
                {
                    if (!Connected(machine, generation)) throw new InvalidOperationException("Disconnected during Photon request.");
                    await Task.WhenAny(completion.Task, Task.Delay(25));
                }
                if (!Connected(machine, generation)) throw new InvalidOperationException("Disconnected during Photon request.");
                if (!completion.Task.IsCompleted) throw new TimeoutException("No matching Photon response.");
                return await completion.Task;
            }
            finally { machine.LineReceived -= handler; }
        }
        private async Task<string> ReadIdAsync(IMachine machine, int generation, byte slot)
        {
            var reply = await ExchangeAsync(machine, generation, _protocolHandler.GenerateGCode(FeederCommands.GetId, slot), slot);
            if (reply.Status != 0 || reply.BinaryPayload.Length != 12)
                throw new InvalidOperationException($"Slot {slot}: invalid ID reply, status 0x{reply.Status:X2}.");
            return reply.TextPayload;
        }

        public async void Discover()
        {
            if (Machine == null || !Machine.Connected || Machine.Busy || IsBusBusy) return;
            var count = SlotsToSearch;
            if (count == 0 || count == 255) { Status = "Search 1–254 slots."; return; }
            await OnBusAsync("Discovery", async (machine, generation) =>
            {
                ManualTestArmed = false;
                SelectedPhotonFeeder = null;
                DiscoveredFeeders.Clear();
                SlotSearchIndex = 0;
                int missing = 0, errors = 0;
                for (int i = 1; i <= count; i++)
                {
                    if (!Connected(machine, generation)) return InvokeResult.FromError("Disconnected; discovery incomplete.");
                    var slot = (byte)i;
                    SlotSearchIndex = slot;
                    try
                    {
                        var id = await ReadIdAsync(machine, generation, slot);
                        if (_initialized.TryGetValue(slot, out var known) && !string.Equals(id, known, StringComparison.OrdinalIgnoreCase))
                            _initialized.TryRemove(slot, out _);
                        DiscoveredFeeders.Add(new PhotonFeeder { Address = id, Slot = slot });
                        Status = $"Slot {slot}: found {id}";
                    }
                    catch (TimeoutException) { _initialized.TryRemove(slot, out _); missing++; Status = $"Slot {slot}: no response"; }
                    catch (Exception ex) { _initialized.TryRemove(slot, out _); errors++; Status = $"Slot {slot}: {ex.Message}"; }
                }
                return InvokeResult.FromError($"Scan complete: {DiscoveredFeeders.Count} found, {missing} no response, {errors} errors. No response is not proof of absence.");
            });
        }

        public Task<InvokeResult> InitializeFeederAsync(byte slot, string id)
        {
            if (slot == 0 || slot == 255 || !PhotonProtocolHandler.IsFeederId(id))
                return Task.FromResult(InvokeResult.FromError("Invalid slot or 12-byte feeder ID."));
            if (_uncertain.ContainsKey(slot)) return Task.FromResult(InvokeResult.FromError("Tape position uncertain; reconcile physically first."));
            return OnBusAsync($"Initialize slot {slot} ({id})", async (machine, generation) =>
            {
                _initialized.TryRemove(slot, out _);
                for (int attempt = 1; attempt <= 3; attempt++)
                {
                    try
                    {
                        var actual = await ReadIdAsync(machine, generation, slot);
                        if (!string.Equals(actual, id, StringComparison.OrdinalIgnoreCase))
                            return InvokeResult.FromError($"Wrong feeder: expected {id}, found {actual}.");
                        var reply = await ExchangeAsync(machine, generation, _protocolHandler.GenerateGCode(FeederCommands.Initialize, slot, id), slot);
                        if (reply.Status != 0 || reply.BinaryPayload.Length != 12 || !string.Equals(reply.TextPayload, id, StringComparison.OrdinalIgnoreCase))
                            return InvokeResult.FromError($"Initialization rejected or ID mismatch, status 0x{reply.Status:X2}.");
                        _initialized[slot] = id;
                        return InvokeResult.Success;
                    }
                    catch (TimeoutException) when (attempt < 3) { await Task.Delay(100); }
                }
                return InvokeResult.FromError("Initialization not confirmed.");
            });
        }

        public Task<InvokeResult> AdvanceFeed(byte slot, double mm, string id = null) => MoveAsync(slot, mm, id, FeederCommands.MoveFeedForward);
        public Task<InvokeResult> RetractFeed(byte slot, double mm, string id = null) => MoveAsync(slot, mm, id, FeederCommands.MoveFeedBackward);
        private Task<InvokeResult> MoveAsync(byte slot, double mm, string id, FeederCommands direction)
        {
            if (slot == 0 || slot == 255 || !PhotonProtocolHandler.TryEncodeDistance(mm, out var distance))
                return Task.FromResult(InvokeResult.FromError("Invalid slot or distance; use 0.1–25.5 mm in 0.1 mm increments."));
            if (id == null) _initialized.TryGetValue(slot, out id);
            if (!IsInitialized(slot, id)) return Task.FromResult(InvokeResult.FromError("Feeder not initialized for this ID, or tape position uncertain."));
            return OnBusAsync($"{direction} slot {slot} ({id}), {mm:0.0} mm", async (machine, generation) =>
            {
                if (!IsInitialized(slot, id)) return InvokeResult.FromError("Feeder not initialized or tape position uncertain.");
                var actual = await ReadIdAsync(machine, generation, slot);
                if (!string.Equals(actual, id, StringComparison.OrdinalIgnoreCase))
                {
                    _initialized.TryRemove(slot, out _);
                    return InvokeResult.FromError($"Wrong feeder: expected {id}, found {actual}.");
                }
                // A dispatched physical move is never retried.
                _uncertain[slot] = true;
                try
                {
                    var reply = await ExchangeAsync(machine, generation, _protocolHandler.GenerateGCode(direction, slot, new byte[] { distance }), slot);
                    if (reply.Status != 0 || reply.BinaryPayload.Length != 2)
                        return InvokeResult.FromError($"Move not confirmed, status 0x{reply.Status:X2}; tape position uncertain.");
                    var expectedMs = (reply.BinaryPayload[0] << 8) | reply.BinaryPayload[1];
                    Status = $"Slot {slot}: move acknowledged; awaiting feed status.";
                    var clock = Stopwatch.StartNew();
                    while (clock.ElapsedMilliseconds < expectedMs)
                    {
                        if (!Connected(machine, generation)) throw new InvalidOperationException("Disconnected while feeding.");
                        await Task.Delay(Math.Min(100, Math.Max(1, expectedMs - (int)clock.ElapsedMilliseconds)));
                    }
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        try
                        {
                            var replyStatus = await ExchangeAsync(machine, generation, _protocolHandler.GenerateGCode(FeederCommands.MoveFeedStatus, slot), slot);
                            if (replyStatus.Status == 0 && replyStatus.BinaryPayload.Length == 0)
                            {
                                _uncertain.TryRemove(slot, out _);
                                return InvokeResult.Success;
                            }
                            if (replyStatus.Status != 4) return InvokeResult.FromError($"Feed status 0x{replyStatus.Status:X2}; tape position uncertain.");
                        }
                        catch (TimeoutException) { }
                        await Task.Delay(100);
                    }
                    return InvokeResult.FromError("Feed completion not confirmed; tape position uncertain. Inspect and reconcile.");
                }
                catch (Exception ex) { return InvokeResult.FromError(ex.Message + " Tape position uncertain; inspect and reconcile."); }
            });
        }

        private async Task TestSelectedAsync(FeederCommands command)
        {
            bool move = command == FeederCommands.MoveFeedForward || command == FeederCommands.MoveFeedBackward;
            if (move ? !CanMove() : !CanTest()) return;
            var slot = (byte)SelectedPhotonFeeder.Slot;
            var id = SelectedPhotonFeeder.Address;
            var mm = ManualDistance;
            ManualTestArmed = false;
            InvokeResult result;
            if (command == FeederCommands.Initialize) result = await InitializeFeederAsync(slot, id);
            else if (move) result = await MoveAsync(slot, mm, id, command);
            else result = await OnBusAsync($"Identify slot {slot} ({id})", async (machine, generation) =>
            {
                if (!string.Equals(await ReadIdAsync(machine, generation, slot), id, StringComparison.OrdinalIgnoreCase))
                    return InvokeResult.FromError("Feeder identity changed.");
                var reply = await ExchangeAsync(machine, generation, _protocolHandler.GenerateGCode(command, 255, id), slot);
                return reply.Status == 0 && reply.BinaryPayload.Length == 0 ? InvokeResult.Success : InvokeResult.FromError($"Identify rejected, status 0x{reply.Status:X2}.");
            });
            if (!result.Successful) Status = result.ErrorMessage;
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
            set { Set(ref _selectedPhotonFeeder, value); ManualTestArmed = false; RaiseCanExecuteChanged(); }
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
