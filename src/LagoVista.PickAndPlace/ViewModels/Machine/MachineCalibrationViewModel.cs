// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 02e031acc2cc85a45d8c2f14cc506ec83bafa1277b5f8260dd888703076f53a9
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineCalibrationViewModel : MachineViewModelBase, IMachineCalibrationViewModel
    {
        public MachineCalibrationViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {

            SetStagingPlateReferenceHole1LocationCommand = CreatedMachineConnectedSettingsCommand(() =>
            {
                SelectedStagingPlate.ReferenceHoleLocation1 = Machine.MachinePosition.ToPoint2D();
                SelectedStagingPlate.IsCalibrated = false;
            }, () => SelectedStagingPlate != null);
            SetStagingPlateReferenceHole2LocationCommand = CreatedMachineConnectedSettingsCommand(() =>
            {
                SelectedStagingPlate.ReferenceHoleLocation2 = Machine.MachinePosition.ToPoint2D();
                SelectedStagingPlate.IsCalibrated = false;
            }, () => SelectedStagingPlate != null);
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

            AutoTuneCameraExposureCommand = CreatedMachineConnectedSettingsCommand(
                async () => await AutoTuneCameraExposureAsync(),
                () => SelectedMachineCamera != null &&
                      SelectedMachineCamera.CameraType?.Value == CameraTypes.Position &&
                      Machine.PositionImageCaptureService != null &&
                      !MachineConfiguration.MachineFiducial.IsOrigin() &&
                      !ExposureCalibrationRunning);

            SetCameraPixelsPerMMCommand = new RelayCommand(
                async () => await SetCameraPixelsPerMMAsync(),
                () => !_savingCameraPixelsPerMM && SelectedMachineCamera != null &&
                      CameraPixelScaleCalibration.TryCalculateText(CameraMeasuredMM, out _));


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



        private bool _savingCameraPixelsPerMM;
        // Text-backed input lets the operator type intermediate values such as "2."
        // without WPF rounding away the decimal point on each keystroke.
        private string _cameraMeasuredMM;
        public string CameraMeasuredMM
        {
            get => _cameraMeasuredMM;
            set
            {
                Set(ref _cameraMeasuredMM, value);
                SetCameraPixelsPerMMCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _clearAllVisionProfilePixelOverrides;
        public bool ClearAllVisionProfilePixelOverrides
        {
            get => _clearAllVisionProfilePixelOverrides;
            set => Set(ref _clearAllVisionProfilePixelOverrides, value);
        }

        private string _cameraCalibrationStatus = "Measure the displayed 200px square to calibrate the selected camera.";
        public string CameraCalibrationStatus
        {
            get => _cameraCalibrationStatus;
            private set => Set(ref _cameraCalibrationStatus, value);
        }

        private async Task SetCameraPixelsPerMMAsync()
        {
            if (_savingCameraPixelsPerMM || SelectedMachineCamera == null ||
                !CameraPixelScaleCalibration.TryCalculateText(CameraMeasuredMM, out var pixelsPerMm))
                return;

            var camera = SelectedMachineCamera;
            var clearOverrides = ClearAllVisionProfilePixelOverrides;
            _savingCameraPixelsPerMM = true;
            SetCameraPixelsPerMMCommand.RaiseCanExecuteChanged();
            try
            {
                CameraPixelScaleCalibration.ApplyToCamera(camera, pixelsPerMm, clearOverrides);
                CameraCalibrationStatus = "Saving camera calibration...";
                var result = await MachineRepo.SaveCurrentMachineAsync();
                if (result.Successful)
                {
                    CameraCalibrationStatus = $"Saved {camera.Name}: {camera.PixelsPerMM:F1} px/mm" +
                        (clearOverrides ? "; profile overrides cleared." : "; profile overrides retained.");
                    Machine?.AddStatusMessage(StatusMessageTypes.Info, CameraCalibrationStatus);
                }
                else
                {
                    CameraCalibrationStatus = $"Calibration applied locally but save failed: {result.ErrorMessage}";
                    Machine?.AddStatusMessage(StatusMessageTypes.Warning, CameraCalibrationStatus);
                }
            }
            catch (Exception ex)
            {
                CameraCalibrationStatus = $"Could not save camera calibration: {ex.Message}";
                Machine?.AddStatusMessage(StatusMessageTypes.Warning, CameraCalibrationStatus);
            }
            finally
            {
                _savingCameraPixelsPerMM = false;
                SetCameraPixelsPerMMCommand.RaiseCanExecuteChanged();
            }
        }

        private readonly System.Diagnostics.Stopwatch _exposureRunClock = new System.Diagnostics.Stopwatch();
        private TimeSpan _exposureStepStarted;
        private int _exposureCurrentStep = -1;
        private bool _exposureCalibrationRunning;
        private string _exposureCalibrationStatus = "Ready for exposure calibration.";

        public ObservableCollection<ExposureCalibrationStep> ExposureSteps { get; } =
            new ObservableCollection<ExposureCalibrationStep>();

        public bool ExposureCalibrationRunning
        {
            get => _exposureCalibrationRunning;
            private set
            {
                if (Set(ref _exposureCalibrationRunning, value))
                {
                    RaisePropertyChanged(nameof(ExposureCalibrationCanRun));
                    AutoTuneCameraExposureCommand.RaiseCanExecuteChanged();
                }
            }
        }

        public bool ExposureCalibrationCanRun => !ExposureCalibrationRunning;

        public string ExposureCalibrationStatus
        {
            get => _exposureCalibrationStatus;
            private set => Set(ref _exposureCalibrationStatus, value);
        }

        public string ExposureElapsed => _exposureRunClock.Elapsed.TotalSeconds.ToString("F1") + " s";

        private void ResetExposureSteps()
        {
            ExposureSteps.Clear();
            foreach (var name in new[]
            {
                "Check camera", "Position camera", "Tune exposure",
                "Verify result", "Save calibration"
            })
                ExposureSteps.Add(new ExposureCalibrationStep { Name = name, Status = "Pending" });
            _exposureCurrentStep = -1;
        }

        private void StartExposureStep(int index, string detail)
        {
            if (_exposureCurrentStep >= 0)
            {
                var previous = ExposureSteps[_exposureCurrentStep];
                if (previous.Status == "Running")
                {
                    previous.Status = "Passed";
                    previous.Duration = (_exposureRunClock.Elapsed - _exposureStepStarted)
                        .TotalSeconds.ToString("F1") + " s";
                }
            }

            _exposureCurrentStep = index;
            _exposureStepStarted = _exposureRunClock.Elapsed;
            ExposureSteps[index].Status = "Running";
            ExposureSteps[index].Detail = detail;
            ExposureCalibrationStatus = detail;
            RaisePropertyChanged(nameof(ExposureElapsed));
        }

        private void FailExposureStep(string reason)
        {
            if (_exposureCurrentStep >= 0)
            {
                var step = ExposureSteps[_exposureCurrentStep];
                step.Status = "Failed";
                step.Detail = reason;
                step.Duration = (_exposureRunClock.Elapsed - _exposureStepStarted)
                    .TotalSeconds.ToString("F1") + " s";
            }
            ExposureCalibrationStatus = reason;
            Machine?.AddStatusMessage(StatusMessageTypes.Warning, reason);
            RaisePropertyChanged(nameof(ExposureElapsed));
        }

        // Standalone operation: no machine-origin reset, staging plate calibration
        // or automatic chaining. Also used by the legacy camera Auto-Tune command.
        public async Task RunExposureCalibrationAsync()
        {
            if (ExposureCalibrationRunning)
                return;

            ExposureCalibrationRunning = true;
            ResetExposureSteps();
            _exposureRunClock.Restart();
            MachineCamera camera = null;
            VisionProfile fiducialProfile = null;
            double previousExposure = 0;
            double? previousProfileExposure = null;
            var tuned = false;
            var saved = false;
            try
            {
                StartExposureStep(0, "Checking position camera, machine and live video.");
                var machine = Machine;
                var service = machine?.PositionImageCaptureService;
                camera = SelectedMachineCamera;
                if (machine == null || !machine.Connected || !machine.WasMachineHomed ||
                    machine.Busy || machine.IsLocating || machine.AreSettingsLocked)
                {
                    FailExposureStep("Connect and home an idle machine; unlock settings before calibration.");
                    return;
                }
                if (camera == null || camera.CameraType?.Value != CameraTypes.Position ||
                    service == null || service.Camera == null ||
                    !ReferenceEquals(service.Camera, camera))
                {
                    FailExposureStep("Select the active position camera before calibrating exposure.");
                    return;
                }
                if (MachineConfiguration.MachineFiducial.IsOrigin())
                {
                    FailExposureStep("Configure the machine fiducial location before calibrating exposure.");
                    return;
                }

                var ready = await service.EnsureCaptureReadyAsync();
                if (!ready.Successful)
                {
                    FailExposureStep("No fresh live camera frames: " + ready.ErrorMessage);
                    return;
                }

                previousExposure = camera.Exposure;
                StartExposureStep(1, "Moving safely to the machine fiducial.");
                machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_MachineFiducual);
                fiducialProfile = camera.CurrentVisionProfile;
                if (fiducialProfile?.Key != VisionProfile.VisionProfile_MachineFiducual)
                {
                    FailExposureStep("The required machine-fiducial vision profile is unavailable.");
                    return;
                }
                previousProfileExposure = fiducialProfile.Exposure;

                machine.SendSafeMoveHeight();
                await machine.MoveToCameraAsync();
                machine.GotoPoint(MachineConfiguration.MachineFiducial);
                var move = await machine.SpinUntilIdleAsync(10000);
                if (!move.Successful)
                {
                    FailExposureStep("Camera positioning failed: " + move.ErrorMessage);
                    return;
                }

                if (!machine.Connected || !ReferenceEquals(service.Camera, camera) ||
                    !ReferenceEquals(camera.CurrentVisionProfile, fiducialProfile))
                {
                    FailExposureStep("Machine or active camera/profile changed before tuning.");
                    return;
                }

                StartExposureStep(2, "Sampling the live fiducial and tuning dark/white levels.");
                var result = await service.AutoTuneExposureAsync();
                if (!result.Successful)
                {
                    FailExposureStep("Exposure tuning failed: " + result.ErrorMessage);
                    return;
                }
                tuned = true;

                StartExposureStep(3, "Checking the measured contrast and camera identity.");
                if (!machine.Connected || !ReferenceEquals(service.Camera, camera) ||
                    !ReferenceEquals(camera.CurrentVisionProfile, fiducialProfile) ||
                    service.ExposureTuneProgress?.Usable != true)
                {
                    FailExposureStep("The verified image measurement or active camera/profile changed; nothing saved.");
                    return;
                }

                // Only the machine-fiducial profile inherits this new camera baseline.
                // Unrelated illumination and exposure overrides remain untouched.
                StartExposureStep(4, "Saving the verified camera baseline.");
                fiducialProfile.Exposure = null;
                var persisted = await MachineRepo.SaveCurrentMachineAsync();
                if (!persisted.Successful)
                {
                    FailExposureStep("Exposure verified but save failed: " + persisted.ErrorMessage);
                    return;
                }
                saved = true;
                var step = ExposureSteps[4];
                step.Status = "Passed";
                step.Detail = "Saved exposure " + camera.Exposure.ToString("F1") +
                    " with actual dark/white image measurements.";
                step.Duration = (_exposureRunClock.Elapsed - _exposureStepStarted)
                    .TotalSeconds.ToString("F1") + " s";
                ExposureCalibrationStatus = "Exposure calibrated and saved: " +
                    camera.Exposure.ToString("F1") + ".";
                machine.AddStatusMessage(StatusMessageTypes.Info, ExposureCalibrationStatus);
            }
            catch (Exception ex)
            {
                FailExposureStep("Camera exposure calibration stopped: " + ex.Message);
            }
            finally
            {
                // The sampling engine restores failed tuning itself. Restore the
                // original camera baseline and profile override when verification
                // or persistence fails AFTER a successful candidate measurement.
                if (tuned && !saved && camera != null)
                {
                    camera.Exposure = previousExposure;
                    if (fiducialProfile != null)
                        fiducialProfile.Exposure = previousProfileExposure;
                }
                _exposureRunClock.Stop();
                RaisePropertyChanged(nameof(ExposureElapsed));
                ExposureCalibrationRunning = false;
            }
        }

        private async Task AutoTuneCameraExposureAsync()
        {
            await RunExposureCalibrationAsync();
        }

        protected void MoveToSelectedCamera()
        {
            if (SelectedMachineCamera == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.FatalError, "Please select a camera first.");
            }
            else
            {
                // The fixed bottom camera is located using the moving top
                // (Position) camera. Activate the locator profile on the
                // viewing camera, not on the bottom camera being located.
                // Keep other camera-selection operations unchanged.
                if (SelectedMachineCamera.CameraType?.Value == CameraTypes.PartInspection)
                    Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_CameraLocator);

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
                CameraMeasuredMM = null;
                ClearAllVisionProfilePixelOverrides = false;
                CameraCalibrationStatus = "Measure the displayed 200px square to calibrate the selected camera.";
                SetCameraPixelsPerMMCommand.RaiseCanExecuteChanged();
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
        public RelayCommand SetCameraPixelsPerMMCommand { get; }
        public RelayCommand AutoTuneCameraExposureCommand { get; }
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
