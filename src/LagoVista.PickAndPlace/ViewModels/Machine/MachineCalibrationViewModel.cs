using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineCalibrationViewModel : MachineViewModelBase, IMachineCalibrationViewModel
    {
        

        public MachineCalibrationViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
            SetDefaultToolReferencePointCommand = CreatedMachineConnectedCommand(() => MachineConfiguration.DefaultToolReferencePoint = Machine.MachinePosition.ToPoint2D());
            SetStagingPlateReferenceHole1LocationCommand = CreatedMachineConnectedCommand(() => SelectedStagingPlate.ReferenceHoleLocation1 = Machine.MachinePosition.ToPoint2D(), () => SelectedStagingPlate != null);
            SetStagingPlateReferenceHole2LocationCommand = CreatedMachineConnectedCommand(() => SelectedStagingPlate.ReferenceHoleLocation2 = Machine.MachinePosition.ToPoint2D(), () => SelectedStagingPlate != null);
            SetFirstFeederOriginCommand = CreatedMachineConnectedCommand(() => SelectedFeederRail.FirstFeederOrigin = Machine.MachinePosition.ToPoint2D(), () => SelectedFeederRail != null);
            SetToolOffsetCommand = CreatedMachineConnectedCommand(() => SelectedToolHead.Offset = Machine.MachinePosition.ToPoint2D() - MachineConfiguration.DefaultToolReferencePoint, () => SelectedToolHead != null);
            SetCameraLocationCommand = CreatedMachineConnectedCommand(() => {
                SelectedMachineCamera.AbsolutePosition = Machine.MachinePosition.ToPoint2D();
                SelectedMachineCamera.FocusHeight = Machine.MachinePosition.Z;
                }, () => SelectedMachineCamera != null);

            MoveToDefaultToolReferencePointCommand = CreatedMachineConnectedCommand(() => Machine.GotoPoint(MachineConfiguration.DefaultToolReferencePoint), () => MachineConfiguration.DefaultToolReferencePoint != null);
            MoveToStagingPlateReferenceHole1LocationCommand = CreatedMachineConnectedCommand(() => Machine.GotoPoint(SelectedStagingPlate.ReferenceHoleLocation1), () => SelectedStagingPlate != null && SelectedStagingPlate.ReferenceHoleLocation1 != null);
            MoveToStagingPlateReferenceHole2LocationCommand = CreatedMachineConnectedCommand(() => Machine.GotoPoint(SelectedStagingPlate.ReferenceHoleLocation2), () => SelectedStagingPlate != null && SelectedStagingPlate.ReferenceHoleLocation2 != null);
            MoveToFirstFeederOriginCommand = CreatedMachineConnectedCommand(() => Machine.GotoPoint(SelectedFeederRail.FirstFeederOrigin), () => SelectedFeederRail != null && SelectedFeederRail.FirstFeederOrigin != null);
            MoveToCameraLocationCommand = CreatedMachineConnectedCommand(() => Machine.GotoPoint(SelectedMachineCamera.AbsolutePosition.X, SelectedMachineCamera.AbsolutePosition.Y, SelectedMachineCamera.FocusHeight), () => SelectedMachineCamera != null && SelectedMachineCamera.AbsolutePosition != null);
            CaptureKnownLocationCommand = CreatedMachineConnectedCommand(() => KnownLocation = Machine.MachinePosition.ToPoint2D());
        }

        protected override void MachineChanged(IMachine machine)
        {
            machine.PropertyChanged += Machine_PropertyChanged;            
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IMachine.MachinePosition))
            {
                Delta = KnownLocation - Machine.MachinePosition.ToPoint2D();
            }
        }

            MachineToolHead _selectedToolHead;
        public MachineToolHead SelectedToolHead
        {
            get => _selectedToolHead;
            set
            {
                Set(ref _selectedToolHead, value);
                RaiseCanExecuteChanged();
            }
        }

        MachineStagingPlate _selectedStagingPlate;
        public MachineStagingPlate SelectedStagingPlate
        { 
            get => _selectedStagingPlate;
            set
            {
                Set(ref _selectedStagingPlate, value);
                RaiseCanExecuteChanged();
            }
        }

        MachineFeederRail _selectedFeederRail;
        public MachineFeederRail SelectedFeederRail 
        {
            get => _selectedFeederRail;
            set
            {
                Set(ref _selectedFeederRail, value);
                RaiseCanExecuteChanged();
            }
        }        

        MachineCamera _selectedMachineCamera;
        public MachineCamera SelectedMachineCamera
        {
            get => _selectedMachineCamera;
            set => Set(ref _selectedMachineCamera, value);
        }

        ToolNozzleTip _selectedNozzleTip;
        public ToolNozzleTip SelectedNozzleTip
        {
            get => _selectedNozzleTip;
            set => Set(ref _selectedNozzleTip, value);  
        }

        public RelayCommand SetStagingPlateReferenceHole1LocationCommand { get; }
        public RelayCommand SetStagingPlateReferenceHole2LocationCommand { get; }
        public RelayCommand SetFirstFeederOriginCommand { get; }
        public RelayCommand SetDefaultToolReferencePointCommand { get; }
        public RelayCommand SetToolOffsetCommand { get; }
        public RelayCommand SetCameraLocationCommand { get; }
        

        public RelayCommand MoveToStagingPlateReferenceHole1LocationCommand { get; }
        public RelayCommand MoveToStagingPlateReferenceHole2LocationCommand { get; }
        public RelayCommand MoveToFirstFeederOriginCommand { get; }
        public RelayCommand MoveToDefaultToolReferencePointCommand { get; }
        public RelayCommand MoveToCameraLocationCommand { get; }
        public RelayCommand CaptureKnownLocationCommand { get; }

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
    }
}
