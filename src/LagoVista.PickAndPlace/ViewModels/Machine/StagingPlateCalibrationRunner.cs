using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// Moves between the two configured staging-plate reference holes and stores
    /// their measured machine coordinates. Does not change image processing,
    /// circle detection, or the Machine Fiducial locator.
    /// </summary>
    internal sealed class StagingPlateCalibrationRunner : ICircleLocatedHandler
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILocatorViewModel _locator;
        private TaskCompletionSource<Point2D<double>> _pendingObservation;

        public bool Running { get; private set; }

        public StagingPlateCalibrationRunner(IMachineRepo machineRepo, ILocatorViewModel locator)
        {
            _machineRepo = machineRepo;
            _locator = locator;
        }

        public async Task RunAsync()
        {
            if (Running)
                return;

            Running = true;
            var machine = _machineRepo.CurrentMachine;
            try
            {
                if (machine?.PositionImageCaptureService == null)
                {
                    machine?.AddStatusMessage(StatusMessageTypes.Warning,
                        "Staging-plate calibration skipped: position camera is not available.");
                    return;
                }

                // The origin G92 must complete before using machine X/Y coordinates.
                var originReady = await machine.SpinUntilIdleAsync(10000);
                if (!originReady.Successful)
                {
                    machine.AddStatusMessage(StatusMessageTypes.Warning,
                        $"Staging-plate calibration skipped: origin reset did not finish: {originReady.ErrorMessage}");
                    return;
                }

                foreach (var plate in machine.Settings.StagingPlates)
                {
                    plate.IsCalibrated = false;
                    if (!HasReferenceHoles(plate))
                    {
                        machine.AddStatusMessage(StatusMessageTypes.Warning,
                            $"Skipping staging plate {plate.Name}: configure two distinct reference holes and the machine fiducial grid position.");
                        continue;
                    }

                    var firstName = plate.ReferenceHoleRow1.Id + plate.ReferenceHoleColumn1.Id;
                    var secondName = plate.ReferenceHoleRow2.Id + plate.ReferenceHoleColumn2.Id;

                    // Start from the known machine fiducial grid position, not from
                    // potentially stale reference-hole coordinates from an older calibration.
                    var firstTarget = StagingPlateUtils.ResolveStagePlateApproximateWorkSpaceLocation(
                        plate, machine.Settings.MachineFiducial,
                        plate.FiducialColumn.Id, plate.FiducialRow.Id,
                        plate.ReferenceHoleColumn1.Id, plate.ReferenceHoleRow1.Id);

                    machine.AddStatusMessage(StatusMessageTypes.Info,
                        $"Calibrating staging plate {plate.Name}: {firstName}, then {secondName}.");
                    machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_StagingPlateHole);

                    var first = await CenterReferenceHoleAsync(machine, plate, firstName, firstTarget);
                    if (!first.Successful)
                    {
                        machine.AddStatusMessage(StatusMessageTypes.Warning,
                            $"Staging plate {plate.Name} calibration not saved: {first.ErrorMessage}");
                        continue;
                    }

                    // Use the newly measured first reference hole to approach the second.
                    // Translation is corrected here; the second measurement establishes rotation.
                    var secondTarget = StagingPlateUtils.ResolveStagePlateApproximateWorkSpaceLocation(
                        plate, first.Result,
                        plate.ReferenceHoleColumn1.Id, plate.ReferenceHoleRow1.Id,
                        plate.ReferenceHoleColumn2.Id, plate.ReferenceHoleRow2.Id);
                    var second = await CenterReferenceHoleAsync(machine, plate, secondName, secondTarget);
                    if (!second.Successful)
                    {
                        machine.AddStatusMessage(StatusMessageTypes.Warning,
                            $"Staging plate {plate.Name} calibration not saved: {second.ErrorMessage}");
                        continue;
                    }

                    // Preserve the last known-good alignment unless BOTH measurements
                    // and the machine configuration save succeed.
                    var previousFirst = plate.ReferenceHoleLocation1;
                    var previousSecond = plate.ReferenceHoleLocation2;
                    try
                    {
                        plate.ReferenceHoleLocation1 = first.Result;
                        plate.ReferenceHoleLocation2 = second.Result;
                        var saved = await _machineRepo.SaveCurrentMachineAsync();
                        if (!saved.Successful)
                        {
                            machine.AddStatusMessage(StatusMessageTypes.Warning,
                                $"Staging plate {plate.Name} measured but save failed: {saved.ErrorMessage}");
                            continue;
                        }

                        plate.IsCalibrated = true;
                        var message = $"Staging plate {plate.Name} calibrated and saved: " +
                            $"{firstName}=({first.Result.X:F3}, {first.Result.Y:F3}), " +
                            $"{secondName}=({second.Result.X:F3}, {second.Result.Y:F3}).";
                        machine.AddStatusMessage(StatusMessageTypes.Info, message);
                        machine.DebugWriteLine("[StagingPlateCalibration] " + message);
                    }
                    catch (Exception ex)
                    {
                        machine.AddStatusMessage(StatusMessageTypes.Warning,
                            $"Staging plate {plate.Name} could not be saved: {ex.Message}");
                        machine.DebugWriteLine($"[StagingPlateCalibration] Save error: {ex}");
                    }
                    finally
                    {
                        if (!plate.IsCalibrated)
                        {
                            plate.ReferenceHoleLocation1 = previousFirst;
                            plate.ReferenceHoleLocation2 = previousSecond;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                machine?.AddStatusMessage(StatusMessageTypes.Warning,
                    $"Staging-plate calibration stopped: {ex.Message}");
                machine?.DebugWriteLine($"[StagingPlateCalibration] ERROR: {ex}");
            }
            finally
            {
                _locator.UnregisterCircleLocatedHandler(this);
                _pendingObservation = null;
                Running = false;
            }
        }

        private static bool HasReferenceHoles(MachineStagingPlate plate)
        {
            return plate.FiducialRow?.Id != null &&
                   plate.FiducialColumn?.Id != null &&
                   plate.ReferenceHoleRow1?.Id != null &&
                   plate.ReferenceHoleColumn1?.Id != null &&
                   plate.ReferenceHoleRow2?.Id != null &&
                   plate.ReferenceHoleColumn2?.Id != null &&
                   !(plate.ReferenceHoleRow1.Id == plate.ReferenceHoleRow2.Id &&
                     plate.ReferenceHoleColumn1.Id == plate.ReferenceHoleColumn2.Id);
        }

        private async Task<InvokeResult<Point2D<double>>> CenterReferenceHoleAsync(
            IMachine machine, MachineStagingPlate plate, string name, Point2D<double> target)
        {
            machine.AddStatusMessage(StatusMessageTypes.Info,
                $"Staging plate {plate.Name}: moving to {name}, X={target.X:F3}, Y={target.Y:F3}.");
            machine.SendSafeMoveHeight();
            await machine.MoveToCameraAsync();
            machine.GotoPoint(target);
            var moved = await machine.SpinUntilIdleAsync(30000);
            if (!moved.Successful)
                return InvokeResult<Point2D<double>>.FromError(
                    $"Could not move to {name}: {moved.ErrorMessage}");

            for (var attempt = 0; attempt < 6; attempt++)
            {
                if (!machine.Connected)
                    return InvokeResult<Point2D<double>>.FromError("Machine disconnected.");

                // Reuse the existing detector. Clear only its previous hole history;
                // never alter the profile, preprocessing, or detection algorithm.
                machine.PositionImageCaptureService.ResetDetections();
                var completion = new TaskCompletionSource<Point2D<double>>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _pendingObservation = completion;
                _locator.RegisterCircleLocatedHandler(this, 10000);

                Point2D<double> offset;
                try
                {
                    offset = await completion.Task;
                }
                finally
                {
                    _locator.UnregisterCircleLocatedHandler(this);
                    _pendingObservation = null;
                }

                if (offset == null)
                    return InvokeResult<Point2D<double>>.FromError(
                        $"No stable circle detected at {name}.");

                if (offset.X == 0 && offset.Y == 0)
                {
                    var measured = await machine.GetCurrentLocationAsync(5000);
                    if (!measured.Successful)
                        return InvokeResult<Point2D<double>>.FromError(
                            $"Could not read position at {name}: {measured.ErrorMessage}");

                    machine.AddStatusMessage(StatusMessageTypes.Info,
                        $"Staging plate {plate.Name}: {name} centered at X={measured.Result.X:F3}, Y={measured.Result.Y:F3}.");
                    return measured;
                }

                machine.GotoPoint(offset, relativeMove: true);
                var centered = await machine.SpinUntilIdleAsync(10000);
                if (!centered.Successful)
                    return InvokeResult<Point2D<double>>.FromError(
                        $"Centering move at {name} failed: {centered.ErrorMessage}");
            }

            return InvokeResult<Point2D<double>>.FromError(
                $"Could not center {name} within six corrections.");
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            var pending = _pendingObservation;
            if (pending == null || !circle.Stabilized || !circle.PositionStable ||
                _machineRepo.CurrentMachine.Busy)
                return;

            // Only consume the detector's measured result; do not change its state.
            pending.TrySetResult(circle.Centered ? new Point2D<double>(0, 0) : circle.OffsetMM);
        }

        public void CircleLocatorTimeout() => _pendingObservation?.TrySetResult(null);

        public void CircleLocatorAborted() => _pendingObservation?.TrySetResult(null);
    }
}
