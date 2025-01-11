﻿using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PickAndPlaceJobViewModel : MachineViewModelBase, IPickAndPlaceJobViewModel
    {
        private readonly IRestClient _restClient;
        private readonly ILogger _logger;
        private readonly IStorageService _storageService;
        
        private readonly IPickAndPlaceJobResolverService _resolver;

        public PickAndPlaceJobViewModel(IRestClient restClient, IPickAndPlaceJobResolverService resolver, IStorageService storage, IMachineRepo machineRepo, IPartsViewModel partsViewModel) : base(machineRepo)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _storageService = storage ?? throw new ArgumentNullException(nameof(storage));
            PartsViewModel = partsViewModel ?? throw new ArgumentNullException(nameof(partsViewModel));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

            ReloadJobCommand = new RelayCommand(async () => await RefreshJob(), () => { return Job != null; });
            SaveCommand = new RelayCommand(async () => await SaveJobAsync(), () => { return Job != null; });
            SetBoardOriginCommand = CreatedMachineConnectedCommand(() => Job.DefaultBoardOrigin = Machine.MachinePosition.ToPoint2D(), () => Job != null);
            CheckBoardFiducialsCommand = CreatedMachineConnectedCommand(() => CheckBoardFiducials(), () => Job != null);

            ShowSchematicCommand = CreatedCommand(ShowSchematic, () => Job != null && CircuitBoard.SchematicPDFFile != null);
            ShowComponentDetailCommand = CreatedCommand(ShowComponentDetail, () => PickAndPlaceJobPart != null);
            ShowDataSheetCommand = CreatedCommand(ShowDataSheeet, () => CurrentComponent != null && !String.IsNullOrEmpty(CurrentComponent.DataSheet));
            ResolveJobCommand = CreatedCommand(ResolveJob, () => Job != null);
            ResolvePartsCommand = CreatedCommand(ResolveParts, () => Job != null);
        }

        public override async Task InitAsync()
        {
            var result = await _restClient.GetListResponseAsync<PickAndPlaceJobSummary>("/api/mfg/pnpjobs");
            if (result.Successful)
            {
                Jobs = new ObservableCollection<PickAndPlaceJobSummary>(result.Model.ToList());
                var lastJobId = await _storageService.GetKVPAsync<string>("last-job-id");
                if (!String.IsNullOrEmpty(lastJobId))
                {
                    await LoadJobAsync(lastJobId);
                }
            }

            await base.InitAsync();
        }

        void ResolveParts()
        {
            var result = _resolver.ResolveParts(Job);
            if (result.Successful)
            {

            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
        }

        void ResolveJob()
        {
            var result = _resolver.ResolveJobAsync(MachineConfiguration, Job, CircuitBoard, PartsViewModel.StripFeederViewModel.Feeders, PartsViewModel.AutoFeederViewModel.Feeders);
            if (result.Successful)
            {

            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
            }
        }

        void CheckBoardFiducials()
        {

        }


        public void ShowDataSheeet()
        {
            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current component.");
            }
            
            if(String.IsNullOrEmpty(CurrentComponent.DataSheet))
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Data sheet not available");
            }
                        
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = CurrentComponent.DataSheet,
                UseShellExecute = true
            });
        }

        public void ShowSchematic()
        {
            if (CircuitBoard?.SchematicPDFFile != null)
            {                
                var url = $"https://www.nuviot.com/api/media/resource/{this.Job.OwnerOrganization.Id}/{this.CircuitBoard.SchematicPDFFile.Id}/download";
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url as string,
                    UseShellExecute = true
                });
            }
            else
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No schematic available.");
            }
        }

        public void ShowComponentDetail()
        {
            if (PickAndPlaceJobPart != null)
            {
                var url = $"https://www.nuviot.com/mfg/component/{PickAndPlaceJobPart.Component.Id}";
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url as string,
                    UseShellExecute = true
                });
            }
        }

        public async void LoadComponent(string componentId)
        {
            if (!componentId.HasValidId())
            {
                CurrentComponent = null;
            }
            else
            {
                var component = await _restClient.GetAsync<DetailResponse<Manufacturing.Models.Component>>($"/api/mfg/component/{componentId}");
                CurrentComponent = component.Result.Model;
            }
        }

        private async Task SaveJobAsync()
        {
            await _restClient.PutAsync("/api/mfg/pnpjob", Job);
        }

        private async Task RefreshJob()
        {
            await LoadJobAsync(this.Job.Id);
        }

        private async Task LoadJobAsync(string jobId)
        {
            var jobLoadReslut = await _restClient.GetAsync<DetailResponse<PickAndPlaceJob>>($"/api/mfg/pnpjob/{jobId}");
            if (jobLoadReslut.Successful)
            {
                Job = jobLoadReslut.Result.Model;
                CircuitBoard = Job.BoardRevision;
                await _storageService.StoreKVP<string>("last-job-id", jobId);
            }
            else
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not load job.");
        }


        private async void LoadJob(string jobId)
        {
            await LoadJobAsync(jobId);
        }

        public IPartsViewModel PartsViewModel { get; }

        ObservableCollection<PickAndPlaceJobSummary> _jobs;
        public ObservableCollection<PickAndPlaceJobSummary> Jobs
        {
            get => _jobs;
            set => Set(ref _jobs, value);
        }

        private PickAndPlaceJob _job;
        public PickAndPlaceJob Job
        {
            get => _job;
            set => Set(ref _job, value);
        }

        PickAndPlaceJobSummary _selectedJob;
        public PickAndPlaceJobSummary SelectedJob
        {
            get => _selectedJob;
            set
            {
                RaisePropertyChanged(nameof(SelectedJob));
                if (value != null && _selectedJob?.Id != value?.Id)
                {
                    LoadJob(value?.Id);
                }

                Set(ref _selectedJob, value);
            }
        }

        CircuitBoardRevision _board;
        public CircuitBoardRevision CircuitBoard
        {
            get => _board;
            set => Set(ref _board, value);
        }


        PickAndPlaceJobPart _jobPart;
        public PickAndPlaceJobPart PickAndPlaceJobPart
        {
            get => _jobPart;
            set
            {
                Set(ref _jobPart, value);
                if(EntityHeader.IsNullOrEmpty( _jobPart?.Component))
                {
                    CurrentComponent = null;
                    RaiseCanExecuteChanged();
                }
                else
                {
                    LoadComponent(_jobPart.Component.Id);
                }
            }
        }

        Component _currentComponent;

        public Component CurrentComponent
        {
            get => _currentComponent;
            set
            {
                Set(ref _currentComponent, value);
                RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SaveCommand { get; }

        public RelayCommand ReloadJobCommand { get; }
        public RelayCommand RefreshConfigurationPartsCommand { get; }
        public RelayCommand SetBoardOriginCommand { get; }
        public RelayCommand CheckBoardFiducialsCommand { get; }

        public RelayCommand ShowComponentDetailCommand { get; }

        public RelayCommand ResolvePartsCommand { get; }
        public RelayCommand ResolveJobCommand { get; }
        public RelayCommand ShowSchematicCommand { get; }
        public RelayCommand ShowDataSheetCommand { get; }

    }
}
