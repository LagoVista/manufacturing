using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// T010 independently calibrates one explicitly selected staging plate.
    /// Uses production staging grid geometry, real Hough circle callbacks and
    /// the existing motion/persistence APIs. NEVER resets origin or auto-tunes.
    /// </summary>
    public sealed partial class StagingPlateCalibrationViewModel : MachineViewModelBase
    {
        private const int ObservationTimeoutMs = 12000;
        private const int MaximumCenteringMoves = 6;
        private readonly ILocatorViewModel _locator;
        private readonly Stopwatch _clock = new Stopwatch();
        private SynchronizationContext _uiContext;
        private IImageCaptureService _capture;
        private MachineCamera _camera;
        private VisionProfile _profile;
        private double _scale;
        private DateTimeOffset _moveCompletedUtc;
        private TaskCompletionSource<FiducialCircleObservation> _pending;
        private CircleWaitHandler _handler;
        private TimeSpan _stageStarted;
        private int _currentStage = -1;
        private bool _running;
        private string _status = "Select a plate, then run the two-hole calibration.";
        private string _lastRejection = "No detection yet.";
        private string _lastPlannedMove = "No target calculated yet.";
        private FiducialCircleObservation _currentObservation;
        private MachineStagingPlate _selectedPlate;
        private Point2D<double> _measuredFirst;
        private Point2D<double> _measuredSecond;
        private int _correctionMoves;

        public StagingPlateCalibrationViewModel(IMachineRepo repo,
            ILocatorViewModel locator, MachineStagingPlate selected = null)
            : base(repo)
        {
            _locator = locator ?? throw new ArgumentNullException(nameof(locator));
            _selectedPlate = selected ??
                MachineConfiguration?.StagingPlates?.FirstOrDefault();
            ResetStages();
            LoadVisionSettings();
        }

        public ObservableCollection<StagingPlateCalibrationStep> Steps { get; } =
            new ObservableCollection<StagingPlateCalibrationStep>();
        public ObservableCollection<string> EventLog { get; } =
            new ObservableCollection<string>();

        public MachineStagingPlate SelectedPlate
        {
            get => _selectedPlate;
            set
            {
                if (Running) return;
                Set(ref _selectedPlate, value);
                ClearPreviousMeasurement();
                RaisePropertyChanged(nameof(CanRun));
                RaisePropertyChanged(nameof(SelectedPlateSummary));
            }
        }

        public bool Running
        {
            get => _running;
            private set
            {
                Set(ref _running, value);
                RaisePropertyChanged(nameof(CanRun));
                RaisePropertyChanged(nameof(CanClose));
                RaisePropertyChanged(nameof(CanEditVision));
            }
        }
        public bool CanRun => !Running && !TuningBusy &&
            !HasVisionPreview && !HasVisionDraftChanges && SelectedPlate != null;
        public bool CanClose => !Running && !TuningBusy;
        public string Status { get => _status; private set => Set(ref _status, value); }
        public string LastRejection { get => _lastRejection; private set => Set(ref _lastRejection, value); }
        public FiducialCircleObservation CurrentObservation
        {
            get => _currentObservation;
            private set => Set(ref _currentObservation, value);
        }
        public string Elapsed => _clock.Elapsed.TotalSeconds.ToString("F1") + " s";
        public string CameraName => _camera?.Name ??
            Machine?.PositionImageCaptureService?.Camera?.Name ?? "Position camera";
        public double PixelsPerMillimeter => _camera?.EffectivePixelsPerMM ??
            Machine?.PositionImageCaptureService?.Camera?.EffectivePixelsPerMM ?? 0;

        public string ConfiguredTravelSummary
        {
            get
            {
                var area = Machine?.Settings?.WorkAreaSize;
                if (area == null || area.X <= 0 || area.Y <= 0)
                    return "Configured XY travel is unavailable.";
                return "Allowed X: 0.." + area.X.ToString("F2") +
                    " mm; Y: 0.." + area.Y.ToString("F2") + " mm.";
            }
        }

        public string CurrentMachinePositionSummary
        {
            get
            {
                if (Machine == null) return "Machine position unavailable.";
                var position = Machine.MachinePosition;
                return "Current machine X=" + position.X.ToString("F3") +
                    ", Y=" + position.Y.ToString("F3") + " mm.";
            }
        }

        public string LastPlannedMove
        {
            get => _lastPlannedMove;
            private set => Set(ref _lastPlannedMove, value);
        }

        public string MeasuredHoleOne =>
            _measuredFirst == null ? "Not measured" :
            "X=" + _measuredFirst.X.ToString("F3") +
            "  Y=" + _measuredFirst.Y.ToString("F3") + " mm";
        public string MeasuredHoleTwo =>
            _measuredSecond == null ? "Not measured" :
            "X=" + _measuredSecond.X.ToString("F3") +
            "  Y=" + _measuredSecond.Y.ToString("F3") + " mm";
        public string SelectedPlateSummary => SelectedPlate == null ? "No plate selected" :
            SelectedPlate.Name + " | " +
            SelectedPlate.ReferenceHoleColumn1?.Id + SelectedPlate.ReferenceHoleRow1?.Id +
            " / " +
            SelectedPlate.ReferenceHoleColumn2?.Id + SelectedPlate.ReferenceHoleRow2?.Id;

        private void ClearPreviousMeasurement()
        {
            _measuredFirst = null;
            _measuredSecond = null;
            CurrentObservation = null;
            RaisePropertyChanged(nameof(MeasuredHoleOne));
            RaisePropertyChanged(nameof(MeasuredHoleTwo));
        }

        private void ResetStages()
        {
            Steps.Clear();
            foreach (var name in new[]
            {
                "Check machine, origin and plate",
                "Move to reference hole 1",
                "Center and measure hole 1",
                "Move to reference hole 2",
                "Center and measure hole 2",
                "Verify two-hole alignment",
                "Save staging plate alignment"
            })
                Steps.Add(new StagingPlateCalibrationStep
                {
                    Name = name, Status = "Pending"
                });
            _currentStage = -1;
        }

        private void Step(int number, string detail)
        {
            FinishCurrentStage("Passed");
            _currentStage = number;
            _stageStarted = _clock.Elapsed;
            Steps[number].Status = "Running";
            Steps[number].Detail = detail;
            Status = detail;
            Log(detail);
            RaisePropertyChanged(nameof(Elapsed));
        }

        private void FinishCurrentStage(string state)
        {
            if (_currentStage < 0) return;
            var step = Steps[_currentStage];
            if (step.Status == "Running")
            {
                step.Status = state;
                step.Duration = (_clock.Elapsed - _stageStarted)
                    .TotalSeconds.ToString("F1") + " s";
            }
        }

        private void Fail(string reason)
        {
            FinishCurrentStage("Failed");
            if (_currentStage >= 0) Steps[_currentStage].Detail = reason;
            Status = reason;
            Log("FAILED: " + reason);
            Machine?.AddStatusMessage(StatusMessageTypes.Warning, reason);
        }

        private void Log(string line)
        {
            var entry = DateTimeOffset.UtcNow.ToString("HH:mm:ss.fff") +
                " UTC  " + line;
            void Append()
            {
                EventLog.Add(entry);
                while (EventLog.Count > 60)
                    EventLog.RemoveAt(0);
            }
            if (_uiContext == null || _uiContext == SynchronizationContext.Current)
                Append();
            else _uiContext.Post(_ => Append(), null);
        }

        private bool MachineReady(out string reason)
        {
            var machine = Machine;
            if (machine == null || !machine.Connected ||
                !machine.WasMachineHomed ||
                !machine.WasMachinOriginCalibrated ||
                machine.Mode != OperatingMode.Manual ||
                machine.IsLocating)
            {
                reason = "Connected, homed, Manual machine with verified machine origin is required.";
                return false;
            }
            if (_capture == null || _camera == null || _profile == null ||
                !ReferenceEquals(machine.PositionImageCaptureService, _capture) ||
                !ReferenceEquals(_capture.Camera, _camera) ||
                _camera.CameraType?.Value != CameraTypes.Position ||
                !ReferenceEquals(_camera.CurrentVisionProfile, _profile) ||
                _capture.Profile?.Key != VisionProfile.VisionProfile_StagingPlateHole ||
                !ReferenceEquals(_capture.Profile, _profile) ||
                !_capture.IsCapturing ||
                _camera.EffectivePixelsPerMM <= 0 ||
                double.IsNaN(_camera.EffectivePixelsPerMM) ||
                Math.Abs(_camera.EffectivePixelsPerMM - _scale) > 0.0001)
            {
                reason = "Live position camera, staging-hole profile or camera pixel scale changed.";
                return false;
            }
            if (SelectedPlate == null ||
                MachineConfiguration?.StagingPlates?.Contains(SelectedPlate) != true)
            {
                reason = "Selected staging plate is no longer configured on this machine.";
                return false;
            }
            if (!StagingPlateReferenceGate.ValidateConfiguration(SelectedPlate, out reason))
                return false;

            reason = null;
            return true;
        }

        private async Task<string> WaitForAcknowledgedIdleAsync(string phase)
        {
            var machine = Machine;
            if (machine == null || !machine.Connected ||
                !machine.WasMachineHomed || !machine.WasMachinOriginCalibrated ||
                machine.Mode != OperatingMode.Manual ||
                machine.IsLocating || _locator.IsLocating)
                return "Machine origin/mode/locator state is not safe for staging motion.";

            Log("Waiting for acknowledged idle before " + phase + ".");
            var firmware = await machine.SpinUntilIdleAsync(10000);
            if (!firmware.Successful)
                return "Firmware idle acknowledgement failed: " + firmware.ErrorMessage;

            var count = 0;
            for (var i = 0; i < 24; i++)
            {
                if (!machine.Connected || !machine.WasMachineHomed ||
                    !machine.WasMachinOriginCalibrated ||
                    machine.Mode != OperatingMode.Manual ||
                    machine.IsLocating || _locator.IsLocating)
                    return "Machine or locator changed while waiting for idle.";
                count = machine.Busy ? 0 : count + 1;
                if (count >= 2) return null;
                await Task.Delay(150);
            }
            return "Machine remains Busy after firmware idle acknowledgment. No new XY motion issued.";
        }

        public async Task RunAsync()
        {
            if (!CanRun)
            {
                if (HasVisionPreview || HasVisionDraftChanges)
                    TuningStatus = "Save or revert the staging vision profile changes before automatic plate calibration.";
                return;
            }
            _uiContext = SynchronizationContext.Current;
            Running = true;
            ResetStages();
            EventLog.Clear();
            ClearPreviousMeasurement();
            _clock.Restart();
            LastPlannedMove = "No target calculated yet.";
            _capture = null;
            _camera = null;
            _profile = null;
            _pending = null;
            var selected = SelectedPlate;
            try
            {
                Step(0, "Checking selected staging plate, current machine origin and position camera.");
                if (!StagingPlateReferenceGate.ValidateConfiguration(selected, out var error))
                {
                    Fail(error);
                    return;
                }
                var machine = Machine;
                if (machine == null || !machine.Connected ||
                    !machine.WasMachineHomed || !machine.WasMachinOriginCalibrated ||
                    machine.Mode != OperatingMode.Manual || machine.IsLocating ||
                    _locator.IsLocating || MachineConfiguration?.StagingPlates?.Contains(selected) != true)
                {
                    Fail("Verify machine origin and select a configured plate on a homed, connected Manual machine.");
                    return;
                }

                _capture = machine.PositionImageCaptureService;
                _camera = _capture?.Camera;
                if (_camera?.CameraType?.Value != CameraTypes.Position)
                {
                    Fail("A live position camera is required.");
                    return;
                }

                var initialIdle = await WaitForAcknowledgedIdleAsync("staging preflight");
                if (initialIdle != null)
                {
                    Fail(initialIdle);
                    return;
                }
                machine.SetVisionProfile(CameraTypes.Position,
                    VisionProfile.VisionProfile_StagingPlateHole);
                _profile = _camera.CurrentVisionProfile;
                if (_profile?.Key != VisionProfile.VisionProfile_StagingPlateHole ||
                    !_profile.PerformShapeDetection || !_profile.FindCircles)
                {
                    Fail("Staging plate hole vision profile must use active circle detection.");
                    return;
                }
                _scale = _camera.EffectivePixelsPerMM;
                RaisePropertyChanged(nameof(CameraName));
                RaisePropertyChanged(nameof(PixelsPerMillimeter));

                var captureReady = await _capture.EnsureCaptureReadyAsync();
                if (!captureReady.Successful || !MachineReady(out error))
                {
                    Fail(!captureReady.Successful
                        ? "Live camera unavailable: " + captureReady.ErrorMessage
                        : error);
                    return;
                }

                var firstTarget = StagingPlateUtils.ResolveStagePlateApproximateWorkSpaceLocation(
                    selected, MachineConfiguration.MachineFiducial,
                    selected.FiducialColumn.Id, selected.FiducialRow.Id,
                    selected.ReferenceHoleColumn1.Id, selected.ReferenceHoleRow1.Id);

                Step(1, "Moving safely to " +
                    selected.ReferenceHoleColumn1.Id + selected.ReferenceHoleRow1.Id + ".");
                if (!await MoveToReferenceAsync(firstTarget, "Reference hole 1")) return;
                Step(2, "Centering the first hole using fresh stable top-camera circles.");
                _measuredFirst = await CenterAndMeasureAsync("reference hole 1");
                if (_measuredFirst == null) return;
                RaisePropertyChanged(nameof(MeasuredHoleOne));

                if (!MachineReady(out error))
                {
                    Fail(error);
                    return;
                }
                var secondTarget = StagingPlateUtils.ResolveStagePlateApproximateWorkSpaceLocation(
                    selected, _measuredFirst,
                    selected.ReferenceHoleColumn1.Id, selected.ReferenceHoleRow1.Id,
                    selected.ReferenceHoleColumn2.Id, selected.ReferenceHoleRow2.Id);

                var referenceGridDelta = new Point2D<double>(
                    secondTarget.X - _measuredFirst.X,
                    secondTarget.Y - _measuredFirst.Y);
                Log("Hole 2 grid displacement from measured hole 1: X=" +
                    referenceGridDelta.X.ToString("+0.000;-0.000;0.000") +
                    ", Y=" + referenceGridDelta.Y.ToString("+0.000;-0.000;0.000") +
                    " mm (spacing " + selected.HoleSpacing.ToString("F3") +
                    " mm).");
                Step(3, "Moving from measured first hole to nominal second reference.");
                if (!await MoveToReferenceAsync(secondTarget, "Reference hole 2")) return;
                Step(4, "Centering the second hole using fresh stable top-camera circles.");
                _measuredSecond = await CenterAndMeasureAsync("reference hole 2");
                if (_measuredSecond == null) return;
                RaisePropertyChanged(nameof(MeasuredHoleTwo));

                Step(5, "Checking physical span and orientation against the configured plate.");
                if (!MachineReady(out error) ||
                    !StagingPlateReferenceGate.ValidateTwoHoleMeasurements(
                        selected, _measuredFirst, _measuredSecond, out error))
                {
                    Fail(error ?? "Live camera or machine context changed.");
                    return;
                }

                Step(6, "Saving both measured reference positions as one verified alignment.");
                if (!MachineReady(out error) || machine.Busy)
                {
                    Fail(error ?? "Machine is no longer idle or verified before plate save.");
                    return;
                }
                // Last-known-good alignment survives every failure until BOTH
                // real measurements and geometry pass. No G92 or other steps.
                var previousFirst = selected.ReferenceHoleLocation1;
                var previousSecond = selected.ReferenceHoleLocation2;
                var saved = false;
                try
                {
                    selected.ReferenceHoleLocation1 = _measuredFirst;
                    selected.ReferenceHoleLocation2 = _measuredSecond;
                    var result = await MachineRepo.SaveCurrentMachineAsync();
                    if (!result.Successful)
                    {
                        Fail("Could not save staging plate: " + result.ErrorMessage);
                        return;
                    }
                    saved = true;
                    selected.IsCalibrated = true;
                    FinishCurrentStage("Passed");
                    Steps[6].Detail = "Both reference coordinates persisted; calibrated this session.";
                    Status = selected.Name + " aligned and saved. No other calibration started.";
                    Log(Status);
                    machine.AddStatusMessage(StatusMessageTypes.Info, Status);
                }
                finally
                {
                    if (!saved)
                    {
                        selected.ReferenceHoleLocation1 = previousFirst;
                        selected.ReferenceHoleLocation2 = previousSecond;
                    }
                }
            }
            catch (Exception ex)
            {
                Fail("Staging plate calibration stopped: " + ex.Message);
            }
            finally
            {
                StopListening();
                _pending = null;
                _clock.Stop();
                RaisePropertyChanged(nameof(Elapsed));
                Running = false;
            }
        }

        private async Task<bool> MoveToReferenceAsync(
            Point2D<double> target, string referenceName)
        {
            var positionText = Machine == null || target == null
                ? "current position unavailable"
                : "current X=" + Machine.MachinePosition.X.ToString("F3") +
                  ", Y=" + Machine.MachinePosition.Y.ToString("F3") +
                  "; delta X=" + (target.X - Machine.MachinePosition.X)
                      .ToString("+0.000;-0.000;0.000") +
                  ", Y=" + (target.Y - Machine.MachinePosition.Y)
                      .ToString("+0.000;-0.000;0.000");
            var targetDescription = StagingPlateReferenceGate.DescribeTravelViolation(
                target, Machine?.Settings?.WorkAreaSize, referenceName);
            LastPlannedMove = targetDescription + " (" + positionText + " mm).";
            RaisePropertyChanged(nameof(CurrentMachinePositionSummary));
            Log(LastPlannedMove);

            if (!StagingPlateReferenceGate.WithinWorkArea(
                target, Machine?.Settings?.WorkAreaSize))
            {
                Fail(LastPlannedMove);
                return false;
            }

            var idle = await WaitForAcknowledgedIdleAsync("safe move");
            if (idle != null)
            {
                Fail(idle);
                return false;
            }
            if (!MachineReady(out var reason) || Machine.Busy)
            {
                Fail(reason ?? "Machine is still Busy.");
                return false;
            }

            Machine.SendSafeMoveHeight();
            var safe = await Machine.SpinUntilIdleAsync(12000);
            if (!safe.Successful || !Machine.Connected ||
                !Machine.WasMachinOriginCalibrated)
            {
                Fail("Safe-Z move was not acknowledged; XY move blocked.");
                return false;
            }
            await Machine.MoveToCameraAsync();
            var cameraIdle = await WaitForAcknowledgedIdleAsync("after camera navigation");
            if (cameraIdle != null || !MachineReady(out reason))
            {
                Fail(cameraIdle ?? reason);
                return false;
            }
            Machine.GotoPoint(target);
            var moved = await Machine.SpinUntilIdleAsync(30000);
            if (!moved.Successful)
            {
                Fail("Reference motion did not finish: " + moved.ErrorMessage);
                return false;
            }
            var settled = await WaitForAcknowledgedIdleAsync("after reference move");
            if (settled != null || !MachineReady(out reason))
            {
                Fail(settled ?? reason);
                return false;
            }

            _moveCompletedUtc = DateTimeOffset.UtcNow;
            _capture.ResetDetections();
            CurrentObservation = null;
            Log("Reached reference target X=" + target.X.ToString("F3") +
                " Y=" + target.Y.ToString("F3") + " mm.");
            return true;
        }

        private async Task<Point2D<double>> CenterAndMeasureAsync(string label)
        {
            for (var n = 0; n <= MaximumCenteringMoves; n++)
            {
                if (!MachineReady(out var reason) || Machine.Busy)
                {
                    Fail(reason ?? "Machine moved or became Busy during detection.");
                    return null;
                }

                var circle = await WaitForFreshStableCircleAsync();
                if (circle == null)
                {
                    Fail("No stable circle at " + label + ": " + LastRejection);
                    return null;
                }
                if (!FiducialCalibrationGate.TryQualify(
                    circle, _profile.Key, _scale, _moveCompletedUtc,
                    DateTimeOffset.UtcNow, out reason))
                {
                    Fail("Stale or invalid circle: " + reason);
                    return null;
                }

                if (FiducialCalibrationGate.CanCommitOrigin(
                    circle, _profile.Key, _scale, _moveCompletedUtc,
                    DateTimeOffset.UtcNow, out reason))
                {
                    if (!MachineReady(out reason) || Machine.Busy)
                    {
                        Fail(reason ?? "Machine changed while measuring position.");
                        return null;
                    }
                    var location = await Machine.GetCurrentLocationAsync(5000);
                    if (!location.Successful ||
                        !StagingPlateReferenceGate.WithinWorkArea(
                            location.Result, Machine.Settings.WorkAreaSize) ||
                        !MachineReady(out reason) || Machine.Busy)
                    {
                        Fail("Could not verify measured position at " + label + ".");
                        return null;
                    }

                    Log(label + " centered at X=" + location.Result.X.ToString("F3") +
                        " Y=" + location.Result.Y.ToString("F3") + " mm.");
                    return location.Result;
                }

                if (n == MaximumCenteringMoves)
                {
                    Fail("Unable to center " + label + " after " +
                         MaximumCenteringMoves + " corrections.");
                    return null;
                }

                var dx = circle.OffsetXmm;
                var dy = circle.OffsetYmm;
                if (double.IsNaN(dx) || double.IsNaN(dy) ||
                    double.IsInfinity(dx) || double.IsInfinity(dy) ||
                    Math.Abs(dx) > 3.0 || Math.Abs(dy) > 3.0)
                {
                    Fail("Circle correction exceeds supervised 3 mm per-axis limit.");
                    return null;
                }
                var correction = new Point2D<double>(dx, dy);
                var current = Machine.MachinePosition;
                var destination = new Point2D<double>(
                    current.X + dx, current.Y + dy);
                if (!StagingPlateReferenceGate.WithinWorkArea(
                    destination, Machine.Settings.WorkAreaSize))
                {
                    Fail(StagingPlateReferenceGate.DescribeTravelViolation(
                        destination, Machine.Settings.WorkAreaSize,
                        "Centering correction for " + label));
                    return null;
                }
                Log(label + " correction X=" + dx.ToString("F3") +
                    " Y=" + dy.ToString("F3") + " mm.");
                Machine.GotoPoint(correction, relativeMove: true);
                var repositioned = await Machine.SpinUntilIdleAsync(10000);
                if (!repositioned.Successful)
                {
                    Fail("Centering move not acknowledged: " + repositioned.ErrorMessage);
                    return null;
                }
                var settled = await WaitForAcknowledgedIdleAsync("after centering correction");
                if (settled != null || !MachineReady(out reason))
                {
                    Fail(settled ?? reason);
                    return null;
                }

                _moveCompletedUtc = DateTimeOffset.UtcNow;
                _capture.ResetDetections();
                CurrentObservation = null;
            }
            return null;
        }

        private async Task<FiducialCircleObservation> WaitForFreshStableCircleAsync()
        {
            var completion = new TaskCompletionSource<FiducialCircleObservation>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _capture.ResetDetections();
            _pending = completion;
            var handler = new CircleWaitHandler(this, completion);
            _handler = handler;
            try
            {
                _locator.RegisterCircleLocatedHandler(handler, ObservationTimeoutMs);
                var winner = await Task.WhenAny(completion.Task,
                    Task.Delay(ObservationTimeoutMs + 1000));
                if (winner != completion.Task)
                {
                    LastRejection = "No qualified circle received within bounded detection timeout.";
                    return null;
                }
                return await completion.Task;
            }
            finally
            {
                StopListening();
                if (ReferenceEquals(_pending, completion))
                    _pending = null;
            }
        }

        private void StopListening()
        {
            var handler = _handler;
            if (handler == null) return;
            _handler = null;
            _locator.UnregisterCircleLocatedHandler(handler);
        }

        private sealed class CircleWaitHandler : ICircleLocatedHandler
        {
            private readonly StagingPlateCalibrationViewModel _owner;
            private readonly TaskCompletionSource<FiducialCircleObservation> _completion;
            public CircleWaitHandler(StagingPlateCalibrationViewModel owner,
                TaskCompletionSource<FiducialCircleObservation> completion)
            {
                _owner = owner;
                _completion = completion;
            }
            public void CircleLocated(MVLocatedCircle circle)
            {
                if (ReferenceEquals(_owner._pending, _completion))
                    _owner.ObserveCircle(circle, _completion);
            }
            public void CircleLocatorTimeout() =>
                _owner.EndWait(_completion, "Circle detector timed out.");
            public void CircleLocatorAborted() =>
                _owner.EndWait(_completion, "Circle detector was aborted.");
        }

        private void EndWait(TaskCompletionSource<FiducialCircleObservation> target,
            string reason)
        {
            if (!ReferenceEquals(target, _pending)) return;
            target.TrySetResult(null);
            if (_uiContext == null || _uiContext == SynchronizationContext.Current)
                LastRejection = reason;
            else _uiContext.Post(_ => LastRejection = reason, null);
        }

        private void ObserveCircle(MVLocatedCircle circle,
            TaskCompletionSource<FiducialCircleObservation> request)
        {
            if (!ReferenceEquals(request, _pending) ||
                circle?.CameraType != CameraTypes.Position ||
                _capture == null || _profile == null || _camera == null)
                return;

            var center = circle.CenterPixels;
            var offset = circle.OffsetMM;
            var px = circle.OffsetPixels;
            var now = DateTimeOffset.UtcNow;
            var snapshot = new FiducialCircleObservation
            {
                TrackId = circle.Id,
                CameraType = circle.CameraType,
                ObservedAtUtc = now,
                FrameAtUtc = _capture.LastProcessedFrameUtc,
                ProfileKey = _capture.Profile?.Key,
                PixelsPerMillimeter = circle.PixelsPerMM,
                FrameWidth = _capture.CaptureFrameSize.Width,
                FrameHeight = _capture.CaptureFrameSize.Height,
                FoundCount = circle.FoundCount,
                Stabilized = circle.Stabilized,
                PositionStable = circle.PositionStable,
                Centered = circle.Centered,
                CenterXpx = center.X,
                CenterYpx = center.Y,
                RadiusPx = circle.RadiusPixels,
                RadiusMm = circle.RadiusMM,
                OffsetXpx = px.X,
                OffsetYpx = px.Y,
                OffsetXmm = offset.X,
                OffsetYmm = offset.Y,
                PositionSpreadMm = circle.PositionSpreadMM,
                SearchRadiusPx = _profile.TargetImageRadius * _scale,
                ErrorToleranceMm = _profile.ErrorToleranceMM
            };
            var qualified = !Machine.Busy &&
                FiducialCalibrationGate.TryQualify(snapshot, _profile.Key,
                    _scale, _moveCompletedUtc, now, out var rejection);
            var reason = qualified ? null : "Machine busy or invalid circle observation.";
            if (!qualified)
            {
                FiducialCalibrationGate.TryQualify(snapshot, _profile.Key,
                    _scale, _moveCompletedUtc, now, out reason);
            }
            snapshot.Qualified = qualified;
            snapshot.RejectionReason = reason;
            void Publish()
            {
                CurrentObservation = snapshot;
                if (!qualified) LastRejection = reason;
            }
            if (_uiContext == null || _uiContext == SynchronizationContext.Current)
                Publish();
            else _uiContext.Post(_ => Publish(), null);
            if (qualified) request.TrySetResult(snapshot);
        }
    }
}
