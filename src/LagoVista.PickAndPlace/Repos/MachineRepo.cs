using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Repos
{
    public class MachineRepo : ViewModelBase, IMachineRepo
    {
        private readonly IRestClient _restClient;
        private readonly IStorageService _storageService;
        private readonly ILogger _logger;


        public event EventHandler<IMachine> MachineChanged;


        public MachineRepo(IRestClient restClient, IStorageService storageService, ILogger logger)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(restClient));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            CurrentMachine = new Machine();
            CurrentMachine.Settings = new Manufacturing.Models.Machine();
            UnlockSettingsCommand = new RelayCommand(() => CurrentMachine.AreSettingsLocked = false);
            LockSettingsCommand = new RelayCommand(() => CurrentMachine.AreSettingsLocked = true);
            ConnectCommand = new RelayCommand(Connect, () => this.HasValidMachine);
            SaveCurrentMachineCommand = new RelayCommand(async () => await SaveCurrentMachineAsync(), ()=> HasValidMachine);
            ReloadCurrentMachineCommand = new RelayCommand(async () => await LoadCurrentMachineAsync(), () => HasValidMachine);
        }

        public IMachine CurrentMachine { get; set; }

        public override async Task InitAsync()
        {
            await GetMachinesAsync();
            var machineId = await _storageService.GetKVPAsync<string>("current_machine_id");
            if (machineId != null)
            {
                await LoadMachineAsync(machineId);
            }
        }

        public async Task<List<LagoVista.Manufacturing.Models.MachineSummary>> GetMachinesAsync()
        {
            var machines = await _restClient.GetListResponseAsync<LagoVista.Manufacturing.Models.MachineSummary>("/api/mfg/machines", ListRequest.CreateForAll());
            Machines = new ObservableCollection<MachineSummary>(machines.Model);
            return machines.Model.ToList();
        }

        public async Task<InvokeResult> LoadCurrentMachineAsync()
        {
            var machineId = await _storageService.GetKVPAsync<string>("current_machine_id");
            if (!String.IsNullOrEmpty(machineId))
            {
                return await LoadMachineAsync(machineId);
            }
            else
            {
                return InvokeResult.FromError("No current machine.");
            }
        }

        private void CurrentMachine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            RaisePropertyChanged(e.PropertyName);
        }

        public async Task<InvokeResult> LoadMachineAsync(string id)
        {
            var result = await _restClient.GetAsync<DetailResponse<LagoVista.Manufacturing.Models.Machine>>($"/api/mfg/machine/{id}");
            if (result.Successful)
            {
                CurrentMachine = new Machine();
                CurrentMachine.PropertyChanged += CurrentMachine_PropertyChanged;
                CurrentMachine.Settings = result.Result.Model;
                _selectedMachine = result.Result.Model.CreateSummary();
                RaisePropertyChanged(nameof(SelectedMachine));
                RaisePropertyChanged(nameof(CurrentMachine));
                await CurrentMachine.InitAsync();
                await _storageService.StoreKVP("current_machine_id", id);
                HasValidMachine = true;
                MachineChanged?.Invoke(this, CurrentMachine);
                SaveCurrentMachineCommand.RaiseCanExecuteChanged();
                ReloadCurrentMachineCommand.RaiseCanExecuteChanged();

                return InvokeResult.Success;
            }
            else
            {
                return result.ToInvokeResult();
            }
        }

        public async void LoadMachine(string machineId)
        {
            await LoadMachineAsync(machineId);
        }

        public async Task<InvokeResult> SaveCurrentMachineAsync()
        {
            return await _restClient.PutAsync("/api/mfg/machine", CurrentMachine.Settings);
        }

        private bool _hasMachine;
        public bool HasValidMachine
        {
            get => _hasMachine;
            set => Set(ref _hasMachine, value);
        }

        ObservableCollection<MachineSummary> _machines;
        public ObservableCollection<MachineSummary> Machines
        {
            get => _machines;
            set => Set(ref _machines, value);
        }

        MachineSummary _selectedMachine;

        public MachineSummary SelectedMachine
        {
            get => _selectedMachine;
            set
            {
                if (value != null && value.Id != _selectedMachine?.Id)
                    LoadMachine(value.Id);
            }
        }

        public RelayCommand ConnectCommand {get;}

        public async void Connect()
        {
            if (CurrentMachine.Connected)
            {
                await CurrentMachine.DisconnectAsync();

                if (CurrentMachine.PositionImageCaptureService != null)
                {
                    CurrentMachine.PositionImageCaptureService.StopCapture();
                }

                if (CurrentMachine.PartInspectionCaptureService != null)
                {
                    CurrentMachine.PartInspectionCaptureService.StopCapture();
                }
            }
            else
            {
                if (CurrentMachine.Settings.MachineType == FirmwareTypes.SimulatedMachine)
                {
                    await CurrentMachine.ConnectAsync(new SimulatedSerialPort(CurrentMachine.Settings.MachineType));
                }
                else if (CurrentMachine.Settings.ConnectionType == Manufacturing.Models.ConnectionTypes.Serial_Port)
                {
                    var port1 = new SerialPort(CurrentMachine.Settings.CurrentSerialPort.Name, CurrentMachine.Settings.CurrentSerialPort.BaudRate);
                    await CurrentMachine.ConnectAsync(port1);
                }
                else
                {
                    try
                    {
                        var socketClient = SLWIOC.Create<ISocketClient>();

                        await socketClient.ConnectAsync(CurrentMachine.Settings.IPAddress, 3000);
                        await CurrentMachine.ConnectAsync(socketClient);
                    }
                    catch (Exception ex)
                    {
                        CurrentMachine.AddStatusMessage(StatusMessageTypes.FatalError, $"Could not connect to machine via network: {ex.Message}.");
                        return;
                    }
                }

                if(CurrentMachine.PositionImageCaptureService != null)
                {
                    CurrentMachine.PositionImageCaptureService.StartCapture();
                }

                if (CurrentMachine.PartInspectionCaptureService != null)
                {
                    CurrentMachine.PartInspectionCaptureService.StartCapture();
                }
            }
        }

        public RelayCommand LockSettingsCommand { get; set; }
        public RelayCommand SaveCurrentMachineCommand { get; set; }
        public RelayCommand ReloadCurrentMachineCommand { get; set; }
        public RelayCommand UnlockSettingsCommand { get; set; }

    }
}
