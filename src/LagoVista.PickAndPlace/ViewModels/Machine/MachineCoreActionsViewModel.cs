// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7f322ec2f0d044618d93c0e42b3943defd4c47ec6681a2c6c8567822abd02c64
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System.Diagnostics;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineCoreActionsViewModel : MachineViewModelBase, IMachineCoreActionsViewModel, ICircleLocatedHandler
    {
        ILocatorViewModel _locatorViewModel;
        private readonly StagingPlateCalibrationRunner _stagingPlateCalibration;

        public MachineCoreActionsViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel) : base(machineRepo)
        {
            HomeCommand = new RelayCommand(Home, () => Machine != null && Machine.Connected);
            SkipHomeCommand = new RelayCommand(() => Machine.SetHomed(), () => Machine != null && Machine.Connected && !Machine.WasMachineHomed);
            RegisterCommandHandler(SkipHomeCommand);
            RegisterCommandHandler(HomeCommand);

            MachineVisionOriginCommand = new RelayCommand(MachineVisionOrigin, () => Machine != null && Machine.Connected && Machine.WasMachineHomed);
            RegisterCommandHandler(MachineVisionOriginCommand);

            GoToSafeMoveHeightCommand = CreatedMachineConnectedCommand(() => Machine.SendSafeMoveHeight());
            GoToPartInspectionCameraCommand = CreatedMachineConnectedCommand(GoToPartInspectionCamera);
            SetCameraNavigationCommand = CreatedMachineConnectedCommand(() => Machine.CurrentMachineToolHead = null, () => Machine.CurrentMachineToolHead != null );
            SetToolHeadNavigationCommand = RelayCommand<MachineToolHead>.Create((mh) => Machine.CurrentMachineToolHead = mh,  (mch) => { return Machine != null &&  Machine.WasMachinOriginCalibrated && Machine.Connected && Machine.CurrentMachineToolHead != mch; } );

            _locatorViewModel = locatorViewModel;
            _stagingPlateCalibration = new StagingPlateCalibrationRunner(machineRepo, locatorViewModel);
        }

        public async void GoToPartInspectionCamera()
        {
            await Machine.GoToPartInspectionCameraAsync();
        }

        protected override void MachineChanged(IMachine machine)
        {
            machine.PropertyChanged += Machine_PropertyChanged;
            base.MachineChanged(machine);
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.CurrentMachineToolHead))
            {
                RaiseCanExecuteChanged();
            }

            if (e.PropertyName == nameof(Machine.Busy))
            {
                RaiseCanExecuteChanged();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            SetToolHeadNavigationCommand.RaiseCanExecuteChanged();
            base.RaiseCanExecuteChanged();
        }


        public void Home()
        {
            Machine.HomingCycle();
        }

        Stopwatch _originSW;

        public async void MachineVisionOrigin()
        {
            if (_stagingPlateCalibration.Running)
                return;

            _originSW = Stopwatch.StartNew();
            Machine.DebugWriteLine("------------------------------------");
            Machine.DebugWriteLine("[MachineVisionOrigin] - Start");


            if (MachineConfiguration.MachineFiducial.IsOrigin())
            {
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, "Can not perform machine vision calibration.  No Machine Fiducial Set.  Please set Machine Fiducial on calibration tab.");
            }
            else
            {
                // Only a successful calibration in this session earns a green check.
                foreach (var plate in MachineConfiguration.StagingPlates)
                    plate.IsCalibrated = false;

                Machine.SetVisionProfile(Manufacturing.Models.CameraTypes.Position, VisionProfile.VisionProfile_MachineFiducual);
                Machine.SendSafeMoveHeight();
                await Machine.MoveToCameraAsync();
                Machine.GotoPoint(MachineConfiguration.MachineFiducial);
                await Machine.SpinUntilIdleAsync();
                Machine.DebugWriteLine($"[MachineVisionOrigin] - Moved into position {_originSW.Elapsed.TotalMilliseconds}ms");

                if (Machine.PositionImageCaptureService != null)
                {
                    var autoTuneResult = await Machine.PositionImageCaptureService.AutoTuneExposureAsync();
                    if (autoTuneResult.Successful)
                    {
                        var camera = Machine.PositionImageCaptureService.Camera;
                        // Only the Machine Fiducial inherits the newly calibrated camera baseline.
                        // Other profiles may require a different exposure for their lighting.
                        foreach (var profile in camera.VisionProfiles)
                        {
                            if (profile.Key == VisionProfile.VisionProfile_MachineFiducual)
                                profile.Exposure = null;
                        }

                        var saveResult = await MachineRepo.SaveCurrentMachineAsync();
                        if (saveResult.Successful)
                        {
                            Machine.AddStatusMessage(
                                Manufacturing.Models.StatusMessageTypes.Info,
                                $"Camera exposure calibrated to {camera.Exposure:F1}.");
                        }
                        else
                        {
                            Machine.AddStatusMessage(
                                Manufacturing.Models.StatusMessageTypes.Warning,
                                $"Camera exposure calibrated but could not be saved: {saveResult.ErrorMessage}");
                        }
                    }
                    else
                    {
                        Machine.DebugWriteLine(
                            $"[MachineVisionOrigin] Camera exposure Auto-Tune failed: {autoTuneResult.ErrorMessage}");
                        Machine.AddStatusMessage(
                            Manufacturing.Models.StatusMessageTypes.Warning,
                            $"Camera exposure Auto-Tune skipped during origin calibration: {autoTuneResult.ErrorMessage}");
                    }
                }

                _locatorViewModel.RegisterCircleLocatedHandler(this);
            }
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            if (circle.Stabilized && !Machine.Busy)
            {
                if (circle.Centered)
                {
                    _locatorViewModel.UnregisterCircleLocatedHandler(this);
                    Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.Info, "Found origin");
                    Machine.WasMachinOriginCalibrated = true;
                    Machine.ResetMachineCoordinates(MachineConfiguration.MachineFiducial);
                    Machine.DebugWriteLine($"[MachineVisionOrigin] - Success {_originSW.Elapsed.TotalMilliseconds}ms");
                    Machine.DebugWriteLine("-----------------------------");

                    // Continue with an independent staging-plate runner. The Machine
                    // Fiducial detector and origin-centering path remain unchanged.
                    _ = _stagingPlateCalibration.RunAsync();
                }
                else
                {
                    Machine.GotoPoint(circle.OffsetMM, relativeMove: true);
                }

                circle.Reset();
            }
        }

        public void CircleLocatorTimeout()
        {
            _locatorViewModel.UnregisterCircleLocatedHandler(this);
            Machine.DebugWriteLine($"[MachineVisionOrigin] - Failed {_originSW.Elapsed.TotalMilliseconds}ms");
            Machine.DebugWriteLine("-----------------------------");
        }

        public void CircleLocatorAborted()
        {
            _locatorViewModel.UnregisterCircleLocatedHandler(this);
        }

        public RelayCommand HomeCommand { get;  }
        public RelayCommand SkipHomeCommand { get; }
        public RelayCommand MachineVisionOriginCommand { get; }
        public RelayCommand GoToSafeMoveHeightCommand { get; }
        public RelayCommand GoToPartInspectionCameraCommand { get; }

        public RelayCommand SetCameraNavigationCommand { get; }
        public RelayCommand<MachineToolHead> SetToolHeadNavigationCommand { get; }
    }
}
