using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

            MoveToPartInFeederCommand = CreatedMachineConnectedCommand(MoveToPartInFeeder, () => JobVM.CurrentComponent != null);
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
                case nameof(JobVM.PartGroup):
                case nameof(JobVM.Placement): RaiseCanExecuteChanged(); break;
            }
        }


        protected InvokeResult ResolveFeeder()
        {
            if (JobVM.PartGroup == null)
            {
                return InvokeResult.FromError("No part to place.");
            }

            var previousFeeder = ActiveFeederViewModel;
            ActiveFeederViewModel = null;

            if (!EntityHeader.IsNullOrEmpty(JobVM.PartGroup.StripFeeder))
            {
                _stripFeederViewModel.Current = _stripFeederViewModel.Feeders.SingleOrDefault(sf => sf.Id == JobVM.PartGroup.StripFeeder.Id);
                if (_stripFeederViewModel.Current == null)
                {
                    return InvokeResult.FromError($"Could not find strip feeder {JobVM.PartGroup.StripFeeder.Text}.");
                }

                if (EntityHeader.IsNullOrEmpty(JobVM.PartGroup.StripFeederRow))
                {
                    return InvokeResult.FromError($"Strip feeder {JobVM.PartGroup.StripFeeder} does ont have row.");
                }

                _stripFeederViewModel.CurrentRow = _stripFeederViewModel.Current.Rows.SingleOrDefault(r => r.Id == JobVM.PartGroup.StripFeederRow.Id);
                if (_stripFeederViewModel.CurrentRow == null)
                {
                    return InvokeResult.FromError($"On Strip feeder {JobVM.PartGroup.StripFeeder}, could not find row {JobVM.PartGroup.StripFeederRow.Text}.");
                }

                ActiveFeederViewModel = _stripFeederViewModel;
                _feederIsVertical = _stripFeederViewModel.Current.Orientation.Value == Manufacturing.Models.FeederOrientations.Vertical;

            }
            else if (!EntityHeader.IsNullOrEmpty(JobVM.PartGroup.AutoFeeder))
            {
                var fdr = _autoFeederViewModel.Feeders.SingleOrDefault(sf => sf.Id == JobVM.PartGroup.AutoFeeder.Id);
                _autoFeederViewModel.Current = fdr;
                ActiveFeederViewModel = _autoFeederViewModel;
                if (_autoFeederViewModel.Current == null)
                {
                    return InvokeResult.FromError($"Could not find auto feeder {JobVM.PartGroup.AutoFeeder}");
                }

                _feederIsVertical = true;
            }
            else
            {
                return InvokeResult.FromError("Selected component does not have an assocaited feeder.");
            }

            if (previousFeeder != ActiveFeederViewModel)
            {
                var placement = JobVM.PartGroup.Placements.FirstOrDefault();
                if (placement == null)
                {
                    return InvokeResult.FromError("Could not identify first placement.");
                }

                JobVM.Placement = placement;
            }

            if(ActiveFeederViewModel != null)
            {
                ActiveFeederViewModel.CurrentComponent = JobVM.CurrentComponent;
                ActiveFeederViewModel.CurrentComponentPackage = JobVM.CurrentComponentPackage;
            }

            RaiseCanExecuteChanged();

            return InvokeResult.Success;
        }


        public async void MoveToPartInFeeder()
        {
            var sw = Stopwatch.StartNew();
            Machine.DebugWriteLine("-----------------------------");
            Machine.DebugWriteLine("[MoveToPartInFeeder] - Start");

            Machine.SetMode(OperatingMode.PlacingParts);

            var result = ResolveFeeder();
            Machine.DebugWriteLine($"[MoveToPartInFeeder] - Resolved Feeder {sw.Elapsed.TotalMilliseconds}ms");

            if (!result.Successful)
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, result.ErrorMessage);
            else
            {
                await Machine.MoveToCameraAsync();
                await ActiveFeederViewModel.MoveToPartInFeederAsync(JobVM.CurrentComponent);
            }

            await Machine.SpinUntilIdleAsync();
            Machine.SetMode(OperatingMode.Manual);

            Machine.DebugWriteLine($"[MoveToPartInFeeder] - End {sw.Elapsed.TotalMilliseconds}ms");
            Machine.DebugWriteLine("-----------------------------");
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

        public RelayCommand MoveToPartInFeederCommand { get; }
    }
}
