using LagoVista.Client.Core;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Repos
{
    public class MachineRepo : ViewModelBase, IMachineRepo
    {
        private readonly IRestClient _restClient;
        private readonly IStorageService _storageService;
        private readonly ILogger _logger;

        public MachineRepo(IRestClient restClient, IStorageService storageService, ILogger logger)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _storageService = storageService ?? throw new ArgumentNullException(nameof(restClient));
            CurrentMachine = new Machine();
            CurrentMachine.Settings = new Manufacturing.Models.Machine();
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
            if(!String.IsNullOrEmpty(machineId))
            {
                return await LoadMachineAsync(machineId);
            }
            else
            {
                return InvokeResult.FromError("No current machine.");
            }
        }

        public async Task<InvokeResult> LoadMachineAsync(string id)
        {
            var result = await _restClient.GetAsync<DetailResponse<LagoVista.Manufacturing.Models.Machine>>($"/api/mfg/machine/{id}");
            if (result.Successful)
            {
                CurrentMachine = new Machine();
                CurrentMachine.Settings = result.Result.Model;
                _selectedMachine = result.Result.Model.CreateSummary();
                RaisePropertyChanged(nameof(SelectedMachine));
                RaisePropertyChanged(nameof(CurrentMachine));
                await _storageService.StoreKVP("current_machine_id", id);
                HasValidMachine = true;
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

    }
}
