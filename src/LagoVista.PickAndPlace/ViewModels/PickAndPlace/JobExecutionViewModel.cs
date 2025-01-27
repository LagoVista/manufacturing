using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class JobExecutionViewModel : MachineViewModelBase, IJobExecutionViewModel
    {

        IStripFeederViewModel _stripFeederViewModel;
        IAutoFeederViewModel _autoFeederViewModel;
        IFeederViewModel _feederViewModel;
        IPartInspectionViewModel _partInspectionVM;
        IJobRunViewModel _jobRunVM;

        public JobExecutionViewModel(IMachineRepo machineRepo, ICircuitBoardViewModel pcbVM, IJobManagementViewModel pnpJobVM, IPartInspectionViewModel partInspectionVM, IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel) : base(machineRepo)
        {
            JobVM = pnpJobVM ?? throw new ArgumentNullException(nameof(pnpJobVM));
            PcbVM = pcbVM ?? throw new ArgumentNullException(nameof(pcbVM));
            _stripFeederViewModel = stripFeederViewModel ?? throw new ArgumentNullException(nameof(stripFeederViewModel));
            _autoFeederViewModel = autoFeederViewModel ?? throw new ArgumentNullException(nameof(autoFeederViewModel));
            _partInspectionVM = partInspectionVM ?? throw new ArgumentNullException(nameof(partInspectionVM));
        }

        private InvokeResult ResolveFeeder()
        {
            if (JobVM.PickAndPlaceJobPart == null)
            {
                return InvokeResult.FromError("No part to place.");
            }

            JobVM.Placement = JobVM.PickAndPlaceJobPart.Placements.FirstOrDefault();
            if (JobVM.Placement == null)
            {
                return InvokeResult.FromError("Could not identify first placement.");
            }

            if (!EntityHeader.IsNullOrEmpty(JobVM.PickAndPlaceJobPart.StripFeeder))
            {
                _stripFeederViewModel.Current = _stripFeederViewModel.Feeders.SingleOrDefault(sf => sf.Id == JobVM.PickAndPlaceJobPart.StripFeeder.Id);
                if (_stripFeederViewModel.Current == null)
                {
                    return InvokeResult.FromError($"Could not find strip feeder {JobVM.PickAndPlaceJobPart.StripFeeder.Text}.");
                }

                if (EntityHeader.IsNullOrEmpty(JobVM.PickAndPlaceJobPart.StripFeederRow))
                {
                    return InvokeResult.FromError($"Strip feeder {JobVM.PickAndPlaceJobPart.StripFeeder} does ont have row.");
                }

                _stripFeederViewModel.CurrentRow = _stripFeederViewModel.Current.Rows.SingleOrDefault(r => r.Id == JobVM.PickAndPlaceJobPart.StripFeederRow.Id);
                if (_stripFeederViewModel.CurrentRow == null)
                {
                    return InvokeResult.FromError($"On Strip feeder {JobVM.PickAndPlaceJobPart.StripFeeder}, could not find row {JobVM.PickAndPlaceJobPart.StripFeederRow.Text}.");
                }

                _feederViewModel = _stripFeederViewModel;

            }
            else if (!EntityHeader.IsNullOrEmpty(JobVM.PickAndPlaceJobPart.AutoFeeder))
            {
                _autoFeederViewModel.Current = _autoFeederViewModel.Feeders.SingleOrDefault(sf => sf.Id == JobVM.PickAndPlaceJobPart.AutoFeeder.Id);
                if (_autoFeederViewModel.Current == null)
                {
                    return InvokeResult.FromError($"Could not find auto feeder {JobVM.PickAndPlaceJobPart.AutoFeeder}");
                }

                _feederViewModel = _autoFeederViewModel;
            }
            else
            {
                return InvokeResult.FromError("Selected component does not have an assocaited feeder.");
            }

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> PlaceCycleAsync()
        {
            var result = ResolveFeeder();
            if (!result.Successful) return result;

            result = JobVM.PickAndPlaceJobPart.Validate();
            if (!result.Successful) return result;

            result = await _feederViewModel.PickCurrentPartAsync();
            if (!result.Successful) return result;

            result = await _partInspectionVM.InspectAsync();
            if (!result.Successful) return result;

            result = await PcbVM.PlacePartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement);
            if (!result.Successful) return result;

            result = await PcbVM.InspectPartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement);
            if (!result.Successful) return result;

            //  _jobRunVM.Current.Placements.Add(JobVM.Placement);

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


        public IJobManagementViewModel JobVM { get; }
        public ICircuitBoardViewModel PcbVM { get; }


        public RelayCommand StartJobCommand { get; }
        public RelayCommand StopJobCommand { get;  }
        public RelayCommand PauseJobCommand { get; set; }
    }
}
