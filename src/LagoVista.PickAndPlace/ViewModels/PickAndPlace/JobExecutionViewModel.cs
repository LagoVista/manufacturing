using LagoVista.Client.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using RingCentral;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class JobExecutionViewModel : JobExecutionBaseViewModel, IJobExecutionViewModel
    {

        public enum PnPStates
        {
            [EnumLabel("idle", ManufacturingResources.Names.PnPState_Idle, typeof(ManufacturingResources))]
            Idle,
            [EnumLabel("error", ManufacturingResources.Names.PnpState_Error, typeof(ManufacturingResources))]
            Error,
            [EnumLabel("feederresolved", ManufacturingResources.Names.PnpState_FeederResolved, typeof(ManufacturingResources))]
            FeederResolved,
            [EnumLabel("validated", ManufacturingResources.Names.PnpState_Validated, typeof(ManufacturingResources))]
            Validated,
            [EnumLabel("atfeeder", ManufacturingResources.Names.PnpState_AtFeeder, typeof(ManufacturingResources))]
            AtFeeder,
            [EnumLabel("partpicked", ManufacturingResources.Names.PnpState_PartPicked, typeof(ManufacturingResources))]
            PartPicked,
            [EnumLabel("detectingpart", ManufacturingResources.Names.PnpState_DetectingPart, typeof(ManufacturingResources))]
            DetectingPart,
            [EnumLabel("partdetected", ManufacturingResources.Names.PnpState_PartDetected, typeof(ManufacturingResources))]
            PartDetected,
            [EnumLabel("partnotdetected", ManufacturingResources.Names.PnpState_PartNotDetected, typeof(ManufacturingResources))]
            PartNotDetected,
            [EnumLabel("inspecting", ManufacturingResources.Names.PnPState_Inspecting, typeof(ManufacturingResources))]
            Inspecting,
            [EnumLabel("inspected", ManufacturingResources.Names.PnPState_Inspected, typeof(ManufacturingResources))]
            Inspected,
            [EnumLabel("pickercompensating", ManufacturingResources.Names.PnPState_PickErrorCompensating, typeof(ManufacturingResources))]
            PickErrorCompensating,
            [EnumLabel("pickerrorcompensated", ManufacturingResources.Names.PnPState_PickErrorCompensated, typeof(ManufacturingResources))]
            PickErrorCompensated,
            [EnumLabel("pickerronotcompensated", ManufacturingResources.Names.PnPState_PickErrorNotCompensated, typeof(ManufacturingResources))]
            PickErrorNotCompensated,
            [EnumLabel("atplacelocation", ManufacturingResources.Names.PnPState_AtPlaceLocation, typeof(ManufacturingResources))]
            AtPlaceLocation,
            [EnumLabel("onboard", ManufacturingResources.Names.PnPState_OnBoard, typeof(ManufacturingResources))]
            OnBoard,
            [EnumLabel("placed", ManufacturingResources.Names.PnPState_Placed, typeof(ManufacturingResources))]
            Placed,
            [EnumLabel("advanced", ManufacturingResources.Names.PnPState_Advanced, typeof(ManufacturingResources))]
            Advanced,
            [EnumLabel("placementcompleted", ManufacturingResources.Names.PnPState_PlacementCompleted, typeof(ManufacturingResources))]
            PlacementCompleted,
            [EnumLabel("partcompleted", ManufacturingResources.Names.PnPState_JobCompleted, typeof(ManufacturingResources))]
            PartCompleted,
            [EnumLabel("jobcompleted", ManufacturingResources.Names.JobState_Completed, typeof(ManufacturingResources))]
            JobCompleted,
        }
     

        public JobExecutionViewModel(IRestClient restClient, ICircuitBoardViewModel pcbVM, IPartInspectionViewModel partInspectionVM, IVacuumViewModel vacuumViewModel,
                                     IJobManagementViewModel jobVM, IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel, IMachineRepo machineRepo) :
                                     base(restClient, pcbVM, partInspectionVM, vacuumViewModel, jobVM, stripFeederViewModel, autoFeederViewModel, machineRepo)
        {
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
            State = EntityHeader<PnPStates>.Create(PnPStates.FeederResolved);

            result = JobVM.PickAndPlaceJobPart.Validate();
            if (!result.Successful) return result;
            State = EntityHeader<PnPStates>.Create(PnPStates.Validated);

            result = await ActiveFeederViewModel.PickCurrentPartAsync();
            if (!result.Successful) return result;
            State = EntityHeader<PnPStates>.Create(PnPStates.PartPicked);

            if (JobVM.CurrentComponentPackage.CheckForPartPrecence)
            {
                result = await VacuumViewModel.CheckPartPresent(JobVM.CurrentComponent, 2500, JobVM.CurrentComponentPackage.PresenseVacuumOverride);
                if (!result.Successful) return result;
            }

            if (JobVM.CurrentComponentPackage.CompensateForPickError)
            { 
                result = await PartInspectionVM.CenterPartAsync(JobVM.CurrentComponent, JobVM.Placement, JobVM.CurrentComponentPackage.PartInspectionAlgorithm);
                if (!result.Successful) return result;
                State = EntityHeader<PnPStates>.Create(PnPStates.PickErrorCompensated);
            }
            else
            {
                result = await PartInspectionVM.InspectAsync(JobVM.CurrentComponent);
            }

            result = await PcbVM.PlacePartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement);
            if (!result.Successful) return result;

            result = await PcbVM.InspectPartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement);
            if (!result.Successful) return result;

            result = await ActiveFeederViewModel.NextPartAsync();
            if (!result.Successful) return result;

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> PartCycle()
        {
            foreach (var placement in JobVM.PickAndPlaceJobPart.Placements)
            {
                JobVM.Placement = placement;
                var result = await PlaceCycleAsync();
                if (!result.Successful) return result;
            }

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> JobCycleAsync()
        {
            foreach(var part in JobVM.Job.Parts)
            {
                JobVM.PickAndPlaceJobPart = part;
                var result = await PartCycle();
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

        public async Task PauseJob()
        {

        }

        public async Task StopJob()
        {

        }

        private EntityHeader<PnPStates> _state;
        public EntityHeader<PnPStates> State
        {
            get => _state;
            set => Set(ref _state, value);
        }

        public RelayCommand StartJobCommand { get; }
        public RelayCommand StopJobCommand { get;  }
        public RelayCommand PauseJobCommand { get; set; }
    }
}
