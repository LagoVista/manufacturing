using LagoVista.Client.Core;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class DryRunViewModel : JobExecutionBaseViewModel, IDryRunViewModel
    {

        public DryRunViewModel(IRestClient restClient, ICircuitBoardViewModel pcbVM, IPartInspectionViewModel partInspectionVM, IVacuumViewModel vacuumViewModel,
                               IJobManagementViewModel jobVM, IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel, IMachineRepo machineRepo) : 
                                base(restClient, pcbVM, partInspectionVM, vacuumViewModel, jobVM, stripFeederViewModel, autoFeederViewModel, machineRepo)
        {
            MoveToPartInFeederCommand = CreatedMachineConnectedCommand(MoveToPartInFeeder, () => JobVM.CurrentComponent != null);
            PickPartCommand = CreatedMachineConnectedCommand(PickPart, () => JobVM.CurrentComponent != null);
            InspectPartCommand = CreatedMachineConnectedCommand(InspectPart, () => JobVM.CurrentComponent != null);
            CenterInspectedPartCommand = CreatedMachineConnectedCommand(CenterInspectedPart, () => JobVM.CurrentComponent != null);
            RecyclePartCommand = CreatedMachineConnectedCommand(RecyclePart, () => JobVM.CurrentComponent != null);

            PlacePartCommand = CreatedMachineConnectedCommand(() => PcbVM.PlacePartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement), () => JobVM.Placement != null);

            GoToPartOnBoardCommand = CreatedMachineConnectedCommand(() => PcbVM.GoToPartOnBoardAsync(JobVM.PartGroup, JobVM.Placement), () => JobVM.Placement != null);
            InspectPartOnBoardCommand = CreatedMachineConnectedCommand(() => PcbVM.InspectPartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement), () => JobVM.Placement != null);
            
            PickPartFromBoardCommand = CreatedMachineConnectedCommand(async () =>
            {
                await PcbVM.PickPartFromBoardAsync(JobVM.CurrentComponent, JobVM.Placement);
                CheckPartPresent();
                InspectPart();
            }, () => JobVM.Placement != null);

            RotatePartCommand = CreatedMachineConnectedCommand(() => JobVM.RotateCurrentPartAsync(JobVM.PartGroup, JobVM.Placement, ActiveFeederViewModel != null, false), () => JobVM.Placement != null);
            RotateBackPartCommand = CreatedMachineConnectedCommand(() => JobVM.RotateCurrentPartAsync(JobVM.PartGroup, JobVM.Placement, ActiveFeederViewModel != null, true), () => JobVM.Placement != null);

            ClonePartInspectionVisionProfileCommand = CreatedCommand(ClonePartInspectionVisionProfile, () => JobVM.CurrentComponentPackage != null);
            ClonePartInTapeVisionProfileCommand = CreatedCommand(ClonePartInTapeVisionProfile, () => JobVM.CurrentComponentPackage != null);
            ClonePartOnBoardVisionProfileCommand = CreatedCommand(ClonePartOnBoardisionProfile, () => JobVM.CurrentComponentPackage != null);

            CheckPartPresentCommand = CreatedCommand(CheckPartPresent, () => JobVM.CurrentComponent != null);
            CheckNoPartPresentCommand = CreatedCommand(CheckNoPartPresent, () => JobVM.CurrentComponent != null);

            NextPartCommand = CreatedCommand(NextPart, () => ActiveFeederViewModel != null);
        }

        public void ClonePartInspectionVisionProfile()
        {
            var partInspectionCampera = MachineConfiguration.Cameras.SingleOrDefault(cam => cam.CameraType.Value == Manufacturing.Models.CameraTypes.PartInspection);
            if(partInspectionCampera == null)
            {
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, "Could not find part inspection camera.");
                return;
            }
            var newName = $"{MachineConfiguration.Cameras.First().CurrentVisionProfile.Name} ({JobVM.CurrentComponentPackage.Name})";
            var newProfile = JsonConvert.DeserializeObject<VisionProfile>(JsonConvert.SerializeObject(MachineConfiguration.Cameras.First().CurrentVisionProfile));
            newProfile.Name = newName;
            newProfile.Id = Guid.NewGuid().ToId();
            JobVM.CurrentComponentPackage.PartInspectionVisionProfile = newProfile;
            Machine.SetVisionProfile(Manufacturing.Models.CameraTypes.PartInspection, VisionProfileSource.ComponentPackage, JobVM.CurrentComponentPackage.Id, newProfile);
            JobVM.SaveComponentPackageCommand.Execute(null);
        }

        public async void NextPart()
        {
            await ActiveFeederViewModel.NextPartAsync();
            var idx = JobVM.PartGroup.Placements.IndexOf(JobVM.Placement);
            if(idx == -1)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not find current placement.");
            }
            idx++;
            if(JobVM.PartGroup.Placements.Count > idx)
            {
                JobVM.Placement = JobVM.PartGroup.Placements[idx];
                MoveToPartInFeeder();
            }
            else
            {
                JobVM.Placement = null;
            }


        }

        public void ClonePartOnBoardisionProfile()
        {
            var positionCamera = MachineConfiguration.Cameras.SingleOrDefault(cam => cam.CameraType.Value == Manufacturing.Models.CameraTypes.Position);
            if (positionCamera == null)
            {
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, "Could not find part position camera.");
                return;
            }
            var newName = $"{MachineConfiguration.Cameras.First().CurrentVisionProfile.Name} ({JobVM.CurrentComponentPackage.Name})";
            var newProfile = JsonConvert.DeserializeObject<VisionProfile>(JsonConvert.SerializeObject(MachineConfiguration.Cameras.First().CurrentVisionProfile));
            newProfile.Name = newName;
            newProfile.Id = Guid.NewGuid().ToId();
            JobVM.CurrentComponentPackage.PartOnBoardVisionProfile = newProfile;
            Machine.SetVisionProfile(Manufacturing.Models.CameraTypes.Position, VisionProfileSource.ComponentPackage, JobVM.CurrentComponentPackage.Id, newProfile);
            JobVM.SaveComponentPackageCommand.Execute(null);
        }

        public void ClonePartInTapeVisionProfile()
        {
            var positionCamera = MachineConfiguration.Cameras.SingleOrDefault(cam => cam.CameraType.Value == Manufacturing.Models.CameraTypes.Position);
            if (positionCamera == null)
            {
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, "Could not find part position camera.");
                return;
            }
            var newName = $"{MachineConfiguration.Cameras.First().CurrentVisionProfile.Name} ({JobVM.CurrentComponentPackage.Name})";
            var newProfile = JsonConvert.DeserializeObject<VisionProfile>(JsonConvert.SerializeObject(MachineConfiguration.Cameras.First().CurrentVisionProfile));
            newProfile.Name = newName;
            newProfile.Id = Guid.NewGuid().ToId();
            JobVM.CurrentComponentPackage.PartInTapeVisionProfile = newProfile;
            Machine.SetVisionProfile(Manufacturing.Models.CameraTypes.Position, VisionProfileSource.ComponentPackage, JobVM.CurrentComponentPackage.Id, newProfile);
            JobVM.SaveComponentPackageCommand.Execute(null);
        }

        public async void CheckPartPresent()
        {
            var result = await VacuumViewModel.CheckPartPresent(JobVM.CurrentComponent, 1000, JobVM.CurrentComponentPackage.PresenseVacuumOverride);
            if (result.Successful)
            {
                LastActionSuccess = true;
                LastStatus = "Success";
            }
            else
            {
                LastActionSuccess = false;
                LastStatus = result.ErrorMessage;
            }
        }

        public async void CheckNoPartPresent()
        {
             var result = await VacuumViewModel.CheckNoPartPresent(JobVM.CurrentComponent, 1000);
            if (result.Successful)
            {
                LastActionSuccess = true;
                LastStatus = "Success";
            }
            else
            {
                LastActionSuccess = false;
                LastStatus = result.ErrorMessage;
            }
        }

        public string _lastStatus;
        public string LastStatus
        {
            get => _lastStatus;
            set => Set(ref _lastStatus, value);
        }

        bool _lastActionSuccess;
        public bool LastActionSuccess
        {
            get => _lastActionSuccess;
            set => Set(ref _lastActionSuccess, value);
        }
        
        
        void GotoPartInFeeder(PartsGroup part)
        {
            JobVM.PartGroup = part;
            MoveToPartInFeeder();
        }


        public async void MoveToPartInFeeder()
        {
            var result = ResolveFeeder();
            if (!result.Successful)
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, result.ErrorMessage);
            else
            {
                await Machine.MoveToCameraAsync();
                await ActiveFeederViewModel.MoveToPartInFeederAsync(JobVM.CurrentComponent);
            }
        }

        public void PickPart()
        {
            var result = ResolveFeeder();
            if (!result.Successful)
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, result.ErrorMessage);
            else
                ActiveFeederViewModel.PickPartAsync(JobVM.CurrentComponent);            
        }

        public async void CenterInspectedPart()
        {
            var result = await PartInspectionVM.CenterPartAsync(JobVM.CurrentComponent, JobVM.Placement);
            if(result.Successful)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Info, $"Found center for {JobVM.Placement.Name} ");
                MessageText = "Centered Part!!";
            }
        }

        public void InspectPart()
        {
            PartInspectionVM.InspectAsync(JobVM.CurrentComponent);            
        }

        public void RecyclePart()
        {
            var result = ResolveFeeder();
            if (!result.Successful)
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, result.ErrorMessage);
            else
                ActiveFeederViewModel.RecyclePartAsync(JobVM.CurrentComponent);
        }



        public RelayCommand GoToPartOnBoardCommand { get; }

        public RelayCommand MoveToPartInFeederCommand { get; }
        public RelayCommand PickPartCommand { get; }
        public RelayCommand InspectPartCommand { get; }
        public RelayCommand RecyclePartCommand { get; }
        public RelayCommand InspectPartOnBoardCommand { get; }
        public RelayCommand CenterInspectedPartCommand { get; }
        public RelayCommand PickPartFromBoardCommand { get; }
        public RelayCommand PlacePartCommand { get; }
        public RelayCommand RotatePartCommand { get; }
        public RelayCommand RotateBackPartCommand { get; }

        public RelayCommand CheckPartPresentCommand { get; }
        public RelayCommand CheckNoPartPresentCommand { get; }

        public RelayCommand NextPartCommand { get; }

        public RelayCommand ClonePartInTapeVisionProfileCommand { get; }
        public RelayCommand ClonePartOnBoardVisionProfileCommand { get; }
        public RelayCommand ClonePartInspectionVisionProfileCommand { get;  }
    }
}
