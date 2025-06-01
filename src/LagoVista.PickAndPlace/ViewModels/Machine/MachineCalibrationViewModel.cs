using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineCalibrationViewModel : MachineViewModelBase, IMachineCalibrationViewModel
    {
        public MachineCalibrationViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {

            SetStagingPlateReferenceHole1LocationCommand = CreatedMachineConnectedSettingsCommand(() => SelectedStagingPlate.ReferenceHoleLocation1 = Machine.MachinePosition.ToPoint2D(), () => SelectedStagingPlate != null);
            SetStagingPlateReferenceHole2LocationCommand = CreatedMachineConnectedSettingsCommand(() => SelectedStagingPlate.ReferenceHoleLocation2 = Machine.MachinePosition.ToPoint2D(), () => SelectedStagingPlate != null);
            SetFirstAutoFeederOriginCommand = CreatedMachineConnectedSettingsCommand(() => SelectedFeederRail.FirstFeederOrigin = Machine.MachinePosition.ToPoint2D(), () => SelectedFeederRail != null);

            SetMachineFiducialCommand = CreateCommand(() => MachineConfiguration.MachineFiducial = Machine.MachinePosition.ToPoint2D());
            SetDefaultPCBOrigin = CreatedMachineConnectedSettingsCommand(() => MachineConfiguration.DefaultWorkOrigin = Machine.MachinePosition.ToPoint2D());

            SetDefaultSafeMoveHeightCommand = CreatedMachineConnectedSettingsCommand(() => MachineConfiguration.DefaultSafeMoveHeight = Machine.MachinePosition.Z);
            MoveToDefaultSafeMoveHeightCommand = CreatedMachineConnectedCommand(() => Machine.SendSafeMoveHeight());

            CaptureKnownLocationCommand = CreatedMachineConnectedSettingsCommand(() => MachineConfiguration.KnownCalibrationPoint = Machine.MachinePosition.ToPoint2D());

            MoveToKnownLocationCommand = CreatedMachineConnectedCommand(() => {
                Machine.GotoPoint(MachineConfiguration.KnownCalibrationPoint);
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_KnownLocation    );
            }, () => !MachineConfiguration.KnownCalibrationPoint.IsOrigin());

            MoveToCameraLocationCommand = CreatedMachineConnectedCommand(MoveToSelectedCamera, () => SelectedMachineCamera != null && SelectedMachineCamera.AbsolutePosition != null);
            SetCameraLocationCommand = CreatedMachineConnectedSettingsCommand(() => {
                SelectedMachineCamera.AbsolutePosition = Machine.MachinePosition.ToPoint2D();
                SelectedMachineCamera.FocusHeight = Machine.MachinePosition.Z;
            }, () => SelectedMachineCamera != null);


            MoveToStagingPlateReferenceHole1LocationCommand = CreatedMachineConnectedCommand(() =>
            {
                Machine.GotoPoint(SelectedStagingPlate.ReferenceHoleLocation1);
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_StagingPlateHole);
                
            }, () => SelectedStagingPlate != null && (UseCalibratedLocation || SelectedStagingPlate.ReferenceHoleLocation1 != null));
        
            MoveToStagingPlateReferenceHole2LocationCommand = CreatedMachineConnectedCommand(() =>
            {
                Machine.GotoPoint(SelectedStagingPlate.ReferenceHoleLocation2);
                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_StagingPlateHole);

            }, () => SelectedStagingPlate != null && (UseCalibratedLocation || SelectedStagingPlate.ReferenceHoleLocation2 != null));

            MoveToFirstAutoFeederOriginCommand = CreatedMachineConnectedCommand(() => Machine.GotoPoint(SelectedFeederRail.FirstFeederOrigin), () => SelectedFeederRail != null && SelectedFeederRail.FirstFeederOrigin != null);

            MoveToMachineFiducialCommand = CreatedMachineConnectedCommand(() => Machine.GotoPoint(Machine.Settings.MachineFiducial), () => !Machine.Settings.MachineFiducial.IsOrigin());

            MoveToDefaultPCBOrigin = CreatedMachineConnectedCommand(() => Machine.GotoPoint(MachineConfiguration.DefaultWorkOrigin), () => !MachineConfiguration.DefaultWorkOrigin.IsOrigin());
        }



        protected void MoveToSelectedCamera()
        {
            if (SelectedMachineCamera == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Please select a camera first.");
            }
            else
            {
                Machine.GotoPoint(SelectedMachineCamera.AbsolutePosition);
                Machine.SendCommand($"G0 Z{SelectedMachineCamera.FocusHeight}");
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
            set
            {
                Set(ref _selectedMachineCamera, value);
                RaiseCanExecuteChanged();
            }
        }

        ToolNozzleTip _selectedNozzleTip;
        public ToolNozzleTip SelectedNozzleTip
        {
            get => _selectedNozzleTip;
            set => Set(ref _selectedNozzleTip, value);  
        }

        public RelayCommand SetStagingPlateReferenceHole1LocationCommand { get; }
        public RelayCommand SetStagingPlateReferenceHole2LocationCommand { get; }
        public RelayCommand SetFirstAutoFeederOriginCommand { get; }
        
        
        public RelayCommand SetCameraLocationCommand { get; }
        public RelayCommand SetMachineFiducialCommand { get; }
        public RelayCommand MoveToMachineFiducialCommand { get; }

        public RelayCommand MoveToStagingPlateReferenceHole1LocationCommand { get; }
        public RelayCommand MoveToStagingPlateReferenceHole2LocationCommand { get; }

        public RelayCommand MoveToFirstAutoFeederOriginCommand { get; }

        public RelayCommand MoveToCameraLocationCommand { get; }
        
        public RelayCommand SetDefaultSafeMoveHeightCommand { get; }
        public RelayCommand MoveToDefaultSafeMoveHeightCommand { get; }
        public RelayCommand SetDefaultPCBOrigin {get;}
        public RelayCommand MoveToDefaultPCBOrigin { get; }

        public RelayCommand CaptureKnownLocationCommand { get; }
        public RelayCommand MoveToKnownLocationCommand { get; }
        

        private bool _useCalibratedLocation;
        public bool UseCalibratedLocation
        {
            get => _useCalibratedLocation;
            set => Set(ref _useCalibratedLocation, value);
        }
    }
}
