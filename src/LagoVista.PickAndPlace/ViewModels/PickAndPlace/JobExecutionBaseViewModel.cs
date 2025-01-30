using LagoVista.Client.Core;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public abstract class JobExecutionBaseViewModel : MachineViewModelBase
    {
        readonly IStripFeederViewModel _stripFeederViewModel;
        readonly IAutoFeederViewModel _autoFeederViewModel;


        private bool _feederIsVertical;

        public JobExecutionBaseViewModel(IRestClient restClient, ICircuitBoardViewModel pcbVM, IPartInspectionViewModel partInspectionVM, IVacuumViewModel vacuumViewModel,
                               IJobManagementViewModel jobVM, IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel, IMachineRepo machineRepo) : base(machineRepo)
        {
            _autoFeederViewModel = autoFeederViewModel ?? throw new ArgumentNullException(nameof(autoFeederViewModel));
            _stripFeederViewModel = stripFeederViewModel ?? throw new ArgumentNullException(nameof(stripFeederViewModel));

            PartInspectionVM = partInspectionVM ?? throw new ArgumentException(nameof(partInspectionVM));
            JobVM = jobVM ?? throw new ArgumentNullException(nameof(jobVM));
            PcbVM = pcbVM ?? throw new ArgumentNullException(nameof(pcbVM));
            VacuumViewModel = vacuumViewModel ?? throw new ArgumentNullException(nameof(vacuumViewModel));

            JobVM.PropertyChanged += JobVM_PropertyChanged;
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


        protected InvokeResult ResolveFeeder()
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

            var currentFeeder = ActiveFeederViewModel;
            ActiveFeederViewModel = null;

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

                ActiveFeederViewModel = _stripFeederViewModel;
                _feederIsVertical = _stripFeederViewModel.Current.Orientation.Value == Manufacturing.Models.FeederOrientations.Vertical;

            }
            else if (!EntityHeader.IsNullOrEmpty(JobVM.PickAndPlaceJobPart.AutoFeeder))
            {
                var fdr = _autoFeederViewModel.Feeders.SingleOrDefault(sf => sf.Id == JobVM.PickAndPlaceJobPart.AutoFeeder.Id);
                _autoFeederViewModel.Current = fdr;
                ActiveFeederViewModel = _autoFeederViewModel;
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

            if (currentFeeder != ActiveFeederViewModel)
            {
                JobVM.Placement = placement;
            }

            if(ActiveFeederViewModel != null)
            {
                ActiveFeederViewModel.CurrentComponent = JobVM.CurrentComponent;
            }

            RaiseCanExecuteChanged();

            return InvokeResult.Success;
        }


        private IFeederViewModel _activeFeeder;
        public IFeederViewModel ActiveFeederViewModel
        {
            get => _activeFeeder;
            set => Set(ref _activeFeeder, value);
        }


        public IVacuumViewModel VacuumViewModel { get; }
        public IJobManagementViewModel JobVM { get; }
        public ICircuitBoardViewModel PcbVM { get; }
        public IPartInspectionViewModel PartInspectionVM { get; }

    }
}
