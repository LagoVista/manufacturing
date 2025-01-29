using LagoVista.Client.Core;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using Newtonsoft.Json;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class DryRunViewModel : MachineViewModelBase, IDryRunViewModel
    {
        readonly IStripFeederViewModel _stripFeederViewModel;
        readonly IAutoFeederViewModel _autoFeederViewModel;

        IFeederViewModel _feederViewModel;

        private bool _feederIsVertical;

        public DryRunViewModel(IRestClient restClient, ICircuitBoardViewModel pcbVM, IPartInspectionViewModel partInspectionVM, IVacuumViewModel vacuumViewModel, IJobManagementViewModel jobVM, IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel, IMachineRepo machineRepo) : base(machineRepo)
        {
            _autoFeederViewModel = autoFeederViewModel ?? throw new ArgumentNullException(nameof(autoFeederViewModel)); 
            _stripFeederViewModel = stripFeederViewModel ?? throw new ArgumentNullException(nameof(stripFeederViewModel));

            PartInspectionVM = partInspectionVM ?? throw new ArgumentException(nameof(partInspectionVM));
            JobVM = jobVM ?? throw new ArgumentNullException(nameof(jobVM));
            PcbVM = pcbVM ?? throw new ArgumentNullException(nameof(pcbVM));
            VacuumViewModel = vacuumViewModel ?? throw new ArgumentNullException(nameof(vacuumViewModel));

            JobVM.PropertyChanged += JobVM_PropertyChanged;

            MoveToPartInFeederCommand = CreatedMachineConnectedCommand(MoveToPartInFeeder, () => JobVM.CurrentComponent != null);
            PickPartCommand = CreatedMachineConnectedCommand(PickPart, () => JobVM.CurrentComponent != null);
            InspectPartCommand = CreatedMachineConnectedCommand(InspectPart, () => JobVM.CurrentComponent != null);
            RecyclePartCommand = CreatedMachineConnectedCommand(RecyclePart, () => JobVM.CurrentComponent != null);

            PlacePartCommand = CreatedMachineConnectedCommand(() => PcbVM.PlacePartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement), () => JobVM.Placement != null);

            GoToPartOnBoardCommand = CreatedMachineConnectedCommand(() => PcbVM.GoToPartOnBoardAsync(JobVM.PickAndPlaceJobPart, JobVM.Placement), () => JobVM.Placement != null);
            InspectPartOnBoardCommand = CreatedMachineConnectedCommand(() => PcbVM.InspectPartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement), () => JobVM.Placement != null);
            
            PickPartFromBoardCommand = CreatedMachineConnectedCommand(async () =>
            {
                await PcbVM.PickPartFromBoardAsync(JobVM.CurrentComponent, JobVM.Placement);
                CheckPartPresent();
                InspectPart();
            }, () => JobVM.Placement != null);

            RotatePartCommand = CreatedMachineConnectedCommand(() => JobVM.RotateCurrentPartAsync(JobVM.PickAndPlaceJobPart, JobVM.Placement, _feederIsVertical, false), () => JobVM.Placement != null);
            RotateBackPartCommand = CreatedMachineConnectedCommand(() => JobVM.RotateCurrentPartAsync(JobVM.PickAndPlaceJobPart, JobVM.Placement, _feederIsVertical, true), () => JobVM.Placement != null);

            ClonePartInspectionVisionProfileCommand = CreatedCommand(ClonePartInspectionVisionProfile, () => JobVM.CurrentComponentPackage != null);
            ClonePartInTapeVisionProfileCommand = CreatedCommand(ClonePartInspectionVisionProfile, () => JobVM.CurrentComponentPackage != null);
            ClonePartOnBoardVisionProfileCommand = CreatedCommand(ClonePartInspectionVisionProfile, () => JobVM.CurrentComponentPackage != null);

            CheckPartPresentCommand = CreatedCommand(CheckPartPresent, () => JobVM.CurrentComponent != null);
            CheckNoPartPresentCommand = CreatedCommand(CheckNoPartPresent, () => JobVM.CurrentComponent != null);

            NextPartCommand = CreatedCommand(NextPart, () => _feederViewModel != null);
        }

        private InvokeResult ResolveFeeder()
        {
            if (JobVM.PickAndPlaceJobPart == null)
            {
                return InvokeResult.FromError("No part to place.");
            }

            var placement = JobVM.PickAndPlaceJobPart.Placements.FirstOrDefault();
            if (placement == null)
            {
                return InvokeResult.FromError("Could not identify first placement.");
            }

            var currentFeeder = _feederViewModel;

            _feederViewModel = null;

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
                _feederIsVertical = _stripFeederViewModel.Current.Orientation.Value == Manufacturing.Models.FeederOrientations.Vertical;

            }
            else if (!EntityHeader.IsNullOrEmpty(JobVM.PickAndPlaceJobPart.AutoFeeder))
            {
                var fdr = _autoFeederViewModel.Feeders.SingleOrDefault(sf => sf.Id == JobVM.PickAndPlaceJobPart.AutoFeeder.Id);
                _autoFeederViewModel.Current = fdr;
                _feederViewModel = _autoFeederViewModel;
                if (_autoFeederViewModel.Current == null)
                {
                    return InvokeResult.FromError($"Could not find auto feeder {JobVM.PickAndPlaceJobPart.AutoFeeder}");
                }

                _feederIsVertical = true;
            }
            else
            {
                
                return InvokeResult.FromError("Selected component does not have an assocaited feeder.");
            }

            if(currentFeeder != _feederViewModel)
            {
                JobVM.Placement = placement;
            }

            RaiseCanExecuteChanged();

            return InvokeResult.Success;
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
            Machine.SetVisionProfile(Manufacturing.Models.CameraTypes.PartInspection, newProfile);
            JobVM.SaveComponentPackageCommand.Execute(null);
        }

        public void NextPart()
        {
            _feederViewModel.NextPart();
            var idx = JobVM.PickAndPlaceJobPart.Placements.IndexOf(JobVM.Placement);
            if(idx == -1)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Could not find current placement.");
            }
            idx++;
            if(JobVM.PickAndPlaceJobPart.Placements.Count > idx)
            {
                JobVM.Placement = JobVM.PickAndPlaceJobPart.Placements[idx];
                MoveToPartInFeeder();
            }
            else
            {
                JobVM.Placement = null;
            }


        }

        public void ClonePartOnBoardisionProfile()
        {

        }

        public void ClonePartInTapeVisionProfile()
        {

        }

        public async void CheckPartPresent()
        {
            var result = await VacuumViewModel.CheckPartPresent(JobVM.CurrentComponent, 1000);
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
        
        
        void GotoPartInFeeder(PickAndPlaceJobPart part)
        {
            JobVM.PickAndPlaceJobPart = part;
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
                await _feederViewModel.MoveToPartInFeederAsync(JobVM.CurrentComponent);
            }
        }

        public void PickPart()
        {
            var result = ResolveFeeder();
            if (!result.Successful)
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, result.ErrorMessage);
            else
                _feederViewModel.PickPartAsync(JobVM.CurrentComponent);            
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
                _feederViewModel.RecyclePartAsync(JobVM.CurrentComponent);
        }

        private void JobVM_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(JobVM.CurrentComponent):
                case nameof(JobVM.PickAndPlaceJobPart):
                case nameof(JobVM.Placement): RaiseCanExecuteChanged(); break;
            }
        }

        public IVacuumViewModel VacuumViewModel { get; }   
        public IJobManagementViewModel JobVM { get; }
        public ICircuitBoardViewModel PcbVM { get; }
        public IPartInspectionViewModel PartInspectionVM { get; }

        public RelayCommand GoToPartOnBoardCommand { get; }

        public RelayCommand MoveToPartInFeederCommand { get; }
        public RelayCommand PickPartCommand { get; }
        public RelayCommand InspectPartCommand { get; }
        public RelayCommand RecyclePartCommand { get; }
        public RelayCommand InspectPartOnBoardCommand { get; }
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
