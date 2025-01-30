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
            JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.FeederResolved);

            result = JobVM.PickAndPlaceJobPart.Validate();
            if (!result.Successful) return result;
            JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.Validated);

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
