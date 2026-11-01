// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6f6aef4811358a48eb9c866213f8ba9b399b999345f407630a3dea538992033a
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

            SetToolOffsetCommand = CreatedMachineConnectedSettingsCommand(() => Current.Offset = (Machine.MachinePosition.ToPoint2D() - MachineConfiguration.KnownCalibrationPoint).Round(2), () => Current != null);

            CaptureKnownLocationCommand = CreatedMachineConnectedSettingsCommand(() => MachineConfiguration.KnownCalibrationPoint = Machine.MachinePosition.ToPoint2D());
            MoveToKnownLocationCommand = CreatedMachineConnectedCommand(() => {
                Machine.GotoPoint(MachineConfiguration.KnownCalibrationPoint);
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_KnownLocation);
                }, () => !MachineConfiguration.KnownCalibrationPoint.IsOrigin());

            SetDefaultOriginCommand = CreatedMachineConnectedSettingsCommand(() => Current.DefaultOriginPosition = Machine.MachinePosition.Z, () => Current != null);
            SetIdleVacuumCommand = CreatedMachineConnectedSettingsCommand(() => SetVacuum(VacuumState.Idle), () => Current != null);
            SetNoPartPickedVacuumCommand = CreatedMachineConnectedSettingsCommand(() => SetVacuum(VacuumState.NoPartPicked), () => Current != null);
            SetPartPickedVacuumCommand = CreatedMachineConnectedSettingsCommand(() => SetVacuum(VacuumState.PartPicked), () => Current != null);

            ToggleCaptureToolHeadCalibrationCommand = CreatedMachineConnectedSettingsCommand(() => ToggleCaptureToolHeadOffset(), () => Current != null);
            SetToolHeadCalibrationCommand = CreatedMachineConnectedSettingsCommand(() => SetToolHeadCalibrationOffset(), () => Current != null && CalibrationOffset != null);

            MoveToolHeadOverKnownLocationCommand = CreatedMachineConnectedCommand(MoveToolHeadOverKnownLocation, () => Current != null && Current.Offset != null);

        }

        private async void SetVacuum(VacuumState state)
        {
            if (Current == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "No current tool head selected, will not set vacuum.");
                return;
            }

            var values = new List<double>();

            for (var idx = 0; idx < 10; ++idx)
            {
                var result = Current.HeadIndex == 1 ? await Machine.ReadLeftVacuumAsync() : await Machine.ReadRightVacuumAsync();
                if (result.Successful)
                    values.Add(result.Result);

                await Task.Delay(50);
            }

            switch (state)
            {
                case VacuumState.PartPicked:
                    Current.PartPickedVacuum = values.Average(val => val);
                    break;
                case VacuumState.NoPartPicked:
                    Current.NoPartPickedVacuum = values.Average(val => val);
                    break;
                case VacuumState.Idle:
                    Current.IdleVacuum = values.Average(val => val);
                    break;
            }
        }

        public void MoveToolHeadOverKnownLocation()
        {
            if (Current?.Offset != null)
            {
                Machine.GotoPoint(Current.Offset, true, true);
            }
        }


        public void ToggleCaptureToolHeadOffset()
        {
            if (CalibrationOffset == null)
            {
                CalibrationOffset = new Point2D<double>(0, 0);
                InitialLocation = Machine.MachinePosition.ToPoint2D();
            }
            else
            {
                CalibrationOffset = null;
            }

            RaiseCanExecuteChanged();
        }

       public void SetToolHeadCalibrationOffset()
        {
            Current.Offset -= CalibrationOffset;
            Current.Offset = Current.Offset.Round(2);
            CalibrationOffset = null;
            RaiseCanExecuteChanged();
        }

        public IMachineUtilitiesViewModel Vacuum => _utilties;

        protected override void MachineChanged(IMachine machine)
        {
            machine.PropertyChanged += Machine_PropertyChanged;
            RaisePropertyChanged(nameof(ToolHeads));
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IMachine.MachinePosition) && CalibrationOffset != null)
            {
                Delta = KnownLocation - Machine.MachinePosition.ToPoint2D();
                CalibrationOffset = (InitialLocation - Machine.MachinePosition.ToPoint2D()).Round(2);
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

        private Point2D<double> _initialLocation = new Point2D<double>();
        public Point2D<double> InitialLocation
        {
            get => _initialLocation;
            private set => Set(ref _initialLocation, value);
        }

        private Point2D<double> _delta = new Point2D<double>();
        public Point2D<double> Delta
        {
            get => _delta;
            private set => Set(ref _delta, value);
        }

        Point2D<double> _calibrationOffset;
        public Point2D<double> CalibrationOffset
        {
            get => _calibrationOffset;
            set => Set(ref _calibrationOffset, value);
        }

        public RelayCommand SetToolOffsetCommand { get; }

        public RelayCommand CaptureKnownLocationCommand { get; }
        public RelayCommand MoveToKnownLocationCommand { get; }

        public RelayCommand SetDefaultOriginCommand { get; }
        public RelayCommand SetIdleVacuumCommand { get; }
        public RelayCommand SetNoPartPickedVacuumCommand { get; }
        public RelayCommand SetPartPickedVacuumCommand { get; }

        public RelayCommand ToggleCaptureToolHeadCalibrationCommand { get; }

        public RelayCommand MoveToolHeadOverKnownLocationCommand { get; }

        public RelayCommand SetToolHeadCalibrationCommand { get; }
    }
}
