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
    /// Operator-supervised seven-step strip feeder calibration.
    /// Steps 6–7 use operator-confirmed camera inspection without picking.
    /// Strongly typed C# handlers, never data-only DTOs, own machine motion.
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
        private readonly Func<int, InvokeResult<Point2D<double>>> _resolvePocket;
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
        private string _approachedPocketStep;
        private Point2D<double> _pocketTarget;
        private int _pocketIndex;
        private string _pocketProfileKey;
        private string _pocketCameraId;
        private double _pocketCameraScale;
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
        // Automation is an operator-supervised convenience: no automatic saves,
        // inventory updates, or visual part-presence confirmations.
        public bool CanAutoAdvance => !_disposed && !_busy &&
            FeederCalibrationAutoAdvancePolicy.CanAdvance(
                Current.Status, Current.ActiveStepKey,
                _approachedHoleStep == Current.ActiveStepKey,
                _approachedPocketStep == Current.ActiveStepKey);
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
               Current.ActiveStepKey == "last-tape-hole" ||
               Current.ActiveStepKey == "verify-current-part" ||
               Current.ActiveStepKey == "verify-previous-empty")));

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
                    case "verify-current-part": return _approachedPocketStep == "verify-current-part"
                        ? "Confirm current part PRESENT" : "Move to current part";
                    case "verify-previous-empty": return _approachedPocketStep == "verify-previous-empty"
                        ? "Confirm previous pocket EMPTY" : "Move to previous pocket";
                    default: return "Awaiting operation";
                }
            }
        }

        public StripFeederCalibrationRunner(IMachine machine, ILocatorViewModel locator,
            StripFeeder feeder, StripFeederRow row,
            Func<InvokeResult<Point2D<double>>> resolveCalculatedOrigin,
            Func<InvokeResult<Point2D<double>>> resolveFirstTapeHole,
            Func<double> getTapePitch,
            Func<int, InvokeResult<Point2D<double>>> resolvePocket,
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
            _resolvePocket = resolvePocket ?? throw new ArgumentNullException(nameof(resolvePocket));
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
                    case "verify-current-part":
                        return await AdvancePocketAsync(false, cancellationToken);
                    case "verify-previous-empty":
                        return await AdvancePocketAsync(true, cancellationToken);
                    default:
                        return Result(false, "Unknown guided calibration step.");
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
            if (Current.ActiveStepKey == "verify-current-part" ||
                Current.ActiveStepKey == "verify-previous-empty")
            {
                _approachedPocketStep = null;
                _pocketTarget = null;
            }
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

        private async Task<MachineOperationActionResult> AdvanceTapeHoleAsync(
            bool lastHole, CancellationToken token)
        {
            var stepKey = lastHole ? "last-tape-hole" : "first-tape-hole";
            if (!ReadyForAction(false, false, out var reason))
                return Block(reason);
            if (_calculatedOrigin == null)
                return Block("Feeder origin must be established first.");

            // Two operator actions per hole: move to approximate location,
            // then use the established camera Jog and explicitly verify/capture.
            // No hidden auto-jog or speculative motion loop.
            return _approachedHoleStep == stepKey
                ? await CaptureTapeHoleAsync(lastHole, token)
                : await MoveToTapeHoleAsync(lastHole, token);
        }

        private async Task<MachineOperationActionResult> MoveToTapeHoleAsync(
            bool lastHole, CancellationToken token)
        {
            var stepKey = lastHole ? "last-tape-hole" : "first-tape-hole";
            var capture = _machine.PositionImageCaptureService;
            Point2D<double> target;
            if (lastHole)
            {
                if (_firstHoleMachinePosition == null || _firstHoleRowOffset == null)
                    return Block("First tape-hole measurement is missing.");
                if (!FeederTapeHoleCalibrationMath.TryApproximateLastHole(
                    _firstHoleMachinePosition,
                    _orientation == FeederOrientations.Vertical,
                    _feeder.Length, _getTapePitch(), out target))
                    return Block("Feeder length and tape pitch do not define two usable holes.");

                // Retain the profile used when the first hole was verified.
                _machine.SetVisionProfile(CameraTypes.Position, _tapeProfileKey);
            }
            else
            {
                // Always use the feeder's calculated first-hole geometry.
                // Older persisted row offsets must not bias calibration.
                var first = _resolveFirstTapeHole();
                if (!first.Successful || first.Result == null)
                    return Block(first.ErrorMessage ?? "Could not resolve the calculated first tape hole.");
                target = first.Result;
                if (capture?.Camera == null || capture.Profile == null)
                    return Block("Position camera or tape-hole profile is unavailable.");

                _tapeCameraId = capture.Camera.Id.ToString();
                _tapeCameraScale = capture.Camera.EffectivePixelsPerMM;
                _tapeProfileKey = capture.Profile.Key;
            }

            if (!TapeCameraUnchanged())
                return Block("Tape-hole camera, vision profile or physical scale changed.");

            var area = _machine.Settings.WorkAreaSize;
            if (Double.IsNaN(target.X) || Double.IsNaN(target.Y) ||
                Double.IsInfinity(target.X) || Double.IsInfinity(target.Y) ||
                target.X < 0 || target.Y < 0 || target.X > area.X || target.Y > area.Y)
                return Block("Calculated tape-hole target is outside the machine work area; no move sent.");

            StatusText = $"Moving to {(lastHole ? "last" : "first")} tape hole: X={target.X:F2}, Y={target.Y:F2} mm.";
            Signal();
            token.ThrowIfCancellationRequested();

            _machine.SendSafeMoveHeight();
            var safe = await _machine.SpinUntilIdleAsync(10000);
            if (!safe.Successful)
                return Block("Safe-height move failed: " + safe.ErrorMessage);
            if (!ReadyForAction(false, false, out var reason) || !TapeCameraUnchanged() ||
                Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Tape-hole camera or machine context changed before the move.");

            capture.ResetDetections();
            _machine.GotoPoint(target);
            var moved = await _machine.SpinUntilIdleAsync(30000);
            if (!moved.Successful)
                return Block("Tape-hole approach failed: " + moved.ErrorMessage);
            if (!ReadyForAction(false, false, out reason) || !TapeCameraUnchanged() ||
                Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Tape-hole camera or machine context changed after the move.");

            var actual = await _machine.GetCurrentLocationAsync(5000);
            if (!actual.Successful ||
                !FeederOriginCalibrationMath.SamePosition(actual.Result, target, 0.5))
                return Block("The machine did not confirm reaching the calculated tape-hole target.");

            _approachedHoleStep = stepKey;
            MeasurementText = $"Approached {(lastHole ? "last" : "first")} hole at X {actual.Result.X:F2}, Y {actual.Result.Y:F2} mm";
            StatusText = "Use the existing camera Jog to center the intended complete tape hole, then press Verify. No offset is saved until BOTH holes are captured.";
            Signal();
            return Result(true);
        }

        private async Task<MachineOperationActionResult> CaptureTapeHoleAsync(
            bool lastHole, CancellationToken token)
        {
            if (!ReadyForAction(false, lastHole, out var reason) || !TapeCameraUnchanged())
                return Block(reason ?? "Tape-hole camera/profile/scale changed.");

            StatusText = "Acquiring a fresh stable, centered tape hole at the current camera position.";
            Signal();
            var circle = await AcquireFreshCircleAsync(token);
            if (circle == null)
                return Block("No stable centered tape hole was found. Jog and retry this step.");
            if (!ReadyForAction(false, lastHole, out reason) || !TapeCameraUnchanged() ||
                Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Machine context changed during tape-hole verification.");

            var actual = await _machine.GetCurrentLocationAsync(5000);
            if (!actual.Successful ||
                !FeederOriginCalibrationMath.SamePosition(actual.Result, circle.MachinePosition, 0.1))
                return Block("Machine XY changed after centered tape-hole observation.");

            var calibratedOrigin = _calculatedOrigin + _feeder.OriginOffset;
            var vertical = _orientation == FeederOrientations.Vertical;
            if (!lastHole && !FeederTapeHoleCalibrationMath.CompletelyBeyondLeadingEdge(
                    calibratedOrigin, actual.Result, circle.RadiusMm, vertical))
                return Block("This hole is not fully beyond the feeder leading edge. Choose the first COMPLETE hole and retry.");

            var rowOffset = FeederTapeHoleCalibrationMath.ToRowOffset(
                calibratedOrigin, actual.Result, vertical);
            if (!lastHole)
            {
                _firstHoleMachinePosition = actual.Result;
                _firstHoleRowOffset = rowOffset;
                var observedAt = DateTimeOffset.UtcNow;
                _session.RecordEvidence(new MachineOperationEvidence
                {
                    RunId = Current.RunId,
                    StepKey = "first-tape-hole",
                    Name = "Measured first complete tape hole",
                    Value = $"X {actual.Result.X:F2}, Y {actual.Result.Y:F2}; row offset X {rowOffset.X:F3}, Y {rowOffset.Y:F3}",
                    Unit = "mm",
                    Source = "Position camera / centered tape circle",
                    Verification = MachineOperationVerificationKind.VisionVerified,
                    Saved = false,
                    CapturedAtUtc = observedAt,
                    VisionProfileKey = _tapeProfileKey,
                    PixelsPerMm = circle.PixelsPerMm
                });
                _session.CompleteActiveStep("first-tape-hole", observedAt);
                _approachedHoleStep = null;
                MeasurementText = $"MEASURED first hole: row X {rowOffset.X:F3}, Y {rowOffset.Y:F3} mm (not saved yet)";
                StatusText = "First complete tape hole measured. Next: move to the last hole using the measured first position and tape pitch.";
                Signal();
                return Result(true);
            }

            if (_firstHoleRowOffset == null || rowOffset.X <= _firstHoleRowOffset.X)
                return Block("Last tape hole must be farther along the tape than the first hole.");

            var previousFirst = _row.FirstTapeHoleOffset;
            var previousLast = _row.LastTapeHoleOffset;
            _row.FirstTapeHoleOffset = _firstHoleRowOffset;
            _row.LastTapeHoleOffset = rowOffset;
            InvokeResult saved;
            try
            {
                saved = await _saveFeeder(_feeder);
            }
            catch
            {
                _row.FirstTapeHoleOffset = previousFirst;
                _row.LastTapeHoleOffset = previousLast;
                throw;
            }

            if (!saved.Successful)
            {
                _row.FirstTapeHoleOffset = previousFirst;
                _row.LastTapeHoleOffset = previousLast;
                return Block("Could not save the paired tape-hole offsets; previous offsets restored locally. " + saved.ErrorMessage);
            }
            if (Current.Status != MachineOperationRunStatus.Running)
                return Result(false, "Tape-hole save finished after cancellation. Reload the feeder before continuing.");

            var now = DateTimeOffset.UtcNow;
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = Current.RunId,
                StepKey = "last-tape-hole",
                Name = "Centered and saved first/last tape holes",
                Value = $"first X {_firstHoleRowOffset.X:F3}, Y {_firstHoleRowOffset.Y:F3}; last X {rowOffset.X:F3}, Y {rowOffset.Y:F3}",
                Unit = "mm",
                Source = "Position camera / Manufacturing feeder PUT",
                Verification = MachineOperationVerificationKind.VisionVerified,
                Saved = true,
                CapturedAtUtc = now,
                VisionProfileKey = _tapeProfileKey,
                PixelsPerMm = circle.PixelsPerMm
            });
            _session.CompleteActiveStep("last-tape-hole", now);
            _approachedHoleStep = null;
            MeasurementText = $"SAVED tape holes: first X {_firstHoleRowOffset.X:F3}, Y {_firstHoleRowOffset.Y:F3}; last X {rowOffset.X:F3}, Y {rowOffset.Y:F3}";
            StatusText = "First and last tape-hole offsets saved. Next: inspect the current pocket. No picking.";
            Signal();
            return Result(true);
        }


        // Camera moves and visual confirmation are separate operator actions.
        // Neither action picks a component or updates inventory/CurrentPartIndex.
        private async Task<MachineOperationActionResult> AdvancePocketAsync(
            bool previous, CancellationToken token)
        {
            if (!ReadyForAction(false, false, out var reason))
                return Block(reason);
            if (_row.CurrentPartIndex != (Current.Target.StartingPartIndex ?? 0))
                return Block("Feeder row current-part index changed. Restart calibration.");
            if (!FeederPocketVerificationMath.TryGetInspectionIndex(
                    (Current.Target.StartingPartIndex ?? 0), _row.PartCapacity, previous, out var index))
                return Block(previous
                    ? "Part 1 has no preceding pocket; no move sent."
                    : "Current part index is outside feeder capacity.");

            var key = previous ? "verify-previous-empty" : "verify-current-part";
            return _approachedPocketStep == key
                ? await ConfirmPocketAsync(previous, token)
                : await MoveToPocketAsync(previous, index, token);
        }

        private async Task<MachineOperationActionResult> MoveToPocketAsync(
            bool previous, int index, CancellationToken token)
        {
            var key = previous ? "verify-previous-empty" : "verify-current-part";
            var target = _resolvePocket(index);
            if (!target.Successful || target.Result == null)
                return Block(target.ErrorMessage ?? "Could not resolve pocket position.");
            var point = target.Result;
            var capture = _machine.PositionImageCaptureService;
            if (capture?.Camera == null || capture.Profile == null)
                return Block("Position camera or part-in-tape profile unavailable.");
            _pocketCameraId = capture.Camera.Id.ToString();
            _pocketCameraScale = capture.Camera.EffectivePixelsPerMM;
            _pocketProfileKey = capture.Profile.Key;
            if (_pocketCameraScale <= 0 || Double.IsNaN(_pocketCameraScale) ||
                Double.IsInfinity(_pocketCameraScale) || !PocketCameraUnchanged())
                return Block("Invalid position camera scale/profile.");

            var workArea = _machine.Settings.WorkAreaSize;
            if (Double.IsNaN(point.X) || Double.IsNaN(point.Y) ||
                Double.IsInfinity(point.X) || Double.IsInfinity(point.Y) ||
                point.X < 0 || point.Y < 0 ||
                point.X > workArea.X || point.Y > workArea.Y)
                return Block("Pocket target is outside machine work area; no move sent.");

            StatusText = $"Moving CAMERA to pocket {index}: X={point.X:F2}, Y={point.Y:F2} mm; no pick.";
            Signal();
            token.ThrowIfCancellationRequested();
            _machine.SendSafeMoveHeight();
            var safe = await _machine.SpinUntilIdleAsync(10000);
            if (!safe.Successful)
                return Block("Safe-height move failed: " + safe.ErrorMessage);
            if (!PocketContextReady(out var reason) ||
                Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Machine or camera changed before pocket move.");

            capture.ResetDetections();
            _machine.GotoPoint(point);
            var moved = await _machine.SpinUntilIdleAsync(30000);
            if (!moved.Successful)
                return Block("Pocket camera move failed: " + moved.ErrorMessage);
            if (!PocketContextReady(out reason) ||
                Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Machine or camera changed after pocket move.");

            var actual = await _machine.GetCurrentLocationAsync(5000);
            if (!actual.Successful ||
                !FeederOriginCalibrationMath.SamePosition(actual.Result, point, 0.5))
                return Block("Machine did not confirm pocket arrival.");

            _pocketTarget = new Point2D<double>(actual.Result.X, actual.Result.Y);
            _pocketIndex = index;
            _approachedPocketStep = key;
            var now = DateTimeOffset.UtcNow;
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = Current.RunId,
                StepKey = key,
                Name = "Camera position at pocket",
                Value = $"index {index}; X {actual.Result.X:F3}, Y {actual.Result.Y:F3}",
                Unit = "mm",
                Source = "Machine position readback",
                Verification = MachineOperationVerificationKind.MachineObserved,
                CapturedAtUtc = now,
                VisionProfileKey = _pocketProfileKey,
                PixelsPerMm = _pocketCameraScale
            });
            MeasurementText = $"Pocket {index}: X {actual.Result.X:F3}, Y {actual.Result.Y:F3} mm";
            StatusText = previous
                ? "Inspect camera. Confirm EMPTY only if the preceding pocket has NO part; otherwise cancel."
                : "Inspect camera. Confirm PRESENT only if the current pocket contains a part; otherwise cancel.";
            Signal();
            return Result(true);
        }

        private async Task<MachineOperationActionResult> ConfirmPocketAsync(
            bool previous, CancellationToken token)
        {
            if (!PocketContextReady(out var reason) || _pocketTarget == null)
                return Block(reason ?? "Pocket camera observation is stale; repeat approach.");
            token.ThrowIfCancellationRequested();
            var actual = await _machine.GetCurrentLocationAsync(5000);
            if (!actual.Successful ||
                !FeederOriginCalibrationMath.SamePosition(actual.Result, _pocketTarget, 0.1) ||
                !PocketContextReady(out reason) ||
                Current.Status != MachineOperationRunStatus.Running)
                return Block(reason ?? "Machine moved since pocket approach; retry.");

            var key = previous ? "verify-previous-empty" : "verify-current-part";
            var now = DateTimeOffset.UtcNow;
            _session.RecordEvidence(new MachineOperationEvidence
            {
                RunId = Current.RunId,
                StepKey = key,
                Name = previous ? "Operator confirmed EMPTY" : "Operator confirmed PRESENT",
                Value = $"index {_pocketIndex}; X {actual.Result.X:F3}, Y {actual.Result.Y:F3}; " +
                        (previous ? "EMPTY" : "PRESENT"),
                Unit = "mm",
                Source = "Operator visual confirmation in position camera",
                Verification = MachineOperationVerificationKind.OperatorConfirmed,
                CapturedAtUtc = now,
                VisionProfileKey = _pocketProfileKey,
                PixelsPerMm = _pocketCameraScale
            });
            _session.CompleteActiveStep(key, now);
            _approachedPocketStep = null;
            _pocketTarget = null;
            MeasurementText = $"Confirmed {(previous ? "EMPTY" : "PRESENT")}: pocket {_pocketIndex} at " +
                $"X {actual.Result.X:F3}, Y {actual.Result.Y:F3} mm";
            StatusText = previous
                ? "All seven steps passed. No picking or inventory/index changes."
                : "Current part confirmed present. Next: inspect preceding pocket and confirm EMPTY.";
            Signal();
            return Result(true);
        }

        private bool PocketContextReady(out string reason)
        {
            if (!ReadyForAction(false, false, out reason))
                return false;
            if (_row.CurrentPartIndex != (Current.Target.StartingPartIndex ?? 0))
                reason = "Feeder row current-part index changed during pocket check.";
            else if (!PocketCameraUnchanged())
                reason = "Position camera or vision profile changed during pocket check.";
            return reason == null;
        }

        private bool PocketCameraUnchanged()
        {
            var capture = _machine.PositionImageCaptureService;
            return FeederOriginCalibrationMath.CameraProfileUnchanged(
                _pocketCameraId, _pocketCameraScale,
                capture?.Camera?.Id.ToString(),
                capture?.Camera?.EffectivePixelsPerMM ?? 0,
                capture?.Profile?.Key, _pocketProfileKey);
        }

        private bool TapeCameraUnchanged()
        {
            var capture = _machine.PositionImageCaptureService;
            return FeederOriginCalibrationMath.CameraProfileUnchanged(
                _tapeCameraId, _tapeCameraScale,
                capture?.Camera?.Id.ToString(),
                capture?.Camera?.EffectivePixelsPerMM ?? 0,
                capture?.Profile?.Key, _tapeProfileKey);
        }

        private async Task<CircleProof> AcquireFreshCircleAsync(CancellationToken token)
        {
            TaskCompletionSource<CircleProof> request;
            lock (_observationGate)
            {
                request = new TaskCompletionSource<CircleProof>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _pendingCircle = request;
            }
            try
            {
                _machine.PositionImageCaptureService.ResetDetections();
                _locator.RegisterCircleLocatedHandler(this, 7500);
                _circleRegistered = true;

                var timeout = await Task.WhenAny(request.Task, Task.Delay(8000, token));
                if (timeout != request.Task)
                    return null;
                return await request.Task;
            }
            finally
            {
                ReleaseCircleObservation();
            }
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            TaskCompletionSource<CircleProof> request;
            lock (_observationGate)
                request = _pendingCircle;

            if (request == null || _disposed || circle == null ||
                circle.CameraType != CameraTypes.Position ||
                !circle.Stabilized || !circle.PositionStable || !circle.Centered ||
                circle.RadiusMM <= 0 || _machine.Busy || !_machine.Connected ||
                _machine.Mode != OperatingMode.Manual || !TapeCameraUnchanged())
                return;

            var position = _machine.MachinePosition;
            var offset = circle.OffsetMM;
            request.TrySetResult(new CircleProof
            {
                MachinePosition = new Point2D<double>(position.X, position.Y),
                OffsetX = offset.X,
                OffsetY = offset.Y,
                RadiusMm = circle.RadiusMM,
                PixelsPerMm = _tapeCameraScale
            });
        }

        public void CircleLocatorTimeout()
        {
            lock (_observationGate)
                _pendingCircle?.TrySetResult(null);
        }

        public void CircleLocatorAborted()
        {
            lock (_observationGate)
                _pendingCircle?.TrySetResult(null);
        }

        private void ReleaseCircleObservation()
        {
            lock (_observationGate)
            {
                _pendingCircle?.TrySetResult(null);
                _pendingCircle = null;
            }
            if (_circleRegistered)
            {
                _circleRegistered = false;
                _locator.UnregisterCircleLocatedHandler(this);
            }
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
