using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.Validation;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class DryRunViewModel : MachineViewModelBase, IDryRunViewModel
    {
        IStripFeederViewModel _stripFeederViewModel;
        IAutoFeederViewModel _autoFeederViewModel;
        IFeederViewModel _feederViewModel;

        private bool _feederIsVertical;

        public DryRunViewModel(IRestClient restClient, ICircuitBoardViewModel pcbVM, IPartInspectionViewModel partInspectionVM, IJobManagementViewModel jobVM, IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel, IMachineRepo machineRepo) : base(machineRepo)
        {
            _autoFeederViewModel = autoFeederViewModel;
            _stripFeederViewModel = stripFeederViewModel;

            PartInspectionVM = partInspectionVM ?? throw new ArgumentException(nameof(partInspectionVM));
            JobVM = jobVM ?? throw new ArgumentNullException(nameof(jobVM));
            PcbVM = pcbVM ?? throw new ArgumentNullException(nameof(pcbVM));

            JobVM.PropertyChanged += JobVM_PropertyChanged;

            MoveToPartInFeederCommand = CreatedMachineConnectedCommand(MoveToPartInFeeder, () => JobVM.CurrentComponent != null);
            PickPartCommand = CreatedMachineConnectedCommand(PickPart, () => JobVM.CurrentComponent != null);
            InspectPartCommand = CreatedMachineConnectedCommand(InspectPart, () => JobVM.CurrentComponent != null);
            RecyclePartCommand = CreatedMachineConnectedCommand(RecyclePart, () => JobVM.CurrentComponent != null);

            PlacePartCommand = CreatedMachineConnectedCommand(() => PcbVM.PlacePartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement), () => JobVM.Placement != null);

            GoToPartOnBoardCommand = CreatedMachineConnectedCommand(() => PcbVM.GoToPartOnBoardAsync(JobVM.PickAndPlaceJobPart, JobVM.Placement), () => JobVM.Placement != null);
            InspectPartOnBoardCommand = CreatedMachineConnectedCommand(() => PcbVM.InspectPartOnboardAsync(JobVM.CurrentComponent, JobVM.Placement), () => JobVM.Placement != null);
            
            PickPartFromBoardCommand = CreatedMachineConnectedCommand(() => PcbVM.PickPartFromBoardAsync(JobVM.CurrentComponent, JobVM.Placement), () => JobVM.Placement != null);

            RotatePartCommand = CreatedMachineConnectedCommand(() => JobVM.RotateCurrentPartAsync(JobVM.PickAndPlaceJobPart, JobVM.Placement, _feederIsVertical, false), () => JobVM.Placement != null);
            RotateBackPartCommand = CreatedMachineConnectedCommand(() => JobVM.RotateCurrentPartAsync(JobVM.PickAndPlaceJobPart, JobVM.Placement, _feederIsVertical, true), () => JobVM.Placement != null);
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
                _autoFeederViewModel = null;
                _feederIsVertical = _stripFeederViewModel.Current.Orientation.Value == Manufacturing.Models.FeederOrientations.Vertical;

            }
            else if (!EntityHeader.IsNullOrEmpty(JobVM.PickAndPlaceJobPart.AutoFeeder))
            {
                _autoFeederViewModel.Current = _autoFeederViewModel.Feeders.SingleOrDefault(sf => sf.Id == JobVM.PickAndPlaceJobPart.AutoFeeder.Id);
                if (_autoFeederViewModel.Current == null)
                {
                    return InvokeResult.FromError($"Could not find auto feeder {JobVM.PickAndPlaceJobPart.AutoFeeder}");
                }

                _stripFeederViewModel = null;
                _feederIsVertical = false;

                _feederViewModel = _autoFeederViewModel;
            }
            else
            {
                return InvokeResult.FromError("Selected component does not have an assocaited feeder.");
            }

            return InvokeResult.Success;
        }

        public void MoveToPartInFeeder()
        {
            var result = ResolveFeeder();
            if (!result.Successful)
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, result.ErrorMessage);
            else
                _feederViewModel.MoveToPartInFeederAsync(JobVM.CurrentComponent);
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
    }
}
