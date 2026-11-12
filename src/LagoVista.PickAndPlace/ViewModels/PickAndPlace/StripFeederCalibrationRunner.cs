using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.Operations;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    /// <summary>
    /// Operator-supervised strip-feeder calibration. Steps 1–3 are executable.
    /// Steps 4–7 remain visible in the shared definition but have no motion
    /// handlers until their own Workstream tasks. The C# runner, not the DTO,
    /// owns all physical motion and persistence.
    /// </summary>
    internal sealed class StripFeederCalibrationRunner : IMachineOperation, ICornerLocatedHandler, ICircleLocatedHandler
    {
        private sealed class CornerProof
        {
            public DateTimeOffset CapturedAtUtc { get; set; }
            public Point2D<double> MachinePosition { get; set; }
            public double OffsetX { get; set; }
            public double OffsetY { get; set; }
            public double PixelsPerMm { get; set; }
        }

        private sealed class CircleProof
        {
            public Point2D<double> MachinePosition { get; set; }
            public double OffsetX { get; set; }
            public double OffsetY { get; set; }
            public double RadiusMm { get; set; }
            public double PixelsPerMm { get; set; }
        }

        private readonly IMachine _machine;
        private readonly ILocatorViewModel _locator;
        private readonly StripFeeder _feeder;
        private readonly StripFeederRow _row;
        private readonly Func<InvokeResult<Point2D<double>>> _resolveCalculatedOrigin;
        private readonly Func<InvokeResult<Point2D<double>>> _resolveFirstTapeHole;
        private readonly Func<double> _getTapePitch;
        private readonly Func<StripFeeder, Task<InvokeResult>> _saveFeeder;
        private readonly Func<bool> _contextIsCurrent;
        private readonly MachineOperationSession _session;
        private readonly object _observationGate = new object();

        private readonly string _plateId;
        private readonly string _referenceRowId;
        private readonly string _referenceColumnId;
        private readonly FeederOrientations _orientation;
        private readonly int _rowIndex;
        private readonly double _mountX;
        private readonly double _mountY;
        private TaskCompletionSource<CornerProof> _pendingCorner;
        private TaskCompletionSource<CircleProof> _pendingCircle;
        private bool _registered;
        private bool _circleRegistered;
        private string _approachedHoleStep;
        private string _tapeProfileKey;
        private string _tapeCameraId;
        private double _tapeCameraScale;
        private Point2D<double> _firstHoleMachinePosition;
        private Point2D<double> _firstHoleRowOffset;
        private bool _busy;
        private bool _disposed;
        private Point2D<double> _calculatedOrigin;
        private string _cameraId;
        private double _cameraScale;

        public event EventHandler StateChanged;

        public MachineOperationDefinition Definition => _session.Definition;
        public MachineOperationRunSnapshot Current => _session.Snapshot;
        public IReadOnlyList<MachineOperationAction> AvailableActions => _session.AvailableActions;
        public bool IsBusy => _busy;
        public string StatusText { get; private set; } =
            "Ready. Move to the calculated feeder origin to start calibration.";
        public string MeasurementText { get; private set; } =
            "No measurements captured yet.";

        public bool CanExecuteNext =>
            !_disposed && !_busy &&
            (Current.Status == MachineOperationRunStatus.Pending ||
             (Current.Status == MachineOperationRunStatus.Running &&
              (Current.ActiveStepKey == "approach-origin" ||
               Current.ActiveStepKey == "center-fiducial" ||
               Current.ActiveStepKey == "save-origin" ||
               Current.ActiveStepKey == "first-tape-hole" ||
               Current.ActiveStepKey == "last-tape-hole")));

        public bool CanRetry => !_disposed && !_busy &&
                                Current.Status == MachineOperationRunStatus.Blocked;
        public bool CanCancel => !_disposed &&
            (Current.Status == MachineOperationRunStatus.Running ||
             Current.Status == MachineOperationRunStatus.Blocked);

        public string NextActionLabel
        {
            get
            {
                if (Current.Status == MachineOperationRunStatus.Pending)
                    return "Move to calculated origin";
                if (Current.Status != MachineOperationRunStatus.Running)
                    return "Awaiting operation";
                switch (Current.ActiveStepKey)
                {
                    case "approach-origin": return "Move to calculated origin";
                    case "center-fiducial": return "Verify centered corner";
                    case "save-origin": return "Capture & save origin offset";
                    case "first-tape-hole": return _approachedHoleStep == "first-tape-hole"
                        ? "Verify first centered tape hole" : "Move to first tape hole";
                    case "last-tape-hole": return _approachedHoleStep == "last-tape-hole"
                        ? "Verify & save first/last holes" : "Move to last tape hole";
                    default: return "Pocket verification pending";
                }
            }
        }

        public StripFeederCalibrationRunner(IMachine machine, ILocatorViewModel locator,
            StripFeeder feeder, StripFeederRow row,
            Func<InvokeResult<Point2D<double>>> resolveCalculatedOrigin,
            Func<InvokeResult<Point2D<double>>> resolveFirstTapeHole,
            Func<double> getTapePitch,
            Func<StripFeeder, Task<InvokeResult>> saveFeeder,
            Func<bool> contextIsCurrent)
        {
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _locator = locator ?? throw new ArgumentNullException(nameof(locator));
            _feeder = feeder ?? throw new ArgumentNullException(nameof(feeder));
            _row = row ?? throw new ArgumentNullException(nameof(row));
            _resolveCalculatedOrigin = resolveCalculatedOrigin ?? throw new ArgumentNullException(nameof(resolveCalculatedOrigin));
            _resolveFirstTapeHole = resolveFirstTapeHole ?? throw new ArgumentNullException(nameof(resolveFirstTapeHole));
            _getTapePitch = getTapePitch ?? throw new ArgumentNullException(nameof(getTapePitch));
            _saveFeeder = saveFeeder ?? throw new ArgumentNullException(nameof(saveFeeder));
            _contextIsCurrent = contextIsCurrent ?? throw new ArgumentNullException(nameof(contextIsCurrent));

            _plateId = feeder.StagingPlate?.Id.ToString();
            _referenceRowId = feeder.ReferenceHoleRow?.Id.ToString();
            _referenceColumnId = feeder.ReferenceHoleColumn?.Id.ToString();
            _orientation = feeder.Orientation.Value;
            _rowIndex = row.RowIndex;
            _mountX = feeder.MountingHoleOffset.X;
            _mountY = feeder.MountingHoleOffset.Y;

            _session = new MachineOperationSession(
                StripFeederCalibrationDefinition.Create(),
                new MachineOperationTarget
                {
                    MachineId = machine.Settings?.Id.ToString(),
                    FeederId = feeder.Id.ToString(),
                    RowIndex = row.RowIndex,
                    StartingPartIndex = row.CurrentPartIndex,
                    CameraId = machine.PositionImageCaptureService?.Camera?.Id.ToString()
                }, DateTimeOffset.UtcNow);
        }

        public async Task<MachineOperationActionResult> ExecuteNextAsync(CancellationToken cancellationToken)
        {
            if (!CanExecuteNext)
                return Result(false, "The operation cannot advance from its current state.");
            _busy = true;
            Signal();
            try
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (Current.Status == MachineOperationRunStatus.Pending)
                {
                    if (!ReadyForAction(false, false, out var reason))
                        return Result(false, reason);
                    _session.Start(DateTimeOffset.UtcNow);
                }

                switch (Current.ActiveStepKey)
                {
                    case "approach-origin":
                        return await ApproachOriginAsync(cancellationToken);
                    case "center-fiducial":
                        return await VerifyCornerAsync(cancellationToken);
                    case "save-origin":
                        return await CaptureAndSaveOriginAsync(cancellationToken);
                    case "first-tape-hole":
                        return await AdvanceTapeHoleAsync(false, cancellationToken);
                    case "last-tape-hole":
                        return await AdvanceTapeHoleAsync(true, cancellationToken);
                    default:
                        return Result(false, "Tape-hole and pocket verification will be enabled in subsequent Workstream tasks.");
                }
            }
            catch (OperationCanceledException)
            {
                return Block("The operator action was cancelled.");
            }
            catch (Exception ex)
            {
                return Block("Calibration action failed: " + ex.Message);
            }
            finally
            {
                _busy = false;
                Signal();
            }
        }

        public Task<MachineOperationActionResult> ConfirmAsync(CancellationToken cancellationToken)
        {
            if (Current.Status != MachineOperationRunStatus.Running ||
                Current.ActiveStepKey != "save-origin")
                return Task.FromResult(Result(false, "There is no origin-offset confirmation pending."));
            return ExecuteNextAsync(cancellationToken);
        }

        public Task<MachineOperationActionResult> RetryAsync(CancellationToken cancellationToken)
        {
            if (!CanRetry)
                return Task.FromResult(Result(false, "Only a blocked step can be retried."));
            _session.Retry(DateTimeOffset.UtcNow);
            StatusText = "Retry is ready. Press the primary action to repeat only this step.";
            Signal();
            return Task.FromResult(Result(true));
        }

        public Task<MachineOperationActionResult> CancelAsync(CancellationToken cancellationToken)
        {
            if (Current.Status != MachineOperationRunStatus.Running &&
                Current.Status != MachineOperationRunStatus.Blocked)
                return Task.FromResult(Result(false, "There is no active calibration to cancel."));

            _session.Cancel(DateTimeOffset.UtcNow);
            ReleaseObservation();
            ReleaseCircleObservation();
            StatusText = "Calibration cancelled. No further moves or saves will be started. A previously queued machine move may still finish.";
            Signal();
            return Task.FromResult(Result(true));
        }

        public void Dispose()
        {
            if (_disposed)
                return;
            if (CanCancel)
                _session.Cancel(DateTimeOffset.UtcNow);
            _disposed = true;
            ReleaseObservation();
            ReleaseCircleObservation();
            Signal();
        }

        private async Task<MachineOperationActionResult> ApproachOriginAsync(CancellationToken token)
        {
            if (!ReadyForAction(false, false, out var reason))
                return Block(reason);

            // FindLocation(FeederOrigin) activates the Feeder Origin vision
            // profile and intentionally excludes any previously saved offset.
            var target = _resolveCalculatedOrigin();
            if (!target.Successful || target.Result == null)
                return Block(target.ErrorMessage ?? "Could not calculate the feeder origin.");

            var point = target.Result;
            var workArea = _machine.Settings.WorkAreaSize;
            if (point.X < 0 || point.Y < 0 || point.X > workArea.X || point.Y > workArea.Y)
                return Block("Calculated origin is outside the configured work area. No move was sent.");

            // The FeederOrigin resolver selects the camera profile. Capture
            // the resulting camera identity and effective scale BEFORE testing
            // whether either has changed. The previous order compared these
            // values to their uninitialized defaults on the first click.
            var capture = _machine.PositionImageCaptureService;
            if (capture?.Camera == null)
                return Block("The position camera is unavailable.");
            if (capture.Profile?.Key != VisionProfile.VisionProfile_FeederOrigin)
                return Block("Feeder Origin vision profile did not become active. No move was sent.");

            _cameraId = capture.Camera.Id.ToString();
            _cameraScale = capture.Camera.EffectivePixelsPerMM;
            if (_cameraScale <= 0 || Double.IsNaN(_cameraScale) || Double.IsInfinity(_cameraScale))
                return Block("The position camera has no valid pixels/mm calibration.");
            if (!ReadyForAction(true, false, out reason))
                return Block(reason);

            _calculatedOrigin = new Point2D<double>(point.X, point.Y);

            StatusText = $"Moving to calculated origin X={point.X:F2}, Y={point.Y:F2} mm; saved OriginOffset is ignored.";
            Signal();
            token.ThrowIfCancellationRequested();

            _machine.SendSafeMoveHeight();
            var safeHeight = await _machine.SpinUntilIdleAsync(10000);
            if (!safeHeight.Successful)
                return Block("Safe-height move did not finish: " + safeHeight.ErrorMessage);
            if (!ReadyForAction(true, false, out reason) || Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Calibration was cancelled during the move.");

            capture.ResetDetections();
            _machine.GotoPoint(point);
            var idle = await _machine.SpinUntilIdleAsync(30000);
            if (!idle.Successful)
                return Block("Calculated-origin move did not finish: " + idle.ErrorMessage);
            if (!ReadyForAction(true, false, out reason) || Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Machine context changed after the move.");

            var measured = await _machine.GetCurrentLocationAsync(5000);
            if (!measured.Successful || !FeederOriginCalibrationMath.SamePosition(measured.Result, point, 0.5))
                return Block("Machine did not confirm reaching the calculated origin within 0.5 mm.");

            var now = DateTimeOffset.UtcNow;
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = Current.RunId,
                StepKey = "approach-origin",
                Name = "Calculated origin machine XY",
                Value = $"X {measured.Result.X:F2}, Y {measured.Result.Y:F2}",
                Unit = "mm",
                Source = "Machine position readback",
                Verification = MachineOperationVerificationKind.MachineObserved,
                CapturedAtUtc = now,
                VisionProfileKey = VisionProfile.VisionProfile_FeederOrigin,
                PixelsPerMm = _cameraScale
            });
            _session.CompleteActiveStep("approach-origin", now);
            MeasurementText = $"Calculated: X {point.X:F2}, Y {point.Y:F2} mm";
            StatusText = "At calculated origin. Use the camera's Jog button to center the white corner, then press Verify centered corner.";
            Signal();
            return Result(true);
        }

        private async Task<MachineOperationActionResult> VerifyCornerAsync(CancellationToken token)
        {
            if (!ReadyForAction(true, false, out var reason))
                return Block(reason);
            StatusText = "Acquiring a fresh stable and CENTERED corner. Please keep the machine still.";
            Signal();
            var corner = await AcquireFreshCornerAsync(token);
            if (corner == null)
                return Block("No fresh stable, centered corner was acquired. Jog using the camera control and retry verification.");
            if (!ReadyForAction(true, false, out reason) || Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Operation was cancelled during corner verification.");

            var measured = await _machine.GetCurrentLocationAsync(5000);
            if (!measured.Successful ||
                !FeederOriginCalibrationMath.SamePosition(measured.Result, corner.MachinePosition, 0.1))
                return Block("Machine position changed since the centered corner was observed.");

            var now = DateTimeOffset.UtcNow;
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = Current.RunId,
                StepKey = "center-fiducial",
                Name = "Centered white feeder corner",
                Value = $"X {corner.OffsetX:+0.00;-0.00;0.00}, Y {corner.OffsetY:+0.00;-0.00;0.00}",
                Unit = "mm",
                Source = "Position camera / white-corner detector",
                Verification = MachineOperationVerificationKind.VisionVerified,
                CapturedAtUtc = now,
                VisionProfileKey = VisionProfile.VisionProfile_FeederOrigin,
                PixelsPerMm = corner.PixelsPerMm
            });
            _session.CompleteActiveStep("center-fiducial", now);
            MeasurementText = $"Corner X {corner.OffsetX:+0.00;-0.00;0.00}, Y {corner.OffsetY:+0.00;-0.00;0.00} mm; scale {corner.PixelsPerMm:F1} px/mm";
            StatusText = "Corner verified and centered. Press Capture & save origin offset to recheck and persist the measured position.";
            Signal();
            return Result(true);
        }

        private async Task<MachineOperationActionResult> CaptureAndSaveOriginAsync(CancellationToken token)
        {
            if (!ReadyForAction(true, true, out var reason))
                return Block(reason);
            if (_calculatedOrigin == null)
                return Block("Calculated origin was not recorded. Restart calibration.");

            StatusText = "Rechecking fresh corner centering before saving the feeder offset.";
            Signal();
            var corner = await AcquireFreshCornerAsync(token);
            if (corner == null)
                return Block("Corner verification expired. Recenter, then retry Capture & Save.");
            if (!ReadyForAction(true, true, out reason) || Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Machine context changed before origin save.");

            var measured = await _machine.GetCurrentLocationAsync(5000);
            if (!measured.Successful ||
                !FeederOriginCalibrationMath.SamePosition(measured.Result, corner.MachinePosition, 0.1))
                return Block("Machine moved after camera confirmation; origin offset was not saved.");

            if (!FeederOriginCalibrationMath.TryCalculate(
                    _calculatedOrigin, measured.Result, corner.PixelsPerMm,
                    positionStable: true, centered: true, out var newOffset))
                return Block("Invalid measured feeder origin; calibration was not saved.");

            var previous = new Point2D<double>(_feeder.OriginOffset.X, _feeder.OriginOffset.Y);
            _feeder.OriginOffset = newOffset;
            InvokeResult saved;
            try
            {
                saved = await _saveFeeder(_feeder);
            }
            catch
            {
                _feeder.OriginOffset = previous;
                throw;
            }

            if (!saved.Successful)
            {
                _feeder.OriginOffset = previous;
                return Block("Could not persist feeder origin; previous offset was restored locally. " + saved.ErrorMessage);
            }

            if (Current.Status != MachineOperationRunStatus.Running)
                return Result(false, "Feeder save completed after cancellation. Reload the feeder before another calibration.");
            var now = DateTimeOffset.UtcNow;
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = Current.RunId,
                StepKey = "save-origin",
                Name = "Saved feeder OriginOffset",
                Value = $"X {newOffset.X:+0.00;-0.00;0.00}, Y {newOffset.Y:+0.00;-0.00;0.00}",
                Unit = "mm",
                Source = "Operator confirmation / Manufacturing feeder PUT",
                Verification = MachineOperationVerificationKind.OperatorConfirmed,
                Saved = true,
                CapturedAtUtc = now,
                VisionProfileKey = VisionProfile.VisionProfile_FeederOrigin,
                PixelsPerMm = corner.PixelsPerMm
            });
            _session.CompleteActiveStep("save-origin", now);
            MeasurementText = $"SAVED OriginOffset X {newOffset.X:+0.00;-0.00;0.00}, Y {newOffset.Y:+0.00;-0.00;0.00} mm";
            StatusText = "Feeder origin saved. Next: move to the first complete tape hole. Manual camera Jog remains available.";
            Signal();
            return Result(true);
        }

        private async Task<CornerProof> AcquireFreshCornerAsync(CancellationToken token)
        {
            TaskCompletionSource<CornerProof> request;
            lock (_observationGate)
            {
                request = new TaskCompletionSource<CornerProof>(TaskCreationOptions.RunContinuationsAsynchronously);
                _pendingCorner = request;
            }

            try
            {
                // Only register during the explicit verification action. The
                // locator's IsLocating state otherwise disables normal Jog.
                _machine.PositionImageCaptureService.ResetDetections();
                _locator.RegisterCornerLocatedHandler(this, 7500);
                _registered = true;

                var timedOut = await Task.WhenAny(request.Task, Task.Delay(8000, token));
                if (timedOut != request.Task)
                    return null;
                return await request.Task;
            }
            finally
            {
                ReleaseObservation();
            }
        }

        public void CornerLocated(MVLocatedCorner corner)
        {
            TaskCompletionSource<CornerProof> request;
            lock (_observationGate)
                request = _pendingCorner;

            if (request == null || _disposed || corner == null ||
                corner.Camera != CameraTypes.Position ||
                !corner.Stabilized || !corner.PositionStable || !corner.Centered ||
                _machine.Busy || !_machine.Connected ||
                _machine.Mode != OperatingMode.Manual)
                return;

            var capture = _machine.PositionImageCaptureService;
            if (!FeederOriginCalibrationMath.CameraProfileUnchanged(
                    _cameraId, _cameraScale,
                    capture?.Camera?.Id.ToString(),
                    capture?.Camera?.EffectivePixelsPerMM ?? 0,
                    capture?.Profile?.Key, VisionProfile.VisionProfile_FeederOrigin))
                return;

            var machinePosition = _machine.MachinePosition;
            var offset = corner.OffsetMM;
            request.TrySetResult(new CornerProof
            {
                CapturedAtUtc = DateTimeOffset.UtcNow,
                MachinePosition = new Point2D<double>(machinePosition.X, machinePosition.Y),
                OffsetX = offset.X,
                OffsetY = offset.Y,
                PixelsPerMm = corner.PixelsPerMm
            });
        }

        public void CornerLocatorTimeout()
        {
            lock (_observationGate)
                _pendingCorner?.TrySetResult(null);
        }

        public void CornerLocatorAborted()
        {
            lock (_observationGate)
                _pendingCorner?.TrySetResult(null);
        }

        private void ReleaseObservation()
        {
            lock (_observationGate)
            {
                _pendingCorner?.TrySetResult(null);
                _pendingCorner = null;
            }
            if (_registered)
            {
                _registered = false;
                _locator.UnRegisterCornerLocatedHandler(this);
            }
        }

        private bool ReadyForAction(bool requireProfile, bool saving, out string reason)
        {
            reason = null;
            if (_disposed || !_contextIsCurrent())
                reason = "Machine, feeder or row selection changed.";
            else if (!_machine.Connected || _machine.Mode != OperatingMode.Manual ||
                     !_machine.WasMachineHomed || !_machine.WasMachinOriginCalibrated)
                reason = "A connected, homed, origin-calibrated machine in Manual mode is required.";
            else if (_machine.Busy)
                reason = "Machine is busy; complete the previous move first.";
            else if (_feeder.StagingPlate?.Id.ToString() != _plateId ||
                     _feeder.ReferenceHoleRow?.Id.ToString() != _referenceRowId ||
                     _feeder.ReferenceHoleColumn?.Id.ToString() != _referenceColumnId ||
                     _feeder.Orientation.Value != _orientation ||
                     _row.RowIndex != _rowIndex ||
                     _feeder.MountingHoleOffset.X != _mountX ||
                     _feeder.MountingHoleOffset.Y != _mountY)
                reason = "Feeder mounting geometry changed; start a new calibration.";
            else if (_machine.PositionImageCaptureService?.Camera == null)
                reason = "The position camera is unavailable.";
            else if (requireProfile &&
                     !FeederOriginCalibrationMath.CameraProfileUnchanged(
                         _cameraId, _cameraScale,
                         _machine.PositionImageCaptureService.Camera.Id.ToString(),
                         _machine.PositionImageCaptureService.Camera.EffectivePixelsPerMM,
                         _machine.PositionImageCaptureService.Profile?.Key,
                         VisionProfile.VisionProfile_FeederOrigin))
                reason = "Position camera, Feeder Origin profile or camera scale changed; start over.";
            else if (saving && _machine.AreSettingsLocked)
                reason = "Machine settings are locked; unlock before saving calibration.";
            return reason == null;
        }

        private MachineOperationActionResult Block(string message)
        {
            if (Current.Status == MachineOperationRunStatus.Running)
                _session.BlockActiveStep(message, DateTimeOffset.UtcNow);
            StatusText = message;
            Signal();
            return Result(false, message);
        }

        private MachineOperationActionResult Result(bool success, string error = null) =>
            new MachineOperationActionResult
            {
                Successful = success,
                ErrorMessage = error,
                Snapshot = Current
            };

        private void Signal() => StateChanged?.Invoke(this, EventArgs.Empty);
    }
}
