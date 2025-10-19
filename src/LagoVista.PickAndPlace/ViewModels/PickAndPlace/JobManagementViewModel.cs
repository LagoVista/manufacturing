// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7449edf4e5a39ce83dc0d1a5bae724bc4691bf88a60787b83a70ada61f9930ef
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class JobManagementViewModel : MachineViewModelBase, IJobManagementViewModel
    {
        private readonly IRestClient _restClient;
        private readonly IStorageService _storageService;

        private readonly IPickAndPlaceJobResolverService _resolver;

        public JobManagementViewModel(IRestClient restClient, IPickAndPlaceJobResolverService resolver, IStorageService storage, IMachineRepo machineRepo, IPartsViewModel partsViewModel) : base(machineRepo)
        {
            _restClient = restClient ?? throw new ArgumentNullException(nameof(restClient));
            _storageService = storage ?? throw new ArgumentNullException(nameof(storage));
            _resolver = resolver ?? throw new ArgumentNullException(nameof(resolver));

            PartsViewModel = partsViewModel ?? throw new ArgumentNullException(nameof(partsViewModel));

            ReloadJobCommand = new RelayCommand(async () => await RefreshJob(), () => { return Job != null; });
            SetBoardOriginCommand = CreatedMachineConnectedCommand(() => Job.DefaultBoardOrigin = Machine.MachinePosition.ToPoint2D(), () => Job != null);
            CheckBoardFiducialsCommand = CreatedMachineConnectedCommand(() => CheckBoardFiducials(), () => Job != null);

            ShowSchematicCommand = CreateCommand(ShowSchematic, () => Job != null && CircuitBoard?.SchematicPDFFile != null);
            ShowComponentDetailCommand = CreateCommand(ShowComponentDetail, () => PartGroup != null);
            ShowComponentPackageDetailCommand = CreateCommand(ShowComponentPackageDetail, () => CurrentComponent != null && CurrentComponent?.ComponentPackage != null);
            ShowDataSheetCommand = CreateCommand(ShowDataSheeet, () => CurrentComponent != null && !String.IsNullOrEmpty(CurrentComponent.DataSheet));
            ResolveJobCommand = CreateCommand(ResolveJob, () => Job != null);
            ResolvePartsCommand = CreateCommand(ResolveParts, () => Job != null);

            SaveComponentPackageCommand = CreateCommand(SaveComponentPackage, () => CurrentComponentPackage != null);
            SubstitutePartCommand = CreateCommand(SubstitutePart, () => CurrentComponent != null);
            SaveSubstitutePartCommand = CreateCommand(SaveSubstitutePart, () => SelectedAvailablePart != null);
            CancelSubstitutePartCommand = CreateCommand(CancelSubstitutePart);

            GoToPartOnBoardCommand = CreatedMachineConnectedCommand(GoToPartOnBoard, () => Placement != null);

            VacuumOnCommand = CreatedMachineConnectedCommand(() => Machine.VacuumPump = true);
            VacuumOffCommand = CreatedMachineConnectedCommand(() => Machine.VacuumPump = false);
            ReadVacuumCommand = CreatedMachineConnectedCommand(ReadVacuum);

            SaveCommand = CreateCommand(async () => await SaveJobAsync(), () => Job != null);

            CreateJobRunCommand = CreateCommand(async () => await CreateJobRunAsync(), () => Job != null);
            SaveJobRunCommand = CreateCommand(async () => await SaveJobRunAsync(), () => JobRun != null);
        }

        public override async Task InitAsync()
        {
            var result = await _restClient.GetListResponseAsync<PickAndPlaceJobSummary>("/api/mfg/pnpjobs");
            if (result.Successful)
            {
                Jobs = new ObservableCollection<PickAndPlaceJobSummary>(result.Model.ToList());

                var lastJobRunId = await _storageService.GetKVPAsync<string>("last-job-run-id");
                if (!String.IsNullOrEmpty(lastJobRunId))
                {
                    await LoadJobRunAsync(lastJobRunId);
                }
                else
                {
                    var lastJobId = await _storageService.GetKVPAsync<string>("last-job-id");
                    if (!String.IsNullOrEmpty(lastJobId))
                    {
                        await LoadJobAsync(lastJobId);
                    }
                }
            }

            await base.InitAsync();
        }

        public Task<InvokeResult> RotateCurrentPartAsync(PartsGroup part, PickAndPlaceJobPlacement placement, bool rotated90, bool reverse)
        {
            var rotatePart = GetRotationAngle(part, placement, rotated90, reverse);
            Machine.RotateToolHeadRelative(-rotatePart);
            return Task<InvokeResult>.FromResult(InvokeResult.Success);
        }

        public double GetRotationAngle(PartsGroup part, PickAndPlaceJobPlacement placement, bool rotated90, bool reverse)
        {
            var inTapeAngle = 0.0;

            switch (CurrentComponent.ComponentPackage.Value.TapeRotation.Value)
            {
                case TapeRotations.MinusNinety:
                    inTapeAngle = -90;
                    break;
                case TapeRotations.Ninety:
                    inTapeAngle = 90;
                    break;
                case TapeRotations.OneEighty:
                    inTapeAngle = 180;
                    break;
            }

            if (rotated90)
            {
                inTapeAngle -= 90;
            }


            var rotatePart = inTapeAngle - placement.Rotation;
            if (reverse)
            {
                rotatePart *= -1;
            }

            rotatePart = rotatePart % 360;

            if (rotatePart > 180)
                rotatePart = rotatePart - 360;

            return rotatePart;
        }


        public async void ReadVacuum()
        {
            var result = await Machine.ReadVacuumAsync();
            if (result.Successful)
                Vacuum = result.Result;
            else
                Vacuum = 0;
        }

        public void GoToPartOnBoard()
        {
            var boardLocation = MachineConfiguration.DefaultWorkOrigin + Placement.PCBLocation;
            Machine.GotoPoint(boardLocation);
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

        void CancelSubstitutePart()
        {
            IsSubstituting = false;
        }

        void SubstitutePart()
        {
            PartsViewModel.RefreshAvailableParts();
            IsSubstituting = true;
        }



        void SaveSubstitutePart()
        {
            if (SelectedAvailablePart == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Please select a part.");
                return;
            }

            var substitutes = Job.Parts.Where(prt => prt.Component.Id == PartGroup.Component.Id);
            foreach (var substitute in substitutes)
            {
                substitute.Component = SelectedAvailablePart.Component;
            }

            SelectedAvailablePart = null;
            IsSubstituting = false;
        }

        void CheckBoardFiducials()
        {

        }

        public void ShowComponentPackageDetail()
        {
            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current component.");
                return;
            }

            if (CurrentComponent.ComponentPackage == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No component does not have package assigned.");
                return;
            }

            var url = $"https://www.nuviot.com/mfg/package/{CurrentComponent.ComponentPackage.Id}";
            System.Diagnostics.Process.Start(new ProcessStartInfo
            {
                FileName = url as string,
                UseShellExecute = true
            });
        }

        public void ShowDataSheeet()
        {
            if (CurrentComponent == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current component.");
                return;
            }

            if (String.IsNullOrEmpty(CurrentComponent.DataSheet))
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
            if (PartGroup != null)
            {
                var url = $"https://www.nuviot.com/mfg/component/{PartGroup.Component.Id}";
                System.Diagnostics.Process.Start(new ProcessStartInfo
                {
                    FileName = url as string,
                    UseShellExecute = true
                });
            }
        }

        public async void LoadComponent(string componentId)
        {
            await LoadComponentAsync(componentId);
        }

        public async Task<InvokeResult> LoadComponentAsync(string componentId)
        {
            if (!componentId.HasValidId())
            {
                CurrentComponent = null;
                return InvokeResult.Success;
            }
            else
            {
                var result = await _restClient.GetAsync<DetailResponse<Manufacturing.Models.Component>>($"/api/mfg/component/{componentId}?loadcomponent=true");
                if (result.Successful)
                {
                    CurrentComponent = result.Result.Model;
                    if (EntityHeader.IsNullOrEmpty(result.Result.Model.ComponentPackage))
                    {
                        Machine.AddStatusMessage(StatusMessageTypes.Warning, "Loaded component for execution without a component package, likely trouble ahead!");
                    }
                    else
                    {
                        CurrentComponentPackage = result.Result.Model.ComponentPackage.Value;
                    }

                    return InvokeResult.Success;
                }
                else
                {
                    return InvokeResult.FromError(result.ErrorMessage);
                }
            }
        }



        private async Task SaveJobAsync()
        {
            await _restClient.PutAsync("/api/mfg/pnpjob", Job);
        }

        public async Task SaveJobRunAsync()
        {
            await _restClient.PutAsync("/api/mfg/pnpjob/run", JobRun);
        }

        async void SaveComponentPackage()
        {
            var result = await _restClient.PutAsync("/api/mfg/component/package", CurrentComponentPackage);
            if (!result.Successful)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Could not save component package: {result.ErrorMessage}");
            }
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

                if(JobRun != null)
                {
                    foreach (var placed in JobRun.Placements)
                    {
                        var part = Job.Parts.FirstOrDefault(cmp => cmp.Component.Id == placed.Component.Id);
                        if (part == null)
                        {
                            Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Could not find part for component: {placed.Component.Text} when restoring job.");
                        }
                        else
                        {
                            var placement = part.Placements.FirstOrDefault(pl => pl.Name == placed.Name);
                            if (placement == null)
                            {
                                Machine.AddStatusMessage(StatusMessageTypes.FatalError, $"Could not find placement {placed.Name}, on ocmponent {placed.Component.Text} when restoring job.");
                            }
                            else
                            {
                                if (placed.Status.Value == JobPlacementStatuses.Placed)
                                {
                                    placement.State = EntityHeader<PnPStates>.Create(PnPStates.Placed);
                                    foreach (var transition in placed.Transitions)
                                        placement.Transitions.Add(transition);
                                }

                            }
                        }
                    }                   
                }
            }
            else
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not load job.");
        }

        public async Task CreateJobRunAsync()
        {
            if (Job == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current job.");
                return;
            }


            foreach(var pg in Job.Parts)
            {
                foreach (var plc in pg.Placements)
                    plc.State = EntityHeader<PnPStates>.Create(PnPStates.New);
            }

            var jobLoadReslut = await _restClient.GetAsync<DetailResponse<PickAndPlaceJobRun>>($"/api/mfg/pnpjob/{Job.Id}/run/factory");
            if (jobLoadReslut.Successful && jobLoadReslut.Result.Successful)
            {
                JobRun = jobLoadReslut.Result.Model;
                await _storageService.StoreKVP("last-job-run-id", JobRun.Id);
            }
            else
            {
                if(jobLoadReslut.Successful)
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, jobLoadReslut.Result.ErrorMessage);
                else
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, jobLoadReslut.ErrorMessage);

            }
        }

        public async Task LoadJobRunAsync(string jobRunId)
        {
            var jobLoadReslut = await _restClient.GetAsync<DetailResponse<PickAndPlaceJobRun>>($"/api/mfg/pnpjob/run/{jobRunId}");
            if (jobLoadReslut.Successful)
            {
                JobRun = jobLoadReslut.Result.Model;
                await LoadJobAsync(JobRun.Job.Id);
            }
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
            private set
            {
                Set(ref _job, value);
                RaiseCanExecuteChanged();
            }
        }

        private PickAndPlaceJobRun _jobRun;
        public PickAndPlaceJobRun JobRun
        {
            get => _jobRun;
            private set 
            {
                Set(ref _jobRun, value);
                RaiseCanExecuteChanged();
            }
        }

        public async Task<InvokeResult> ResetPlacementOnlineAsync()
        {
            if (JobRun == null)
            {
                return InvokeResult.FromError("No job run has been created.");
            }

            if(Placement == null)
            {
                return InvokeResult.FromError("Placement is null.");
            }

            Placement.Reset();
            var jobPlacement = Placement.ToJobRunPlacement(PartGroup);
            var result = await _restClient.PutAsync($"/api/mfg/pmpjob/run/{JobRun.Id}/placement", jobPlacement);

            return result;
        }


        public async Task<InvokeResult> CompletePlacementAsync()
        {
            if(JobRun == null)
            {
                return InvokeResult.FromError("No job run has been created.");
            }

            var jobPlacement = Placement.ToJobRunPlacement(PartGroup);
            var result = await _restClient.PutAsync($"/api/mfg/pmpjob/run/{JobRun.Id}/placement", jobPlacement);

            return result;
        
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
                    LoadJob(value.Id);
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

        public async Task<InvokeResult> SetPartGroupToPlaceAsync(PartsGroup part)
        {
            var result = await LoadComponentAsync(part.Component.Id);
            if (result.Successful)
            {
                _partGroup = part;
                RaisePropertyChanged(nameof(PartGroup));
                RaiseCanExecuteChanged();
            }
            
            return result;
        }

        public Task<InvokeResult> SetIndividualPartToPlaceAsync(PickAndPlaceJobPlacement placement)
        {
            Placement = placement;
            return Task.FromResult(InvokeResult.Success);
        }

        PartsGroup _partGroup;
        public PartsGroup PartGroup
        {
            get => _partGroup;
            set
            {
                Set(ref _partGroup, value);
                if(EntityHeader.IsNullOrEmpty( _partGroup?.Component))
                {
                    CurrentComponent = null;
                    RaiseCanExecuteChanged();
                }
                else
                {
                    LoadComponent(_partGroup.Component.Id);
                }
            }
        }

        PickAndPlaceJobPlacement _placement;
        public PickAndPlaceJobPlacement Placement
        {
            get => _placement;
            set
            {
                Set(ref _placement, value);
                RaiseCanExecuteChanged();
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
                if (value != null && value.ComponentPackage?.Value != null)
                    CurrentComponentPackage = value.ComponentPackage.Value;
                else
                    CurrentComponentPackage = null;
            }
        }

        public ComponentPackage _currentComponentPackage;
        public ComponentPackage CurrentComponentPackage
        {
            get => _currentComponentPackage;
            set
            {
                Set(ref _currentComponentPackage, value);
                RaiseCanExecuteChanged();
            }
        }

        private bool _isSubstituting;
        public bool IsSubstituting
        {
            get => _isSubstituting;
            set => Set(ref _isSubstituting, value);
        }

        AvailablePart _selectedAvailablePart;
        public AvailablePart SelectedAvailablePart
        {
            get => _selectedAvailablePart;
            set
            {
                Set(ref _selectedAvailablePart, value);
                RaiseCanExecuteChanged();
            }
        }

        private long _vacuum;
        public long Vacuum
        {
            get => _vacuum;
            set => Set(ref _vacuum, value);
        }

        public RelayCommand SaveCommand { get; }

        public RelayCommand ReloadJobCommand { get; }
        public RelayCommand RefreshConfigurationPartsCommand { get; }
        public RelayCommand SetBoardOriginCommand { get; }
        public RelayCommand CheckBoardFiducialsCommand { get; }

        public RelayCommand ShowComponentDetailCommand { get; }
        public RelayCommand ShowComponentPackageDetailCommand { get; }

        public RelayCommand ResolvePartsCommand { get; }
        public RelayCommand ResolveJobCommand { get; }
        public RelayCommand ShowSchematicCommand { get; }
        public RelayCommand ShowDataSheetCommand { get; }

        public RelayCommand SubstitutePartCommand { get; }
        public RelayCommand SaveSubstitutePartCommand { get; }
        public RelayCommand CancelSubstitutePartCommand { get; }

        public RelayCommand GoToPartOnBoardCommand { get; }

        public RelayCommand SaveComponentPackageCommand { get; }

        public RelayCommand VacuumOnCommand { get;  }
        public RelayCommand ReadVacuumCommand { get; }
        public RelayCommand VacuumOffCommand { get; }

        public RelayCommand CreateJobRunCommand { get; }
        public RelayCommand SaveJobRunCommand { get; }
    }
}
