// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7f322ec2f0d044618d93c0e42b3943defd4c47ec6681a2c6c8567822abd02c64
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineCoreActionsViewModel : MachineViewModelBase, IMachineCoreActionsViewModel, ICircleLocatedHandler
    {
        ILocatorViewModel _locatorViewModel;
        public MachineCoreActionsViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel) : base(machineRepo)
        {
            HomeCommand = new RelayCommand(Home, () => Machine != null && Machine.Connected);
            SkipHomeCommand = new RelayCommand(() => Machine.SetHomed(), () => Machine != null && Machine.Connected && !Machine.WasMachineHomed);
            RegisterCommandHandler(SkipHomeCommand);
            RegisterCommandHandler(HomeCommand);

            MachineVisionOriginCommand = new RelayCommand(MachineVisionOrigin, () => Machine != null && Machine.Connected && Machine.WasMachineHomed && !_machineCalibrationInProgress);
            RegisterCommandHandler(MachineVisionOriginCommand);

            GoToSafeMoveHeightCommand = CreatedMachineConnectedCommand(() => Machine.SendSafeMoveHeight());
            GoToPartInspectionCameraCommand = CreatedMachineConnectedCommand(GoToPartInspectionCamera);
            SetCameraNavigationCommand = CreatedMachineConnectedCommand(() => Machine.CurrentMachineToolHead = null, () => Machine.CurrentMachineToolHead != null );
            SetToolHeadNavigationCommand = RelayCommand<MachineToolHead>.Create((mh) => Machine.CurrentMachineToolHead = mh,  (mch) => { return Machine != null &&  Machine.WasMachinOriginCalibrated && Machine.Connected && Machine.CurrentMachineToolHead != mch; } );

            _locatorViewModel = locatorViewModel;
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
        private bool _machineCalibrationInProgress;
        private bool _originLocating;
        private TaskCompletionSource<Point2D<double>> _plateCircleCompletion;

        private void FinishMachineCalibration()
        {
            _originLocating = false;
            _machineCalibrationInProgress = false;
            MachineVisionOriginCommand.RaiseCanExecuteChanged();
        }

        public async void MachineVisionOrigin()
        {
            if (_machineCalibrationInProgress)
                return;

            _machineCalibrationInProgress = true;
            MachineVisionOriginCommand.RaiseCanExecuteChanged();
            _originSW = Stopwatch.StartNew();
            Machine.DebugWriteLine("------------------------------------");
            Machine.DebugWriteLine("[MachineVisionOrigin] - Start");

            var registeredOriginLocator = false;
            try
            {
                if (MachineConfiguration.MachineFiducial.IsOrigin())
                {
                    Machine.AddStatusMessage(StatusMessageTypes.FatalError,
                        "Cannot calibrate machine: Machine Fiducial is not configured.");
                    return;
                }

                // The check mark means calibrated and saved during this session,
                // not merely that old reference coordinates are available.
                foreach (var plate in MachineConfiguration.StagingPlates)
                    plate.IsCalibrated = false;

                Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_MachineFiducual);
                Machine.SendSafeMoveHeight();
                await Machine.MoveToCameraAsync();
                Machine.GotoPoint(MachineConfiguration.MachineFiducial);
                var moveResult = await Machine.SpinUntilIdleAsync(30000);
                if (!moveResult.Successful)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        $"Machine calibration could not reach the fiducial: {moveResult.ErrorMessage}");
                    return;
                }
                Machine.DebugWriteLine($"[MachineVisionOrigin] - Moved into position {_originSW.Elapsed.TotalMilliseconds}ms");

                if (Machine.PositionImageCaptureService != null)
                {
                    var autoTuneResult = await Machine.PositionImageCaptureService.AutoTuneExposureAsync();
                    if (autoTuneResult.Successful)
                    {
                        var camera = Machine.PositionImageCaptureService.Camera;
                        foreach (var profile in camera.VisionProfiles)
                        {
                            if (profile.Key == VisionProfile.VisionProfile_MachineFiducual)
                                profile.Exposure = null;
                        }

                        var saveResult = await MachineRepo.SaveCurrentMachineAsync();
                        if (saveResult.Successful)
                        {
                            Machine.AddStatusMessage(StatusMessageTypes.Info,
                                $"Camera exposure calibrated to {camera.Exposure:F1}.");
                        }
                        else
                        {
                            Machine.AddStatusMessage(StatusMessageTypes.Warning,
                                $"Camera exposure calibrated but could not be saved: {saveResult.ErrorMessage}");
                        }
                    }
                    else
                    {
                        Machine.DebugWriteLine(
                            $"[MachineVisionOrigin] Camera exposure Auto-Tune failed: {autoTuneResult.ErrorMessage}");
                        Machine.AddStatusMessage(StatusMessageTypes.Warning,
                            $"Camera exposure Auto-Tune skipped during origin calibration: {autoTuneResult.ErrorMessage}");
                    }
                }

                _originLocating = true;
                _locatorViewModel.RegisterCircleLocatedHandler(this);
                registeredOriginLocator = true;
            }
            catch (Exception ex)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning,
                    $"Machine calibration failed: {ex.Message}");
                Machine.DebugWriteLine($"[MachineVisionOrigin] ERROR: {ex}");
            }
            finally
            {
                // The successful origin locator hands off to staging calibration.
                // Otherwise release the command immediately.
                if (!registeredOriginLocator)
                    FinishMachineCalibration();
            }
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            if (!circle.Stabilized || Machine.Busy)
                return;

            // Staging-plate measurements use the same production circle-locator
            // callback, with one clean observation returned to the async caller.
            if (_plateCircleCompletion != null)
            {
                if (!circle.PositionStable)
                    return;

                var completion = _plateCircleCompletion;
                var offset = circle.Centered ? new Point2D<double>(0, 0) : circle.OffsetMM;
                _locatorViewModel.UnregisterCircleLocatedHandler(this);
                circle.Reset();
                completion.TrySetResult(offset);
                return;
            }

            if (!_originLocating)
                return;

            if (circle.Centered)
            {
                _originLocating = false;
                _locatorViewModel.UnregisterCircleLocatedHandler(this);
                Machine.AddStatusMessage(StatusMessageTypes.Info, "Found origin");
                Machine.WasMachinOriginCalibrated = true;
                Machine.ResetMachineCoordinates(MachineConfiguration.MachineFiducial);
                Machine.DebugWriteLine($"[MachineVisionOrigin] - Success {_originSW.Elapsed.TotalMilliseconds}ms");
                Machine.DebugWriteLine("-----------------------------");

                // One Calibrate Machine action now includes configured staging plates.
                _ = CalibrateConfiguredStagingPlatesAsync();
            }
            else
            {
                Machine.GotoPoint(circle.OffsetMM, relativeMove: true);
                circle.Reset();
            }
        }

        private static bool HasConfiguredReferenceHoles(MachineStagingPlate plate)
        {
            if (plate.ReferenceHoleColumn1 == null || plate.ReferenceHoleRow1 == null ||
                plate.ReferenceHoleColumn2 == null || plate.ReferenceHoleRow2 == null ||
                plate.ReferenceHoleLocation1 == null || plate.ReferenceHoleLocation2 == null)
                return false;

            if (plate.ReferenceHoleColumn1.Id == plate.ReferenceHoleColumn2.Id &&
                plate.ReferenceHoleRow1.Id == plate.ReferenceHoleRow2.Id)
                return false;

            // Unconfigured machine references default to (0,0). Do not move there.
            return !(plate.ReferenceHoleLocation1.X == 0 && plate.ReferenceHoleLocation1.Y == 0) &&
                   !(plate.ReferenceHoleLocation2.X == 0 && plate.ReferenceHoleLocation2.Y == 0);
        }

        private async Task<InvokeResult<Point2D<double>>> LocateStagingHoleAsync(
            MachineStagingPlate plate, string holeName, Point2D<double> approximateLocation)
        {
            Machine.SendSafeMoveHeight();
            await Machine.MoveToCameraAsync();
            Machine.GotoPoint(approximateLocation);

            var move = await Machine.SpinUntilIdleAsync(30000);
            if (!move.Successful)
                return InvokeResult<Point2D<double>>.FromError(
                    $"Could not move to {holeName}: {move.ErrorMessage}");

            for (var attempt = 0; attempt < 6; attempt++)
            {
                if (!Machine.Connected)
                    return InvokeResult<Point2D<double>>.FromError("Machine disconnected.");

                // The next physical hole must not inherit the previous hole's
                // tracked circles or its stabilization count.
                Machine.PositionImageCaptureService.ResetDetections();
                var completion = new TaskCompletionSource<Point2D<double>>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _plateCircleCompletion = completion;
                _locatorViewModel.RegisterCircleLocatedHandler(this, 10000);

                Point2D<double> offset;
                try
                {
                    offset = await completion.Task;
                }
                finally
                {
                    _locatorViewModel.UnregisterCircleLocatedHandler(this);
                    _plateCircleCompletion = null;
                }

                if (offset == null)
                    return InvokeResult<Point2D<double>>.FromError(
                        $"No stable staging-plate hole was found at {holeName}.");

                if (offset.X == 0 && offset.Y == 0)
                {
                    var position = await Machine.GetCurrentLocationAsync(5000);
                    if (!position.Successful)
                        return InvokeResult<Point2D<double>>.FromError(
                            $"Could not read machine position for {holeName}: {position.ErrorMessage}");

                    Machine.DebugWriteLine(
                        $"[StagingPlateCalibration] {plate.Name} {holeName} centered at X={position.Result.X:F3}, Y={position.Result.Y:F3}");
                    return position;
                }

                Machine.GotoPoint(offset, relativeMove: true);
                var jog = await Machine.SpinUntilIdleAsync(10000);
                if (!jog.Successful)
                    return InvokeResult<Point2D<double>>.FromError(
                        $"Centering move for {holeName} failed: {jog.ErrorMessage}");
            }

            return InvokeResult<Point2D<double>>.FromError(
                $"Could not center {holeName} after six corrections.");
        }

        private async Task CalibrateConfiguredStagingPlatesAsync()
        {
            try
            {
                // The origin reset (G92) must finish before measuring plate coordinates.
                var originReady = await Machine.SpinUntilIdleAsync(10000);
                if (!originReady.Successful)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        $"Origin was found, but machine coordinates did not settle: {originReady.ErrorMessage}");
                    return;
                }

                foreach (var plate in MachineConfiguration.StagingPlates)
                {
                    plate.IsCalibrated = false;

                    if (!HasConfiguredReferenceHoles(plate))
                    {
                        Machine.AddStatusMessage(StatusMessageTypes.Warning,
                            $"Skipping staging plate {plate.Name}: configure two distinct reference holes and their approximate locations.");
                        continue;
                    }

                    var hole1 = $"{plate.ReferenceHoleRow1.Id}{plate.ReferenceHoleColumn1.Id}";
                    var hole2 = $"{plate.ReferenceHoleRow2.Id}{plate.ReferenceHoleColumn2.Id}";
                    var old1 = plate.ReferenceHoleLocation1;
                    var old2 = plate.ReferenceHoleLocation2;
                    Machine.AddStatusMessage(StatusMessageTypes.Info,
                        $"Calibrating staging plate {plate.Name} using {hole1} and {hole2}.");
                    Machine.DebugWriteLine(
                        $"[StagingPlateCalibration] {plate.Name} starting. {hole1}={old1}; {hole2}={old2}");

                    Machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_StagingPlateHole);

                    var first = await LocateStagingHoleAsync(plate, hole1, old1);
                    if (!first.Successful)
                    {
                        Machine.AddStatusMessage(StatusMessageTypes.Warning,
                            $"Staging plate {plate.Name} calibration skipped: {first.ErrorMessage}");
                        continue;
                    }

                    var second = await LocateStagingHoleAsync(plate, hole2, old2);
                    if (!second.Successful)
                    {
                        Machine.AddStatusMessage(StatusMessageTypes.Warning,
                            $"Staging plate {plate.Name} calibration skipped: {second.ErrorMessage}");
                        continue;
                    }

                    // Both observations succeeded: replace the existing two-point
                    // transform together and roll back both values if persistence fails.
                    try
                    {
                        plate.ReferenceHoleLocation1 = first.Result;
                        plate.ReferenceHoleLocation2 = second.Result;
                        var saved = await MachineRepo.SaveCurrentMachineAsync();
                        if (!saved.Successful)
                        {
                            Machine.AddStatusMessage(StatusMessageTypes.Warning,
                                $"Staging plate {plate.Name} measured but could not be saved: {saved.ErrorMessage}");
                            continue;
                        }

                        plate.IsCalibrated = true;
                        var message = $"Staging plate {plate.Name} calibrated and saved: " +
                            $"{hole1}=({first.Result.X:F3}, {first.Result.Y:F3}), " +
                            $"{hole2}=({second.Result.X:F3}, {second.Result.Y:F3}).";
                        Machine.AddStatusMessage(StatusMessageTypes.Info, message);
                        Machine.DebugWriteLine($"[StagingPlateCalibration] SUCCESS {message}");
                    }
                    catch (Exception ex)
                    {
                        Machine.AddStatusMessage(StatusMessageTypes.Warning,
                            $"Could not save staging plate {plate.Name}: {ex.Message}");
                        Machine.DebugWriteLine($"[StagingPlateCalibration] Save failed: {ex}");
                    }
                    finally
                    {
                        if (!plate.IsCalibrated)
                        {
                            plate.ReferenceHoleLocation1 = old1;
                            plate.ReferenceHoleLocation2 = old2;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning,
                    $"Staging plate calibration failed: {ex.Message}");
                Machine.DebugWriteLine($"[StagingPlateCalibration] ERROR: {ex}");
            }
            finally
            {
                _locatorViewModel.UnregisterCircleLocatedHandler(this);
                FinishMachineCalibration();
            }
        }

        public void CircleLocatorTimeout()
        {
            if (_plateCircleCompletion != null)
            {
                _plateCircleCompletion.TrySetResult(null);
                return;
            }

            if (_originLocating)
            {
                _originLocating = false;
                _locatorViewModel.UnregisterCircleLocatedHandler(this);
                Machine.DebugWriteLine($"[MachineVisionOrigin] - Failed {_originSW.Elapsed.TotalMilliseconds}ms");
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine Fiducial could not be centered.");
                FinishMachineCalibration();
            }
        }

        public void CircleLocatorAborted()
        {
            if (_plateCircleCompletion != null)
            {
                _plateCircleCompletion.TrySetResult(null);
                return;
            }

            if (_originLocating)
            {
                _originLocating = false;
                _locatorViewModel.UnregisterCircleLocatedHandler(this);
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Machine calibration was aborted.");
                FinishMachineCalibration();
            }
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
