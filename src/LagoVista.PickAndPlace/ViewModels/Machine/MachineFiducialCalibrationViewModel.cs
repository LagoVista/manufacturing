using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// Independent machine-fiducial calibration. Uses the production macfiducial
    /// Hough detector, machine safe-travel and coordinate-reset commands.
    /// Never invokes exposure auto-tune or staging plate calibration.
    /// </summary>
    public sealed partial class MachineFiducialCalibrationViewModel : MachineViewModelBase
    {
        private const int DetectionTimeoutMs = 18000;
        private const int MaximumCenteringMoves = 5;
        private readonly ILocatorViewModel _locator;
        private readonly Stopwatch _clock = new Stopwatch();
        private TaskCompletionSource<FiducialCircleObservation> _pending;
        private SynchronizationContext _uiContext;
        private IImageCaptureService _capture;
        private MachineCamera _camera;
        private VisionProfile _profile;
        private double _scale;
        private DateTimeOffset _moveCompletedUtc;
        private TimeSpan _stepBegan;
        private int _activeStep = -1;
        private CircleWaitHandler _registeredHandler;
        private bool _running;
        private string _status = "Ready to locate the machine fiducial.";
        private string _lastRejection = "No detections yet.";
        private FiducialCircleObservation _observation;
        private int _observations;
        private int _stableObservations;
        private int _centeringMoves;

        public MachineFiducialCalibrationViewModel(IMachineRepo repo, ILocatorViewModel locator)
            : base(repo)
        {
            _locator = locator ?? throw new ArgumentNullException(nameof(locator));
            ResetSteps();
            InitializeManualControls();
        }

        public ObservableCollection<FiducialCalibrationStep> Steps { get; } =
            new ObservableCollection<FiducialCalibrationStep>();

        // Bounded, factual timeline for a normal run; collapsed in the UI until
        // troubleshooting is requested. Never generates synthetic confidence.
        public ObservableCollection<string> EventLog { get; } =
            new ObservableCollection<string>();

        private void Log(string message)
        {
            var text = DateTimeOffset.UtcNow.ToString("HH:mm:ss.fff") + " UTC  " + message;
            void Add()
            {
                EventLog.Add(text);
                while (EventLog.Count > 60)
                    EventLog.RemoveAt(0);
            }

            if (_uiContext == null || SynchronizationContext.Current == _uiContext)
                Add();
            else
                _uiContext.Post(_ => Add(), null);
        }

        public string Status { get => _status; private set => Set(ref _status, value); }
        public string LastRejection { get => _lastRejection; private set => Set(ref _lastRejection, value); }
        public FiducialCircleObservation CurrentObservation
        {
            get => _observation;
            private set => Set(ref _observation, value);
        }
        public bool Running
        {
            get => _running;
            private set
            {
                Set(ref _running, value);
                RaisePropertyChanged(nameof(CanRun));
                RaisePropertyChanged(nameof(CanClose));
                RaisePropertyChanged(nameof(CanManualAdjust));
            }
        }
        public bool CanRun => !Running && !ManualBusy && !HasUnsavedTuning;
        public string Elapsed => _clock.Elapsed.TotalSeconds.ToString("F1") + " s";
        public int ObservationCount { get => _observations; private set => Set(ref _observations, value); }
        public int StableObservationCount { get => _stableObservations; private set => Set(ref _stableObservations, value); }
        public int CenteringMoves { get => _centeringMoves; private set => Set(ref _centeringMoves, value); }
        public string CameraName => _camera?.Name ?? Machine?.PositionImageCaptureService?.Camera?.Name ?? "Position camera";
        public string MachinePositionSummary => Machine == null ? "Not connected" :
            $"X={Machine.MachinePosition.X:F2}  Y={Machine.MachinePosition.Y:F2}  Z={Machine.MachinePosition.Z:F2}";
        public double PixelsPerMillimeter => _camera?.EffectivePixelsPerMM ??
            Machine?.PositionImageCaptureService?.Camera?.EffectivePixelsPerMM ?? 0;

        private void ResetSteps()
        {
            Steps.Clear();
            foreach (var name in new[]
            {
                "Check machine and camera",
                "Move to fiducial",
                "Detect stable circle",
                "Center and verify",
                "Set machine origin"
            })
                Steps.Add(new FiducialCalibrationStep { Name = name, Status = "Pending" });
            _activeStep = -1;
        }

        private void Step(int number, string detail)
        {
            if (_activeStep >= 0)
                FinishCurrentStep("Passed");
            _activeStep = number;
            _stepBegan = _clock.Elapsed;
            Steps[number].Status = "Running";
            Steps[number].Detail = detail;
            Status = detail;
            Log(detail);
            RaisePropertyChanged(nameof(Elapsed));
        }

        private void FinishCurrentStep(string state)
        {
            if (_activeStep < 0) return;
            var step = Steps[_activeStep];
            if (step.Status == "Running")
            {
                step.Status = state;
                step.Duration = (_clock.Elapsed - _stepBegan).TotalSeconds.ToString("F1") + " s";
            }
        }

        private void Fail(string reason)
        {
            FinishCurrentStep("Failed");
            if (_activeStep >= 0) Steps[_activeStep].Detail = reason;
            Status = reason;
            Log("FAILED: " + reason);
            Machine?.AddStatusMessage(StatusMessageTypes.Warning, reason);
            RaisePropertyChanged(nameof(Elapsed));
        }

        private bool ContextValid(bool requireIdle, out string reason)
        {
            var machine = Machine;
            if (machine == null || !machine.Connected || !machine.WasMachineHomed ||
                machine.Mode != OperatingMode.Manual)
            {
                reason = "Machine must be connected, homed and in Manual mode.";
                return false;
            }
            if (requireIdle && machine.Busy)
            {
                reason = "Machine has outstanding motion.";
                return false;
            }
            if (_capture == null || _camera == null || _profile == null ||
                !ReferenceEquals(machine.PositionImageCaptureService, _capture) ||
                !ReferenceEquals(_capture.Camera, _camera) ||
                !ReferenceEquals(_camera.CurrentVisionProfile, _profile) ||
                _capture.Profile?.Key != VisionProfile.VisionProfile_MachineFiducual)
            {
                reason = "Position camera or machine-fiducial profile changed.";
                return false;
            }
            if (Math.Abs(_camera.EffectivePixelsPerMM - _scale) > 0.0001 ||
                _scale <= 0 || !_capture.IsCapturing)
            {
                reason = "Pixel scale or live camera freshness changed.";
                return false;
            }
            reason = null;
            return true;
        }

        public async Task RunAsync()
        {
            if (Running || ManualBusy) return;
            if (HasUnsavedTuning)
            {
                Status = "Save or revert preview Hough tuning before automatic calibration.";
                return;
            }

            _uiContext = SynchronizationContext.Current;
            Running = true;
            EventLog.Clear();
            ResetSteps();
            _clock.Restart();
            _pending = null;
            _capture = null;
            _camera = null;
            _profile = null;
            CurrentObservation = null;
            ObservationCount = 0;
            StableObservationCount = 0;
            CenteringMoves = 0;
            LastRejection = "No detections yet.";
            bool resetRequested = false;
            bool originCommitted = false;
            try
            {
                Step(0, "Checking machine homing, manual mode and live camera.");
                var machine = Machine;
                if (machine == null || !machine.Connected || !machine.WasMachineHomed ||
                    machine.Mode != OperatingMode.Manual || machine.Busy || machine.IsLocating)
                {
                    Fail("Connect and home an idle machine in Manual mode; finish other locator activity first.");
                    return;
                }
                if (MachineConfiguration?.MachineFiducial == null ||
                    MachineConfiguration.MachineFiducial.IsOrigin())
                {
                    Fail("Configure the Machine Fiducial position before this procedure.");
                    return;
                }
                _capture = machine.PositionImageCaptureService;
                _camera = _capture?.Camera;
                if (_camera?.CameraType?.Value != CameraTypes.Position)
                {
                    Fail("The live position camera is not selected or available.");
                    return;
                }
                var readiness = await _capture.EnsureCaptureReadyAsync();
                if (!readiness.Successful)
                {
                    Fail("No fresh position-camera frames: " + readiness.ErrorMessage);
                    return;
                }
                RaisePropertyChanged(nameof(CameraName));

                Step(1, "Moving position camera safely over configured machine fiducial.");
                machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_MachineFiducual);
                _profile = _camera.CurrentVisionProfile;
                if (_profile?.Key != VisionProfile.VisionProfile_MachineFiducual ||
                    !_profile.PerformShapeDetection || !_profile.FindCircles)
                {
                    Fail("The macfiducial profile must enable production circle detection.");
                    return;
                }
                _scale = _camera.EffectivePixelsPerMM;
                RaisePropertyChanged(nameof(PixelsPerMillimeter));
                if (_scale <= 0 || double.IsNaN(_scale) || double.IsInfinity(_scale))
                {
                    Fail("Position camera has no valid pixels-per-millimeter calibration.");
                    return;
                }
                if (!ContextValid(true, out var error))
                {
                    Fail(error);
                    return;
                }
                machine.SendSafeMoveHeight();
                await machine.MoveToCameraAsync();
                machine.GotoPoint(MachineConfiguration.MachineFiducial);
                var positioning = await machine.SpinUntilIdleAsync(10000);
                if (!positioning.Successful)
                {
                    Fail("Could not reach fiducial: " + positioning.ErrorMessage);
                    return;
                }
                _moveCompletedUtc = DateTimeOffset.UtcNow;
                _capture.ResetDetections();
                RaisePropertyChanged(nameof(MachinePositionSummary));
                if (!ContextValid(true, out error))
                {
                    Fail(error);
                    return;
                }

                Step(2, "Acquiring a fresh stable Hough circle.");
                var found = await WaitForStableCircleAsync();
                if (found == null)
                {
                    Fail("No verified stable circle: " + LastRejection);
                    return;
                }
                Step(3, "Centering the camera; verifying fresh circle after each move.");

                while (true)
                {
                    if (!ContextValid(true, out error))
                    {
                        Fail(error);
                        return;
                    }
                    if (!FiducialCalibrationGate.TryQualify(found,
                        _profile.Key, _scale, _moveCompletedUtc, DateTimeOffset.UtcNow,
                        out error))
                    {
                        Fail("Circle evidence expired: " + error);
                        return;
                    }
                    if (FiducialCalibrationGate.CanCommitOrigin(found,
                        _profile.Key, _scale, _moveCompletedUtc, DateTimeOffset.UtcNow,
                        out error))
                        break;

                    if (CenteringMoves >= MaximumCenteringMoves)
                    {
                        Fail("Fiducial did not center after " + CenteringMoves +
                            " corrective moves. Last measured error: " +
                            found.OffsetXmm.ToString("F2") + ", " +
                            found.OffsetYmm.ToString("F2") + " mm.");
                        return;
                    }

                    var current = machine.MachinePosition;
                    var correctedX = current.X + found.OffsetXmm;
                    var correctedY = current.Y + found.OffsetYmm;
                    var area = machine.Settings.WorkAreaSize;
                    if (correctedX < 0 || correctedY < 0 ||
                        correctedX > area.X || correctedY > area.Y)
                    {
                        Fail("Centered correction exceeds configured machine XY work area.");
                        return;
                    }

                    CenteringMoves++;
                    Steps[3].Detail = "Move " + CenteringMoves + ": correction X=" +
                        found.OffsetXmm.ToString("+0.00;-0.00;0.00") + " mm, Y=" +
                        found.OffsetYmm.ToString("+0.00;-0.00;0.00") + " mm.";
                    Log(Steps[3].Detail);
                    machine.GotoPoint(new LagoVista.Core.Models.Drawing.Point2D<double>(
                        found.OffsetXmm, found.OffsetYmm), relativeMove: true);
                    var centeredMove = await machine.SpinUntilIdleAsync(10000);
                    if (!centeredMove.Successful)
                    {
                        Fail("Fiducial correction did not finish: " + centeredMove.ErrorMessage);
                        return;
                    }
                    _moveCompletedUtc = DateTimeOffset.UtcNow;
                    _capture.ResetDetections();
                    RaisePropertyChanged(nameof(MachinePositionSummary));
                    if (!ContextValid(true, out error))
                    {
                        Fail(error);
                        return;
                    }
                    found = await WaitForStableCircleAsync();
                    if (found == null)
                    {
                        Fail("No fresh stable circle after centering move " +
                            CenteringMoves + ": " + LastRejection);
                        return;
                    }
                }

                if (!ContextValid(true, out error) ||
                    !FiducialCalibrationGate.CanCommitOrigin(found,
                        _profile.Key, _scale, _moveCompletedUtc, DateTimeOffset.UtcNow, out error))
                {
                    Fail("Final centered observation is not current: " + error);
                    return;
                }

                Step(4, "Applying verified machine origin; no staging-plate continuation.");
                // Machine.ResetMachineCoordinates queues G92; wait for M400 acknowledgement
                // before claiming success. Never set WasMachinOriginCalibrated early.
                resetRequested = true;
                var reset = await machine.ResetMachineCoordinates(MachineConfiguration.MachineFiducial);
                if (!reset.Successful)
                {
                    Fail("Machine origin command could not be queued: " + reset.ErrorMessage);
                    return;
                }
                var resetConfirmed = await machine.SpinUntilIdleAsync(10000);
                if (!resetConfirmed.Successful || !machine.Connected)
                {
                    Fail("Origin reset acknowledgement missing; machine origin now unverified.");
                    return;
                }
                machine.WasMachinOriginCalibrated = true;
                originCommitted = true;

                // Resetting the machine origin invalidates dependent staging-plate
                // references, just as the legacy origin operation does. Preserve
                // their measured locations but require a separate staging procedure
                // before considering the plates calibrated again.
                foreach (var plate in MachineConfiguration.StagingPlates)
                    plate.IsCalibrated = false;

                // Persist the invalidated dependent state; never silently allow
                // a stale staging plate to appear verified after a new origin.
                var save = await MachineRepo.SaveCurrentMachineAsync();
                if (!save.Successful)
                {
                    FinishCurrentStep("Failed");
                    Steps[4].Detail = "Origin was applied, but staging-plate invalidation could not be saved.";
                    Status = "Origin applied; save failed. Revalidate staging plates before use: " +
                        save.ErrorMessage;
                    machine.AddStatusMessage(StatusMessageTypes.Warning, Status);
                    return;
                }

                FinishCurrentStep("Passed");
                Steps[4].Detail = "Verified origin applied; staging plates marked uncalibrated.";
                Status = "Machine fiducial calibrated. Staging plates require a separate calibration.";
                Log(Status);
                machine.AddStatusMessage(StatusMessageTypes.Info, Status);
            }
            catch (Exception ex)
            {
                Fail("Machine fiducial calibration stopped: " + ex.Message);
            }
            finally
            {
                StopListening();
                if (resetRequested && !originCommitted && Machine != null)
                    Machine.WasMachinOriginCalibrated = false;
                _pending = null;
                _clock.Stop();
                RaisePropertyChanged(nameof(Elapsed));
                Running = false;
            }
        }

        private async Task<FiducialCircleObservation> WaitForStableCircleAsync()
        {
            var waiting = new TaskCompletionSource<FiducialCircleObservation>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _pending = waiting;
            _capture.ResetDetections();
            try
            {
                // A unique registration per measurement window prevents a
                // late timer callback from the preceding correction attempt
                // from completing the next window's observation.
                var handler = new CircleWaitHandler(this, waiting);
                _registeredHandler = handler;
                _locator.RegisterCircleLocatedHandler(handler, DetectionTimeoutMs);
                var finished = await Task.WhenAny(waiting.Task,
                    Task.Delay(DetectionTimeoutMs + 1000));
                if (finished != waiting.Task)
                {
                    LastRejection = "Detector timeout with no qualifying circle.";
                    return null;
                }
                return await waiting.Task;
            }
            finally
            {
                StopListening();
                if (ReferenceEquals(_pending, waiting))
                    _pending = null;
            }
        }

        private void StopListening()
        {
            var handler = _registeredHandler;
            if (handler == null) return;
            _registeredHandler = null;
            _locator.UnregisterCircleLocatedHandler(handler);
        }

        private sealed class CircleWaitHandler : ICircleLocatedHandler
        {
            private readonly MachineFiducialCalibrationViewModel _owner;
            private readonly TaskCompletionSource<FiducialCircleObservation> _completion;

            public CircleWaitHandler(MachineFiducialCalibrationViewModel owner,
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

            public void CircleLocatorTimeout()
            {
                _owner.EndWait(_completion,
                    "The circle detector timed out before a stable observation.");
            }

            public void CircleLocatorAborted()
            {
                _owner.EndWait(_completion, "The circle locator was aborted.");
            }
        }

        private void EndWait(TaskCompletionSource<FiducialCircleObservation> completion,
            string reason)
        {
            if (!ReferenceEquals(_pending, completion)) return;
            completion.TrySetResult(null);
            SetRejection(reason);
        }

        private void ObserveCircle(MVLocatedCircle circle,
            TaskCompletionSource<FiducialCircleObservation> request)
        {
            if (!ReferenceEquals(_pending, request)) return;
            if (request == null || circle == null || _capture == null ||
                _camera == null || _profile == null ||
                circle.CameraType != CameraTypes.Position) return;

            var offset = circle.OffsetMM;
            var pixels = circle.OffsetPixels;
            var center = circle.CenterPixels;
            var snapshot = new FiducialCircleObservation
            {
                TrackId = circle.Id,
                CameraType = circle.CameraType,
                ObservedAtUtc = DateTimeOffset.UtcNow,
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
                OffsetXpx = pixels.X,
                OffsetYpx = pixels.Y,
                OffsetXmm = offset.X,
                OffsetYmm = offset.Y,
                PositionSpreadMm = circle.PositionSpreadMM,
                SearchRadiusPx = _profile.TargetImageRadius * _scale,
                ErrorToleranceMm = _profile.ErrorToleranceMM
            };

            var good = FiducialCalibrationGate.TryQualify(snapshot,
                _profile.Key, _scale, _moveCompletedUtc,
                DateTimeOffset.UtcNow, out var reason);
            if (Machine?.Busy == true)
            {
                good = false;
                reason = "Machine is still moving";
            }
            snapshot.Qualified = good;
            snapshot.RejectionReason = reason;
            void Update()
            {
                CurrentObservation = snapshot;
                ObservationCount++;
                if (good) StableObservationCount++;
                if (!good) LastRejection = reason;
                if (ObservationCount == 1 || ObservationCount % 8 == 0 || good)
                    Log("Circle #" + snapshot.FoundCount +
                        "  offset=" + snapshot.OffsetXmm.ToString("F2") + "," +
                        snapshot.OffsetYmm.ToString("F2") + " mm; radius=" +
                        snapshot.RadiusMm.ToString("F2") + " mm; " +
                        (good ? "stable" : reason));
            }
            if (_uiContext == null || SynchronizationContext.Current == _uiContext)
                Update();
            else
                _uiContext.Post(_ => Update(), null);
            if (good)
                request.TrySetResult(snapshot);
        }

        private void SetRejection(string reason)
        {
            if (_uiContext == null || SynchronizationContext.Current == _uiContext)
                LastRejection = reason;
            else
                _uiContext.Post(_ => LastRejection = reason, null);
        }
    }
}
