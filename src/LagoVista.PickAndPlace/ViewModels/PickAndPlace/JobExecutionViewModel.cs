using LagoVista.Client.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using RingCentral;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class JobExecutionViewModel : JobExecutionBaseViewModel, IJobExecutionViewModel
    {
        public enum JobState
        {
            [EnumLabel("idle", ManufacturingResources.Names.PnPJobState_Idle, typeof(ManufacturingResources))]
            Idle,
            [EnumLabel("new", ManufacturingResources.Names.PnPJobState_New, typeof(ManufacturingResources))]
            New,
            [EnumLabel("running", ManufacturingResources.Names.PnPJobState_Running, typeof(ManufacturingResources))]
            Running,
            [EnumLabel("paused", ManufacturingResources.Names.PnPJobState_Paused, typeof(ManufacturingResources))]
            Paused,
            [EnumLabel("completed", ManufacturingResources.Names.PnPJobState_Completed, typeof(ManufacturingResources))]
            Completed,
            [EnumLabel("aborted", ManufacturingResources.Names.PnPJobState_Aborted, typeof(ManufacturingResources))]
            Aborted,
            [EnumLabel("errored", ManufacturingResources.Names.PnPJobState_Error, typeof(ManufacturingResources))]
            Errored,
        }

        public JobExecutionViewModel(IRestClient restClient, ICircuitBoardViewModel pcbVM, IPartInspectionViewModel partInspectionVM, IVacuumViewModel vacuumViewModel,
                                     IJobManagementViewModel jobVM, IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel, IMachineRepo machineRepo) :
                                     base(restClient, pcbVM, partInspectionVM, vacuumViewModel, jobVM, stripFeederViewModel, autoFeederViewModel, machineRepo)
        {
            StartJobCommand = CreatedMachineConnectedCommand(async () => await StartJobAsync(), () => JobVM.Job != null && State.Value == JobState.New);
            PauseJobCommand = CreatedMachineConnectedCommand(async () => await PauseJobAsync(), () => JobVM.Job != null && State.Value == JobState.Running);
            ResumeJobCommand = CreatedMachineConnectedCommand(async () => await ResumeJobAsync(), () => JobVM.Job != null && State.Value == JobState.Paused);
            ResetJobCommand = CreatedMachineConnectedCommand(async () => await ResetJobAsync(), () => JobVM.Job != null && State.Value != JobState.Running && State.Value != JobState.New);
            AbortJobCommand = CreatedMachineConnectedCommand(async () => await AbortJobAsync(), () => JobVM.Job != null && State.Value == JobState.Running || State.Value == JobState.Paused);

            PlaceGroupPartCommand = CreatedMachineConnectedCommand(async () => await PartCycleAsync(), () => JobVM.Job != null && JobVM.PartGroup != null);
            PlaceIndependentPartCommand = CreatedMachineConnectedCommand(async () => await PlaceCycleAsync(), () => JobVM.Job != null && JobVM.PartGroup != null && JobVM.Placement != null);
            ResetIndpentPartCommand = CreatedMachineConnectedCommand(async () => await ResetIndependentPartAsync(), () => JobVM.Job != null && JobVM.PartGroup != null && JobVM.Placement != null);

            JobVM.PropertyChanged += JobVM_PropertyChanged;
        }

        private void JobVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(IJobManagementViewModel.Job) ||
               e.PropertyName == nameof(IJobManagementViewModel.PartGroup) || 
               e.PropertyName == nameof(IJobManagementViewModel.Placement))
                RaiseCanExecuteChanged();
        }

        public async Task PlaceIndividualPart()
        {
            State = EntityHeader<JobState>.Create(JobState.Running);

            var result = await PlaceCycleAsync();
            if(result.Successful) 
                State = EntityHeader<JobState>.Create(JobState.Completed);
            else
                State = EntityHeader<JobState>.Create(JobState.Errored);

        }

        public Task ResetIndependentPartAsync()
        {
            if(JobVM.Placement == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not reset, not placement.");
                return Task.CompletedTask;
            }

            Machine.VacuumPump = false;

            JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.New);
            return Task.CompletedTask;
        }

        public async Task<InvokeResult> PlaceCycleAsync()
        {
            if (JobVM.CurrentComponent == null)
                return InvokeResult.FromError("No current component");

            if (JobVM.CurrentComponentPackage == null)
                return InvokeResult.FromError("No current component package");

            if (JobVM.Placement == null)
                return InvokeResult.FromError("No placement");

            var result = ResolveFeeder();
            if (!result.Successful) return result;
            JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.FeederResolved);

            result = JobVM.PartGroup.Validate();
            if (!result.Successful) return result;
                JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.Validated);
                result = await ActiveFeederViewModel.MoveToPartInFeederAsync();
                if (!result.Successful) return result;
                JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.PartCenteredOnFeeder);


            if (JobVM.CurrentComponentPackage.CheckInFeeder)
            {
                result = await ActiveFeederViewModel.CenterOnPartAsync();
                if (!result.Successful) return result;
                JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.PartPicked);
            }

            result = await ActiveFeederViewModel.PickCurrentPartAsync();
            if (!result.Successful) return result;
            JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.PartPicked);

            if (JobVM.CurrentComponentPackage.CheckForPartPrecence)
            {
                result = await VacuumViewModel.CheckPartPresent(JobVM.CurrentComponent, 2500, JobVM.CurrentComponentPackage.PresenseVacuumOverride);
                if (!result.Successful) return result;
            }

            if (JobVM.CurrentComponentPackage.CompensateForPickError)
            { 
                result = await PartInspectionVM.CenterPartAsync(JobVM.CurrentComponent, JobVM.Placement, JobVM.CurrentComponentPackage.PartInspectionAlgorithm);
                if (!result.Successful) return result;
                JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.PickErrorCompensated);
            }
            else
            {
                JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.Inspecting);
                result = await PartInspectionVM.InspectAsync(JobVM.CurrentComponent);
                if (!result.Successful) return result;
                JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.Inspected);
            }

            result = await PcbVM.PlacePartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement);
            if (!result.Successful) return result;

            //result = await PcbVM.InspectPartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement);
            //if (!result.Successful) return result;

            result = await ActiveFeederViewModel.NextPartAsync();
            if (!result.Successful) return result;

            result = await JobVM.CompletePlacementAsync();
            if (!result.Successful) return result;

            return InvokeResult.Success;
        }

        EntityHeader<JobState> _state = EntityHeader<JobState>.Create(JobState.Idle);
        public EntityHeader<JobState> State
        {
            get => _state;
            set {
                Set(ref _state, value);
                RaiseCanExecuteChanged();
             }
        }

        public async Task<InvokeResult> PartCycleAsync()
        {
            foreach (var placement in JobVM.PartGroup.Placements)
            {
                var result = await JobVM.SetIndividualPartToPlaceAsync(placement);
                if (!result.Successful) return result;

                JobVM.Placement = placement;
                result = await PlaceCycleAsync();
                if (!result.Successful) return result;
            }

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> JobCycleAsync()
        {
            foreach(var part in JobVM.Job.Parts)
            {
                var result = await JobVM.SetPartGroupToPlaceAsync(part);
                if (!result.Successful) return result;

                result = await PartCycleAsync();
                if (!result.Successful) return result;
            }

            return InvokeResult.Success;
        }

        public async Task StartJobAsync()
        {
            var result = await PcbVM.AlignBoardAsync();
            if(result.Successful)
            {
                await JobCycleAsync();
            }
            else
            {

            }
        }

        public async Task PauseJobAsync()
        {

        }

        public async Task ResumeJobAsync()
        {

        }

        public async Task ResetJobAsync()
        {

        }

        public async Task StopJobAsync()
        {

        }

        public async Task AbortJobAsync()
        {

        }        

        public RelayCommand PlaceIndependentPartCommand { get; set; }
        public RelayCommand ResetIndpentPartCommand { get; set; }
        public RelayCommand PlaceGroupPartCommand { get; set; }


        public RelayCommand StartJobCommand { get; }
        public RelayCommand AbortJobCommand { get;  }
        public RelayCommand PauseJobCommand { get; set; }
        public RelayCommand ResumeJobCommand { get; set; }
        public RelayCommand ResetJobCommand { get; set; }    
    }
}
