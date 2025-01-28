using Emgu.CV.DepthAI;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class ToolHeadViewModel : MachineViewModelBase, IToolHeadViewModel
    {
        private readonly IMachineUtilitiesViewModel _utilties;

        enum VacuumState
        {
            Idle,
            NoPartPicked,
            PartPicked,
        }

        public ToolHeadViewModel(IMachineRepo machineRepo, IMachineUtilitiesViewModel utilties) : base(machineRepo)
        {
            _utilties = utilties ?? throw new ArgumentNullException(nameof(utilties));

            SetToolOffsetCommand = CreatedMachineConnectedSettingsCommand(() => Current.Offset = (Machine.MachinePosition.ToPoint2D() - MachineConfiguration.DefaultToolReferencePoint).Round(2), () => Current != null);
            
            CaptureKnownLocationCommand = CreatedMachineConnectedSettingsCommand(() => MachineConfiguration.KnownCalibrationPoint = Machine.MachinePosition.ToPoint2D());
            MoveToKnownLocationCommand = CreatedMachineConnectedCommand(() => Machine.GotoPoint(MachineConfiguration.KnownCalibrationPoint), () => !MachineConfiguration.KnownCalibrationPoint.IsOrigin());
           
            SetDefaultOriginCommand = CreatedMachineConnectedSettingsCommand(() => Current.DefaultOriginPosition = Machine.MachinePosition.Z, () => Current != null);
            SetIdleVacuumCommand = CreatedMachineConnectedSettingsCommand(() => SetVacuum(VacuumState.Idle), () => Current != null);
            SetNoPartPickedVacuumCommand = CreatedMachineConnectedSettingsCommand(() => SetVacuum(VacuumState.NoPartPicked), () => Current != null);
            SetPartPickedVacuumCommand = CreatedMachineConnectedSettingsCommand(() => SetVacuum(VacuumState.PartPicked), () => Current != null);
        }

        private async void SetVacuum(VacuumState state)
        {
            if(Current == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current tool head selected, will not set vacuum.");
                return;
            }

            var values = new List<double>();

            for(var idx = 0; idx < 10; ++idx)
            {
                var result = Current.HeadIndex == 1 ? await Machine.ReadLeftVacuumAsync() : await Machine.ReadRightVacuumAsync();
                if(result.Successful) 
                    values.Add(result.Result);
                
                await Task.Delay(50);
            }

            switch(state)
            {
                case VacuumState.PartPicked:
                    Current.PartPickedVacuum = values.Average(val => val);
                    break;
                case VacuumState.NoPartPicked:
                    Current.NoPartPickedVacuum = values.Average(val => val);
                    break;
                case VacuumState.Idle:
                    Current.IdleVacuum = values.Average(val=>val);
                    break;
            }
        }

        public IMachineUtilitiesViewModel Vacuum => _utilties;

        protected override void MachineChanged(IMachine machine)
        {
            machine.PropertyChanged += Machine_PropertyChanged;
            RaisePropertyChanged(nameof(ToolHeads));
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IMachine.MachinePosition))
            {
                Delta = KnownLocation - Machine.MachinePosition.ToPoint2D();
            }
        }

        MachineToolHead _current;
        public MachineToolHead Current
        {
            get => _current;
            set
            {
                Set(ref _current, value);
                RaiseCanExecuteChanged();
            } 
        }

        public ObservableCollection<MachineToolHead> ToolHeads => MachineConfiguration?.ToolHeads;

        private Point2D<double> _knownLocation = new Point2D<double>();
        public Point2D<double> KnownLocation
        {
            get => _knownLocation;
            private set => Set(ref _knownLocation, value);
        }

        private Point2D<double> _delta = new Point2D<double>();
        public Point2D<double> Delta
        {
            get => _delta;
            private set => Set(ref _delta, value);
        }

        public RelayCommand SetToolOffsetCommand { get; }

        public RelayCommand CaptureKnownLocationCommand { get; }
        public RelayCommand MoveToKnownLocationCommand { get; }

        public RelayCommand SetDefaultOriginCommand { get;  }
        public RelayCommand SetIdleVacuumCommand { get; }
        public RelayCommand SetNoPartPickedVacuumCommand { get; }
        public RelayCommand SetPartPickedVacuumCommand { get; }
    }
}
