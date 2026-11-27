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
using System;
using System.Threading.Tasks;
using LagoVista.Core.Models.Drawing;

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
        private bool _originRunning;
        private TaskCompletionSource<OriginObservation> _originObservation;
        private sealed class OriginObservation
        {
            public bool Centered;
            public Point2D<double> Offset;
        }

        public async void MachineVisionOrigin()
        {
            if (_stagingPlateCalibration.Running || _originRunning)
                return;
            _originRunning = true;
            try
            {
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
                var positioned = await Machine.SpinUntilIdleAsync();
                if (!positioned.Successful)
                    throw new InvalidOperationException("Machine fiducial approach failed: " + positioned.ErrorMessage);
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

                await RunOriginOpticalAsync(Machine);
            }
            }
            catch (Exception ex)
            {
                Machine?.AddStatusMessage(StatusMessageTypes.Warning, "Machine vision origin stopped: " + ex.Message);
            }
            finally { _originRunning = false; }
        }

        // The legacy origin command uses the same per-observation permission
        // as the newer calibration dialogs. No locator callback moves hardware.
        private async Task RunOriginOpticalAsync(IMachine machine)
        {
            for (var correction = 0; correction <= FeederAutoCornerJogMath.MaxAutomaticJogs; correction++)
            {
                if (!ReferenceEquals(Machine, machine) || !machine.Connected || !machine.WasMachineHomed ||
                    machine.CurrentMachineToolHead != null)
                    throw new InvalidOperationException("Machine or camera navigation changed during origin calibration.");

                OriginObservation found;
                await using (var illumination = await machine.BeginTopCameraAcquisitionAsync(6000))
                {
                    machine.PositionImageCaptureService.ResetDetections();
                    var request = new TaskCompletionSource<OriginObservation>(TaskCreationOptions.RunContinuationsAsynchronously);
                    _originObservation = request;
                    try
                    {
                        _locatorViewModel.RegisterCircleLocatedHandler(this, 5000);
                        var completed = await Task.WhenAny(request.Task, Task.Delay(6000));
                        illumination.ThrowIfInvalid();
                        found = completed == request.Task ? await request.Task : null;
                    }
                    finally
                    {
                        _originObservation = null;
                        _locatorViewModel.UnregisterCircleLocatedHandler(this);
                    }
                }

                if (found == null)
                    throw new InvalidOperationException("No stable machine fiducial circle within the observation window.");
                if (!ReferenceEquals(Machine, machine) || !machine.Connected || machine.Busy ||
                    machine.CurrentMachineToolHead != null)
                    throw new InvalidOperationException("Machine changed after fiducial observation.");
                if (found.Centered)
                {
                    var reset = await machine.ResetMachineCoordinates(MachineConfiguration.MachineFiducial);
                    if (!reset.Successful)
                        throw new InvalidOperationException("Origin reset not acknowledged: " + reset.ErrorMessage);
                    machine.WasMachinOriginCalibrated = true;
                    machine.AddStatusMessage(StatusMessageTypes.Info, "Found origin");
                    machine.DebugWriteLine($"[MachineVisionOrigin] - Success {_originSW.Elapsed.TotalMilliseconds}ms");
                    _ = _stagingPlateCalibration.RunAsync();
                    return;
                }
                if (!FeederAutoCornerJogMath.CanJogAgain(correction))
                    throw new InvalidOperationException("Machine fiducial correction limit reached.");
                machine.GotoPoint(found.Offset, relativeMove: true);
                var moved = await machine.SpinUntilIdleAsync(10000);
                if (!moved.Successful)
                    throw new InvalidOperationException("Fiducial correction not acknowledged: " + moved.ErrorMessage);
            }
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            if (circle.CameraType == CameraTypes.Position && circle.Stabilized && Machine?.Busy == false)
            {
                var request = _originObservation;
                if (request != null)
                    request.TrySetResult(new OriginObservation { Centered = circle.Centered, Offset = circle.OffsetMM });
            }
        }

        public void CircleLocatorTimeout() => _originObservation?.TrySetResult(null);
        public void CircleLocatorAborted() => _originObservation?.TrySetResult(null);

        public RelayCommand HomeCommand { get;  }
        public RelayCommand SkipHomeCommand { get; }
        public RelayCommand MachineVisionOriginCommand { get; }
        public RelayCommand GoToSafeMoveHeightCommand { get; }
        public RelayCommand GoToPartInspectionCameraCommand { get; }

        public RelayCommand SetCameraNavigationCommand { get; }
        public RelayCommand<MachineToolHead> SetToolHeadNavigationCommand { get; }
    }
}
