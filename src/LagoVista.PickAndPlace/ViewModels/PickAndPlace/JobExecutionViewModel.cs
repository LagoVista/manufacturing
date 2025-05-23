﻿using LagoVista.Client.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using RingCentral;
using System;
using System.Diagnostics;
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
            PlaceIndependentPartCommand = CreatedMachineConnectedCommand(async () =>
            {
                var result = await PlaceCycleAsync();
                if (result.Successful)
                {
                    await Machine.MoveToCameraAsync();
                    Machine.AddStatusMessage(StatusMessageTypes.Info, $"Completed part {JobVM.Placement.Name} in {JobVM.Placement.Duration}");
                    var idx = JobVM.PartGroup.Placements.IndexOf(JobVM.Placement);
                    idx++;
                    if (idx < JobVM.PartGroup.Placements.Count)
                        JobVM.Placement = JobVM.PartGroup.Placements[idx ];
                    else
                        JobVM.Placement = null;
                }
                else
                {
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
                }

            }, () => JobVM.Job != null && JobVM.PartGroup != null && JobVM.Placement != null);
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

        public async Task ResetIndependentPartAsync()
        {
            if(JobVM.Placement == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not reset, not placement.");
            }

            Machine.VacuumPump = false;

            Machine.SetMode(OperatingMode.Manual);

            await JobVM.ResetPlacementOnlineAsync();
         }

        public async Task<InvokeResult> PlaceCycleAsync()
        {
            Machine.SetMode(OperatingMode.PlacingParts);

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
            await Machine.SpinUntilIdleAsync();
            JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.AtFeeder);

            result = await ActiveFeederViewModel.SetVisionProfile();
            if (!result.Successful) return result;
            JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.SetVisionProfile);


            //if (JobVM.CurrentComponentPackage.CheckInFeeder)
            //{
            //    result = await ActiveFeederViewModel.CenterOnPartAsync();
            //    if (!result.Successful) return result;
            //    JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.PartCenteredOnFeeder);
            //}

            result = await ActiveFeederViewModel.PickCurrentPartAsync(JobVM.Placement);
            if (!result.Successful) return result;

            JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.PartPicked);
            
            await Machine.SpinUntilIdleAsync();

            if (JobVM.CurrentComponentPackage.CheckForPartPrecence)
            {
                result = await VacuumViewModel.CheckPartPresent(JobVM.CurrentComponent, 2500, JobVM.CurrentComponentPackage.PresenseVacuumOverride);
                JobVM.Placement.State = EntityHeader<PnPStates>.Create(PnPStates.PartDetected);

                if (!result.Successful) return result;
            }

            var angle = JobVM.GetRotationAngle(JobVM.PartGroup, JobVM.Placement, true, false);
          
            if (JobVM.CurrentComponentPackage.CompensateForPickError)
            { 
                result = await PartInspectionVM.CenterPartAsync(JobVM.CurrentComponent, JobVM.Placement, JobVM.CurrentComponentPackage.PartInspectionAlgorithm, angle);
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

            if (JobVM.JobRun != null)
            {
                result = await JobVM.CompletePlacementAsync();
                if (!result.Successful) return result;
            }

            Machine.SetMode(OperatingMode.Manual);

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
                if (placement.State.Value == PnPStates.New)
                {
                    var result = await JobVM.SetIndividualPartToPlaceAsync(placement);
                    if (!result.Successful)
                    {
                        Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
                        return result;
                    }

                    JobVM.Placement = placement;
                    result = await PlaceCycleAsync();
                    if (!result.Successful)
                    {
                        Machine.AddStatusMessage(StatusMessageTypes.FatalError, result.ErrorMessage);
                        return result;
                    }
                }
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

        public Task PauseJobAsync()
        {
            throw new NotImplementedException();
        }

        public Task ResumeJobAsync()
        {
            throw new NotImplementedException();
        }

        public Task ResetJobAsync()
        {
            throw new NotImplementedException();
        }

        public Task StopJobAsync()
        {
            throw new NotImplementedException();
        }

        public Task AbortJobAsync()
        {
            throw new NotImplementedException();
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
