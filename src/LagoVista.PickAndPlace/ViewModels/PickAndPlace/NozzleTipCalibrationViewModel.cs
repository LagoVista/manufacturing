using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Operations;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    /// <summary>
    /// 30-degree bottom-camera nozzle runout pilot. Replaces the commented
    /// historical G92 and hard-coded 20px/mm calibration. The first physical
    /// run produces a reviewed LOGICAL zero / candidate compensation table;
    /// it never resets firmware coordinates or changes placement.
    /// </summary>
    public partial class NozzleTipCalibrationViewModel : ViewModelBase,
        INozzleTipCalibrationViewModel, ICircleLocatedHandler
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILocatorViewModel _locator;
        private readonly IImageCaptureService _bottomCameraBootstrap;
        private readonly object _observationGate = new object();
        private TaskCompletionSource<NozzleRunoutSample> _pendingSample;
        private IMachine _activeMachine;
        private MachineToolHead _activeHead;
        private MachineToolHead _lastMeasuredHead;
        private MachineToolHead _selectedRunoutHead;
        private string _activeNozzleId;
        private string _cameraId;
        private double _cameraScale;
        private bool _runoutOpticalLock;
        private bool _running;
        private bool _finished;
        private string _status = "Ready. Complete machine fiducial and two staging-plate calibrations, then this procedure configures its camera automatically.";
        private int _targetAngle;
        private int _currentAngle;
        private NozzleRunoutResult _result;
        private MachineOperationSession _session;

        public NozzleTipCalibrationViewModel(IMachineRepo machineRepo,
            ILocatorViewModel locatorViewModel, IImageCaptureService bottomCameraBootstrap)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _locator = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
            _bottomCameraBootstrap = bottomCameraBootstrap ??
                throw new ArgumentNullException(nameof(bottomCameraBootstrap));
            CalibrateBottomCameraCommand = new RelayCommand(async () => await StartAsync());
        }

        public ObservableCollection<FeederCalibrationFlowStep> RunoutSteps { get; private set; } =
            new ObservableCollection<FeederCalibrationFlowStep>();
        public ObservableCollection<string> CalibrationUpdates { get; } = new ObservableCollection<string>();
        public RelayCommand CalibrateBottomCameraCommand { get; }
        public bool RunoutRunning { get => _running; private set => Set(ref _running, value); }
        public bool RunoutFinished { get => _finished; private set => Set(ref _finished, value); }
        public string Status { get => _status; private set => Set(ref _status, value); }
        /// <summary>
        /// Exactly one installed head/nozzle is selected for each supervised
        /// optical run. Opening the UI has no side effects and never starts
        /// the sweep or invalidates any installed head's calibration.
        /// </summary>
        public IReadOnlyList<MachineToolHead> AvailableRunoutHeads =>
            InstalledNozzleCalibrationPlan.InstalledHeads(_machineRepo.CurrentMachine?.Settings);

        public MachineToolHead SelectedRunoutHead
        {
            get => _selectedRunoutHead ?? AvailableRunoutHeads.FirstOrDefault();
            set
            {
                if (RunoutRunning) return;
                if (value != null && !AvailableRunoutHeads.Contains(value)) return;
                Set(ref _selectedRunoutHead, value);
                RaisePropertyChanged(nameof(ToolHeadName));
                RaisePropertyChanged(nameof(NozzleName));
                RaisePropertyChanged(nameof(CanStartRunout));
            }
        }

        private MachineToolHead DisplayHead
        {
            get
            {
                var machine = _machineRepo.CurrentMachine;
                return _activeHead ?? _lastMeasuredHead ?? SelectedRunoutHead;
            }
        }

        public string ToolHeadName => DisplayHead == null
            ? "No installed nozzle assigned" : $"Head {DisplayHead.HeadIndex}: {DisplayHead.Name}";
        public string NozzleName => DisplayHead?.CurrentNozzle?.Text ?? "No installed nozzle";
        public int TargetAngle { get => _targetAngle; set => Set(ref _targetAngle, value); }
        public int CurrentCalibrationAngle { get => _currentAngle; set => Set(ref _currentAngle, value); }
        public NozzleRunoutResult CandidateResult { get => _result; private set => Set(ref _result, value); }
        public string CandidateZero => CandidateResult == null
            ? "Not yet measured" : $"Head {(_activeHead ?? _lastMeasuredHead)?.HeadIndex}: {CandidateResult.LogicalZeroAngle}° (logical zero, not firmware G92)";
        public string RunoutSummary => CandidateResult == null
            ? "No calibrated runout table. Placement compensation remains unchanged."
            : $"Peak radial deviation: {CandidateResult.MaxRadialDeviationMm:F3} mm | " +
              $"X span: {CandidateResult.RangeXmm:F3} mm | Y span: {CandidateResult.RangeYmm:F3} mm";

        private static ObservableCollection<FeederCalibrationFlowStep> NewPlan(
            IReadOnlyList<MachineToolHead> installed)
        {
            var steps = new ObservableCollection<FeederCalibrationFlowStep>();
            for (var index = 0; index < installed.Count; index++)
            {
                var head = installed[index];
                var includeFinalSafeHeight = index == installed.Count - 1;
                var prefix = $"head-{head.HeadIndex}/";
                foreach (var step in NozzleRunoutProcedureDefinition.Create(includeFinalSafeHeight).Steps)
                    steps.Add(new FeederCalibrationFlowStep
                    {
                        Number = steps.Count + 1,
                        StepKey = prefix + step.Key,
                        Title = $"Head {head.HeadIndex} ({head.CurrentNozzle?.Text}) · {step.Title}"
                    });
            }
            return steps;
        }

        private FeederCalibrationFlowStep Step(string key) =>
            RunoutSteps.FirstOrDefault(x =>
                x.StepKey == $"head-{_activeHead?.HeadIndex}/{key}");

        private bool ValidCameraBeforeMotion(IMachine machine,
            MachineCamera configuredCamera, out string reason)
        {
            reason = null;
            var capture = machine?.PartInspectionCaptureService;
            if (configuredCamera == null || capture?.Camera == null ||
                capture.CameraType != CameraTypes.PartInspection ||
                !String.Equals(Convert.ToString(capture.Camera.Id),
                    Convert.ToString(configuredCamera.Id), StringComparison.Ordinal))
            {
                reason = "Bottom-camera capture service is missing or bound to a different camera.";
                return false;
            }
            if (!capture.IsCapturing || capture.CaptureFrameSize.Width <= 0 ||
                capture.CaptureFrameSize.Height <= 0)
            {
                reason = "Bottom camera is not capturing live video. Start its stream and confirm live frames before moving.";
                return false;
            }
            if (capture.Profile?.Key != VisionProfile.VisionProfile_Nozzle ||
                !capture.Profile.FindCircles)
            {
                reason = $"Bottom camera's active profile '{capture.Profile?.Key ?? "none"}' " +
                    "must be Nozzle with circle detection enabled before any calibration motion.";
                return false;
            }
            var scale = capture.Camera.EffectivePixelsPerMM;
            if (Double.IsNaN(scale) || Double.IsInfinity(scale) || scale <= 0)
            {
                reason = "Bottom camera has no valid calibrated pixels/mm. Calibrate its scale before motion.";
                return false;
            }
            return true;
        }

        private bool ValidContext(out string reason)
        {
            reason = null;
            var m = _activeMachine;
            if (m == null || !m.Connected || m.Mode != OperatingMode.Manual ||
                !m.WasMachineHomed || !m.WasMachinOriginCalibrated ||
                m.CurrentMachineToolHead == null ||
                !ReferenceEquals(m.CurrentMachineToolHead, _activeHead) ||
                !String.Equals(Convert.ToString(_activeHead?.CurrentNozzle?.Id),
                    _activeNozzleId, StringComparison.Ordinal) ||
                !ReferenceEquals(_machineRepo.CurrentMachine, m))
            {
                reason = "Machine is not homed/connected/Manual or the selected tool head/nozzle changed.";
                return false;
            }
            var configuredCamera = m.Settings?.Cameras?.FirstOrDefault(camera =>
                camera.CameraType.Value == CameraTypes.PartInspection);
            if (!ValidCameraBeforeMotion(m, configuredCamera, out reason))
                return false;
            if (!String.Equals(Convert.ToString(m.PartInspectionCaptureService.Camera.Id),
                    _cameraId, StringComparison.Ordinal) ||
                Math.Abs(m.PartInspectionCaptureService.Camera.EffectivePixelsPerMM -
                    _cameraScale) > 0.1)
            {
                reason = "Bottom-camera identity or calibrated pixels/mm changed during the run.";
                return false;
            }
            return true;
        }

        private async Task<bool> MotionIdleAsync(CancellationToken token, uint timeout = 30000)
        {
            var idle = await _activeMachine.SpinUntilIdleAsync(timeout);
            if (!idle.Successful) return false;
            var quiet = 0;
            for (var poll = 0; poll < 40; poll++)
            {
                token.ThrowIfCancellationRequested();
                if (!_activeMachine.Connected) return false;
                if (!_activeMachine.Busy &&
                    _activeMachine.ToSendQueueCount == 0 &&
                    _activeMachine.UnacknowledgedBytesSent == 0)
                {
                    if (++quiet >= 2) return true;
                }
                else quiet = 0;
                await Task.Delay(75, token);
            }
            return false;
        }

        private async Task<NozzleRunoutSample> ObserveFreshCircleAsync(int angle,
            CancellationToken token)
        {
            var capture = _activeMachine.PartInspectionCaptureService;
            if (!ValidContext(out _)) return null;
            TaskCompletionSource<NozzleRunoutSample> request;
            lock (_observationGate)
            {
                request = new TaskCompletionSource<NozzleRunoutSample>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _pendingSample = request;
            }
            try
            {
                // Each angle MUST build a new stabilization history.
                capture.ResetDetections();
                _locator.RegisterCircleLocatedHandler(this, 7500);
                var outcome = await Task.WhenAny(request.Task, Task.Delay(7500, token));
                token.ThrowIfCancellationRequested();
                if (outcome != request.Task) return null;
                var value = await request.Task;
                if (value == null) return null;
                value.CommandedAngle = angle;
                return value;
            }
            finally
            {
                _locator.UnregisterCircleLocatedHandler(this);
                lock (_observationGate)
                {
                    if (ReferenceEquals(_pendingSample, request))
                        _pendingSample = null;
                }
            }
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            TaskCompletionSource<NozzleRunoutSample> pending;
            lock (_observationGate) pending = _pendingSample;
            var profile = _activeMachine?.PartInspectionCaptureService?.Profile;
            if (pending == null || !_runoutOpticalLock ||
                !NozzleRunoutVisionPreparation.MatchesLockedOpening(
                    profile, _cameraScale, circle) ||
                !ValidContext(out _)) return;

            // Publish only the real, scale/profile-locked observation on the
            // UI thread. This never substitutes for the existing safety gate.
            PublishRunoutObservation(circle, true);

            // Do not use MVLocatedCircle.OffsetMM: that legacy property rounds
            // to 0.1mm and would erase the runout precision we need.
            var offset = circle.OffsetPixels;
            pending.TrySetResult(new NozzleRunoutSample
            {
                Xmm=offset.X / _cameraScale,
                Ymm=-offset.Y / _cameraScale,
                PixelsPerMm=_cameraScale,
                CapturedAtUtc=DateTimeOffset.UtcNow
            });
        }

        public void CircleLocatorTimeout()
        {
            lock (_observationGate) _pendingSample?.TrySetResult(null);
        }

        public void CircleLocatorAborted()
        {
            lock (_observationGate) _pendingSample?.TrySetResult(null);
        }

        // Kept for interface compatibility with the original calibration
        // prototype; the reliable path is ICircleLocatedHandler above.
        public void CircleLocation(Point2D<double> center, double diameter,
            Point2D<double> stdDeviation) { }

        private bool Stop(FeederCalibrationFlowStep step, string reason)
        {
            step?.Fail(reason);
            var currentKey = step?.StepKey;
            if (currentKey != null && currentKey.Contains("/"))
                currentKey = currentKey.Substring(currentKey.IndexOf('/') + 1);
            if (_session?.Snapshot.Status == MachineOperationRunStatus.Running &&
                _session.Snapshot.ActiveStepKey == currentKey)
                _session.BlockActiveStep(reason, DateTimeOffset.UtcNow);
            Status = "Stopped: " + reason;
            return false;
        }

        private void Pass(FeederCalibrationFlowStep step, string description,
            MachineOperationVerificationKind kind, double? pixelsPerMm = null)
        {
            if (_session != null)
            {
                var now=DateTimeOffset.UtcNow;
                _session.RecordEvidence(new MachineOperationEvidence
                {
                    RunId=_session.Snapshot.RunId,
                    StepKey=step.StepKey.Substring(step.StepKey.IndexOf('/') + 1),
                    Name=step.Title, Value=description, Unit="mm",
                    Source=$"Bottom camera / nozzle {_activeNozzleId}, tool head {_activeHead?.HeadIndex}",
                    Verification=kind, Saved=false, CapturedAtUtc=now,
                    VisionProfileKey=VisionProfile.VisionProfile_Nozzle,
                    PixelsPerMm=pixelsPerMm
                });
                _session.CompleteActiveStep(
                    step.StepKey.Substring(step.StepKey.IndexOf('/') + 1),
                    DateTimeOffset.UtcNow);
            }
            step.Pass(description);
        }

        private void PassMachineStep(FeederCalibrationFlowStep step, string description)
        {
            if (_session != null)
            {
                var now = DateTimeOffset.UtcNow;
                var key = step.StepKey.Substring(step.StepKey.IndexOf('/') + 1);
                _session.RecordEvidence(new MachineOperationEvidence
                {
                    RunId = _session.Snapshot.RunId,
                    StepKey = key,
                    Name = step.Title,
                    Value = description,
                    Unit = "mm",
                    Source = "Machine controller / configured safe move height",
                    Verification = MachineOperationVerificationKind.MachineObserved,
                    Saved = false,
                    CapturedAtUtc = now
                });
                _session.CompleteActiveStep(key, DateTimeOffset.UtcNow);
            }
            step.Pass(description);
        }

        /// <summary>
        /// Machine startup / manual calibration procedure. Iterates every
        /// configured, installed nozzle head. Never auto-moves on mere
        /// connection: the supervised modal invokes this after homing.
        /// </summary>
        public async Task StartAsync()
        {
            if (RunoutRunning || RunoutPreviewBusy) return;
            if (HasRunoutVisionDraft || HasRunoutVisionPreview)
            {
                Status = "Save or revert nozzle vision settings before starting a physical run.";
                return;
            }
            _runoutUiContext = SynchronizationContext.Current;
            RunoutVideoObservation = null;
            var machine = _machineRepo.CurrentMachine;
            // A single operator-selected head is the entire run plan.
            // Never silently sweep or invalidate other installed heads.
            var selected = SelectedRunoutHead;
            var available = InstalledNozzleCalibrationPlan.InstalledHeads(machine?.Settings);
            IReadOnlyList<MachineToolHead> installed =
                selected != null && available.Contains(selected)
                    ? new List<MachineToolHead> { selected }
                    : new List<MachineToolHead>();
            RunoutSteps = NewPlan(installed);
            RaisePropertyChanged(nameof(RunoutSteps));
            CalibrationUpdates.Clear();
            CandidateResult = null;
            _session = null;
            _lastMeasuredHead = null;
            RaisePropertyChanged(nameof(CandidateZero));
            RaisePropertyChanged(nameof(RunoutSummary));
            RunoutFinished = false;
            RunoutRunning = true;
            RaisePropertyChanged(nameof(CanStartRunout));
            RaisePropertyChanged(nameof(CanTuneRunoutVision));
            Status = installed.Count == 1
                ? $"Starting supervised runout for Head {installed[0].HeadIndex} ({installed[0].CurrentNozzle?.Text})."
                : "Select one installed nozzle head to start runout.";
            try
            {
                if (installed.Count == 0)
                {
                    Status = "Stopped: select an installed nozzle head before starting. No motion sent.";
                    return;
                }
                _activeHead = installed[0];
                RaisePropertyChanged(nameof(ToolHeadName));
                RaisePropertyChanged(nameof(NozzleName));
                if (machine == null || !machine.Connected ||
                    machine.Mode != OperatingMode.Manual || !machine.WasMachineHomed ||
                    machine.Busy)
                {
                    Stop(Step("readiness"),
                        "Connect and home the machine before calibrating runout; motor commands were not sent.");
                    return;
                }
                if (!NozzleRunoutReadinessPolicy.TryValidate(machine.Settings,
                    machine.WasMachinOriginCalibrated, out var prerequisites))
                {
                    Stop(Step("readiness"), prerequisites + " No movement sent.");
                    return;
                }
                var cameraConfig = machine.Settings?.Cameras?.FirstOrDefault(c =>
                    c.CameraType.Value == CameraTypes.PartInspection);
                if (cameraConfig?.AbsolutePosition == null ||
                    cameraConfig.AbsolutePosition.X < 0 || cameraConfig.AbsolutePosition.Y < 0 ||
                    cameraConfig.AbsolutePosition.X > machine.Settings.WorkAreaSize.X ||
                    cameraConfig.AbsolutePosition.Y > machine.Settings.WorkAreaSize.Y)
                {
                    Stop(Step("camera"), "No configured bottom camera with a valid XY destination; no movement sent.");
                    return;
                }
                foreach (var tool in installed)
                {
                    if (!ReferenceEquals(_machineRepo.CurrentMachine, machine) ||
                        !ReferenceEquals(machine.Settings?.ToolHeads?.FirstOrDefault(
                            h => h.HeadIndex == tool.HeadIndex), tool))
                    {
                        Stop(Step("position"), "Machine or configured tool heads changed; aborting calibration.");
                        return;
                    }

                    _activeHead = tool;
                    _lastMeasuredHead = tool;
                    RaisePropertyChanged(nameof(ToolHeadName));
                    RaisePropertyChanged(nameof(NozzleName));

                    // Failure always leaves this head uncalibrated; a
                    // successful twelve-angle measurement explicitly earns
                    // the runtime green check.
                    tool.InvalidateRunoutCalibration();
                    var isFinalHead = ReferenceEquals(tool, installed[installed.Count - 1]);
                    if (!await CalibrateInstalledHeadAsync(machine, tool, cameraConfig, isFinalHead))
                        return;
                }
                Status = $"Head {installed[0].HeadIndex} runout measured this session. Other heads were not touched. Measurements are invalidated on machine restart or nozzle changes; production compensation remains separate.";
            }
            catch (Exception ex)
            {
                var step = RunoutSteps.FirstOrDefault(s => s.Status == "Running");
                Stop(step, "Nozzle runout procedure error: " + ex.Message);
            }
            finally
            {
                lock (_observationGate) _pendingSample = null;
                _activeMachine = null;
                _activeHead = null;
                _runoutOpticalLock = false;
                _session = null;
                RunoutRunning = false;
                RunoutFinished = true;
                RaisePropertyChanged(nameof(CanStartRunout));
                RaisePropertyChanged(nameof(CanTuneRunoutVision));
                RaisePropertyChanged(nameof(ToolHeadName));
                RaisePropertyChanged(nameof(NozzleName));
            }
        }

        private async Task<bool> CalibrateInstalledHeadAsync(
            IMachine machine, MachineToolHead tool, MachineCamera cameraConfig,
            bool isFinalHead)
        {
                _activeMachine = machine;
                _activeHead = tool;
                _runoutOpticalLock = false;
                _activeNozzleId = Convert.ToString(tool.CurrentNozzle.Id);
                RaisePropertyChanged(nameof(ToolHeadName));
                RaisePropertyChanged(nameof(NozzleName));

                // All non-motion setup belongs to the procedure. Create the
                // canonical session BEFORE marking any stage as passed.
                _session = new MachineOperationSession(
                    NozzleRunoutProcedureDefinition.Create(isFinalHead),
                    new MachineOperationTarget
                    {
                        MachineId = Convert.ToString(machine.Settings.Id),
                        CameraId = Convert.ToString(cameraConfig.Id)
                    }, DateTimeOffset.UtcNow);
                _session.Start(DateTimeOffset.UtcNow);

                var readinessStep = Step("readiness");
                readinessStep.Begin("Checking completed machine fiducial and both staging plates.");
                if (!NozzleRunoutReadinessPolicy.TryValidate(
                    machine.Settings, machine.WasMachinOriginCalibrated,
                    out var readinessError))
                {
                    Stop(readinessStep, readinessError + " No movement sent.");
                    return false;
                }
                Pass(readinessStep, "Machine fiducial and both staging plates calibrated this session.",
                    MachineOperationVerificationKind.MachineObserved);

                var cameraStep = Step("camera");
                cameraStep.Begin("Selecting Nozzle profile, enabling circle detection and starting live bottom camera.");
                // The camera view may not have been opened in the desktop UI.
                // Bind the existing injectable capture service on demand.
                if (machine.PartInspectionCaptureService == null)
                    _bottomCameraBootstrap.CameraType = CameraTypes.PartInspection;
                var capture = machine.PartInspectionCaptureService;
                if (capture?.Camera == null || capture.CameraType != CameraTypes.PartInspection)
                {
                    Stop(cameraStep, "Bottom-camera service could not be initialized automatically. No movement sent.");
                    return false;
                }
                machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_Nozzle);
                if (!NozzleRunoutVisionPreparation.TryEnableCircleDetection(
                    capture.Profile, out var visionError))
                {
                    Stop(cameraStep, visionError + " No movement sent.");
                    return false;
                }
                // Re-selecting reapplies the now-enabled bottom illumination
                // through the canonical Machine.SetVisionProfile path.
                machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_Nozzle);
                var captureReady = await capture.EnsureCaptureReadyAsync();
                if (!captureReady.Successful)
                {
                    Stop(cameraStep, captureReady.ErrorMessage + " No movement sent.");
                    return false;
                }
                _cameraId = Convert.ToString(capture.Camera.Id);
                _cameraScale = capture.Camera.EffectivePixelsPerMM;
                if (!ValidCameraBeforeMotion(machine, cameraConfig, out visionError))
                {
                    Stop(cameraStep, visionError + " No movement sent.");
                    return false;
                }
                Pass(cameraStep, $"Live bottom camera ready; Nozzle circle detection enabled, " +
                    $"{_cameraScale:F1} px/mm.", MachineOperationVerificationKind.MachineObserved,
                    _cameraScale);

                var planStep = Step("move-plan");
                planStep.Begin("Reading controller XY and validating camera-first travel plus the tool-head shift at the camera.");
                // M114 is a read; preflight must approve the exact order
                // we execute: old head -> camera navigation -> bottom-camera
                // XY -> target head. A head-offset shift at the current
                // location may exceed travel even when the camera is safe.
                var location = await machine.GetCurrentLocationAsync(5000);
                if (!location.Successful || location.Result == null)
                {
                    Stop(planStep, "Controller XY could not be read; no movement sent.");
                    return false;
                }
                var previousHead = machine.CurrentMachineToolHead;
                if (!NozzleRunoutMotionPreflight.TryValidate(
                    location.Result, tool.Offset, true,
                    cameraConfig.AbsolutePosition, machine.Settings.WorkAreaSize,
                    out var geometryError, previousHead?.Offset))
                {
                    Stop(planStep, "No movement sent: " + geometryError);
                    return false;
                }
                var planDetails = $"Current ({location.Result.X:F2}, {location.Result.Y:F2}); " +
                    $"previous head {previousHead?.Name ?? "camera"}; " +
                    $"camera ({cameraConfig.AbsolutePosition.X:F2}, {cameraConfig.AbsolutePosition.Y:F2}); " +
                    $"head {tool.HeadIndex} shift ({tool.Offset?.X:F2}, {tool.Offset?.Y:F2}); " +
                    $"bounds ({machine.Settings.WorkAreaSize.X:F2}, {machine.Settings.WorkAreaSize.Y:F2}) mm.";
                machine.AddStatusMessage(StatusMessageTypes.Info,
                    "Nozzle runout camera-first motion plan approved: " + planDetails);
                Pass(planStep, planDetails, MachineOperationVerificationKind.MachineObserved);

                var positionStep = Step("position");
                positionStep.Begin($"Return to camera navigation, move to bottom camera, then select Head {tool.HeadIndex} ({tool.CurrentNozzle.Text}).");

                // The old tool head might already be selected. Restore
                // camera navigation at the original XY BEFORE traveling to
                // the camera, and verify the controller's G92 reference.
                if (previousHead != null)
                {
                    await machine.MoveToCameraAsync();
                    if (machine.CurrentMachineToolHead != null ||
                        !await MotionIdleAsync(CancellationToken.None))
                    {
                        Stop(positionStep, "Unable to restore camera navigation safely.");
                        return false;
                    }
                    var afterCameraNavigation = await machine.GetCurrentLocationAsync(5000);
                    if (!afterCameraNavigation.Successful ||
                        afterCameraNavigation.Result == null ||
                        !FeederOriginCalibrationMath.SamePosition(
                            afterCameraNavigation.Result, location.Result, 0.5))
                    {
                        Stop(positionStep, "Camera-navigation coordinate reset did not preserve XY within 0.5 mm. No camera approach sent.");
                        return false;
                    }
                }

                // We are now in camera coordinates. Raise Z, then move to
                // the camera's known XY while the nozzle-offset shift is
                // not yet active.
                machine.SetAbsoluteMode();
                machine.SendSafeMoveHeight();
                if (!await MotionIdleAsync(CancellationToken.None, 10000))
                {
                    Stop(positionStep, "Safe-height move did not settle before camera approach.");
                    return false;
                }
                machine.GotoPoint(cameraConfig.AbsolutePosition);
                if (!await MotionIdleAsync(CancellationToken.None))
                {
                    Stop(positionStep, "Camera-navigation XY approach did not settle.");
                    return false;
                }
                var atCamera = await machine.GetCurrentLocationAsync(5000);
                if (!atCamera.Successful || atCamera.Result == null ||
                    !FeederOriginCalibrationMath.SamePosition(
                        atCamera.Result, cameraConfig.AbsolutePosition, 0.5))
                {
                    Stop(positionStep, "Controller did not reach the configured bottom camera XY; tool head was not selected.");
                    return false;
                }
                if (!ValidCameraBeforeMotion(machine, cameraConfig, out var contextError))
                {
                    Stop(positionStep, "Bottom camera became unavailable before selecting the tool head: " + contextError);
                    return false;
                }

                // Only now shift from camera to tool navigation, where
                // the full head offset was preflighted at camera XY.
                await machine.MoveToToolHeadAsync(tool);
                if (!ReferenceEquals(machine.CurrentMachineToolHead, tool) ||
                    !await MotionIdleAsync(CancellationToken.None))
                {
                    Stop(positionStep, $"Could not select Head {tool.HeadIndex} after reaching the bottom camera.");
                    return false;
                }
                var afterHeadSwitch = await machine.GetCurrentLocationAsync(5000);
                if (!afterHeadSwitch.Successful || afterHeadSwitch.Result == null ||
                    !FeederOriginCalibrationMath.SamePosition(
                        afterHeadSwitch.Result, cameraConfig.AbsolutePosition, 0.5))
                {
                    Stop(positionStep,
                        "Tool-head coordinate reset did not preserve the bottom-camera XY within 0.5 mm. Aborting before detection-height descent.");
                    return false;
                }
                if (!ValidContext(out contextError))
                {
                    Stop(positionStep, contextError + " No detection-height movement sent.");
                    return false;
                }

                // Reuse the canonical bottom-camera detection-height path.
                // Its XY target is already verified; it now only needs
                // to lower the active head's ZL/ZR at that position.
                await machine.GoToPartInspectionCameraAsync();
                if (!await MotionIdleAsync(CancellationToken.None) ||
                    !ValidContext(out contextError))
                {
                    Stop(positionStep, contextError ?? "Detection-height approach did not settle.");
                    return false;
                }
                var cameraXY = await machine.GetCurrentLocationAsync(5000);
                if (!cameraXY.Successful || cameraXY.Result == null ||
                    !FeederOriginCalibrationMath.SamePosition(
                        cameraXY.Result, cameraConfig.AbsolutePosition, 0.5))
                {
                    Stop(positionStep, "Machine lost the verified bottom-camera XY after lowering the nozzle.");
                    return false;
                }
                Pass(positionStep,$"Head {tool.HeadIndex}, nozzle {_activeNozzleId}, {_cameraScale:F1} px/mm.",
                    MachineOperationVerificationKind.MachineObserved,_cameraScale);

                // The optical detector is always retuned for THIS installed
                // nozzle and the current bottom-camera scale. Previously saved
                // image-tuning results (including old pixels/mm) are not a
                // prerequisite or substitute for fresh runout evidence.
                var tuneStep = Step("tune-image");
                tuneStep.Begin($"Auto-tuning the dark opening for Head {tool.HeadIndex} from 24 fresh camera frames.");
                if (!ValidContext(out contextError) ||
                    !await MotionIdleAsync(CancellationToken.None))
                {
                    Stop(tuneStep, contextError ?? "Machine was not idle for nozzle image acquisition.");
                    return false;
                }
                var tuneResponse = await capture.TuneNozzleOpeningAsync();
                if (!tuneResponse.Successful || tuneResponse.Result == null)
                {
                    Stop(tuneStep, "Nozzle opening could not be auto-tuned: " +
                        (tuneResponse.ErrorMessage ?? "No repeatable dark opening."));
                    return false;
                }
                if (!ValidContext(out contextError) ||
                    !await MotionIdleAsync(CancellationToken.None))
                {
                    Stop(tuneStep, contextError ??
                        "Machine was not idle after nozzle image tuning; optical lock was not applied.");
                    return false;
                }
                if (!NozzleRunoutVisionPreparation.TryLockAdaptiveOpening(
                    capture.Profile, _cameraScale, tuneResponse.Result, out var tuneError))
                {
                    Stop(tuneStep, tuneError);
                    return false;
                }
                _runoutOpticalLock = true;
                capture.ResetDetections();
                var tuned = tuneResponse.Result;
                Pass(tuneStep,
                    $"Head {tool.HeadIndex}: {tuned.GoodFrames}/{tuned.TotalFrames} frames; " +
                    $"opening {tuned.DiameterMm:F3} mm, contrast {tuned.MedianContrast:F1}, " +
                    $"max center drift {tuned.MaxCenterDeviationMm:F3} mm; {_cameraScale:F1} px/mm. " +
                    "Adaptive dark-opening detection locked for this run (not persisted).",
                    MachineOperationVerificationKind.VisionVerified, _cameraScale);

                // The fixed bottom camera sees the nozzle move WITH the
                // gantry. Use the same reverse-offset geometry as the manual
                // bottom-camera Jog; do not move until a stable, scale-locked
                // circle and current physical travel envelope are verified.
                var centerStep = Step("center-nozzle");
                centerStep.Begin("Observing the locked opening and centering with at most three bounded XY adjustments.");
                var tolerance = capture.Profile.ErrorToleranceMM > 0
                    ? Math.Min(capture.Profile.ErrorToleranceMM, 0.10) : 0.10;
                var previousDistance = Double.PositiveInfinity;
                var centered = false;
                for (var centerAttempt = 0; centerAttempt <= 3; centerAttempt++)
                {
                    if (!ValidContext(out contextError) ||
                        !await MotionIdleAsync(CancellationToken.None))
                    {
                        Stop(centerStep, contextError ?? "Machine was not idle before nozzle centering.");
                        return false;
                    }
                    var observation = await ObserveFreshCircleAsync(0, CancellationToken.None);
                    if (observation == null)
                    {
                        Stop(centerStep, "Could not reacquire the scale-locked dark opening for optical centering.");
                        return false;
                    }
                    var distance = Math.Sqrt(observation.Xmm * observation.Xmm +
                                             observation.Ymm * observation.Ymm);
                    if (centerAttempt > 0 && distance > previousDistance + 0.08)
                    {
                        Stop(centerStep, $"Optical centering moved the detected opening away from center " +
                            $"({previousDistance:F3} -> {distance:F3} mm). No further jogging or rotation.");
                        return false;
                    }
                    if (Math.Abs(observation.Xmm) <= tolerance &&
                        Math.Abs(observation.Ymm) <= tolerance)
                    {
                        Pass(centerStep,
                            $"Head {tool.HeadIndex} centered: X={observation.Xmm:+0.000;-0.000;0.000}, " +
                            $"Y={observation.Ymm:+0.000;-0.000;0.000} mm after {centerAttempt} correction(s).",
                            MachineOperationVerificationKind.VisionVerified, _cameraScale);
                        centered = true;
                        break;
                    }
                    if (centerAttempt == 3)
                    {
                        Stop(centerStep, $"Nozzle is still off-center after three bounded corrections: " +
                            $"X={observation.Xmm:F3}, Y={observation.Ymm:F3} mm.");
                        return false;
                    }

                    var before = await machine.GetCurrentLocationAsync(5000);
                    if (!before.Successful || before.Result == null ||
                        !ValidContext(out contextError) || machine.Busy)
                    {
                        Stop(centerStep, contextError ??
                            "Could not verify controller XY before optical centering; no movement sent.");
                        return false;
                    }
                    var correctionObservation = new Point2D<double>(
                        observation.Xmm, observation.Ymm);
                    if (!NozzleOpticalJogMath.TryPlan(
                        before.Result, correctionObservation, tool.Offset,
                        machine.Settings.WorkAreaSize, out var jog, out var jogReason))
                    {
                        Stop(centerStep, "Nozzle optical centering blocked: " + jogReason);
                        return false;
                    }
                    previousDistance = distance;
                    machine.AddStatusMessage(StatusMessageTypes.Info,
                        $"Runout head {tool.HeadIndex} centering {centerAttempt + 1}/3: " +
                        $"observed ({observation.Xmm:+0.000;-0.000;0.000}," +
                        $"{observation.Ymm:+0.000;-0.000;0.000}) mm, " +
                        $"jog ({jog.X:+0.000;-0.000;0.000},{jog.Y:+0.000;-0.000;0.000}) mm.");
                    // Stay at optical inspection height; do not alter Z.
                    machine.GotoPoint(jog, relativeMove: true);
                    if (!await MotionIdleAsync(CancellationToken.None))
                    {
                        Stop(centerStep, "Optical centering XY move did not settle; stopping before rotation.");
                        return false;
                    }
                    var after = await machine.GetCurrentLocationAsync(5000);
                    if (!after.Successful || after.Result == null ||
                        !ValidContext(out contextError) ||
                        Math.Abs(after.Result.X - before.Result.X - jog.X) > 0.15 ||
                        Math.Abs(after.Result.Y - before.Result.Y - jog.Y) > 0.15)
                    {
                        Stop(centerStep, contextError ??
                            "Controller XY did not confirm the requested centering displacement. No further motion.");
                        return false;
                    }
                    capture.ResetDetections();
                }
                if (!centered)
                {
                    Stop(centerStep, "The verified dark opening was not centered; rotation has not started.");
                    return false;
                }

                var samples = new List<NozzleRunoutSample>();

                foreach (var angle in NozzleRunoutMath.SweepAngles())
                {
                    var step=Step("angle-" + angle);
                    step.Begin($"Rotating head {tool.HeadIndex} to {angle}°; waiting for new stable bottom-camera circle.");
                    if (!ValidContext(out contextError) ||
                        !await MotionIdleAsync(CancellationToken.None))
                    {
                        Stop(step, contextError ?? "Machine/vision context did not settle.");
                        return false;
                    }

                    TargetAngle = angle;
                    machine.SetAbsoluteMode();
                    machine.RotateToolHeadAngleAbsolute(angle);
                    if (!await MotionIdleAsync(CancellationToken.None) ||
                        !ValidContext(out contextError))
                    {
                        Stop(step, contextError ?? "Rotary motion did not settle.");
                        return false;
                    }

                    NozzleRunoutSample measured=null;
                    for(var attempt=0; attempt<3 && measured==null; attempt++)
                    {
                        step.RetryCount=attempt;
                        measured=await ObserveFreshCircleAsync(angle,CancellationToken.None);
                    }
                    if (measured==null)
                    {
                        Stop(step,$"No stable nozzle circle at {angle}° after 3 observation attempts. Check focus/lighting/profile.");
                        return false;
                    }
                    samples.Add(measured);
                    CurrentCalibrationAngle=angle;
                    var detail=$"X {measured.Xmm:+0.000;-0.000;0.000} Y {measured.Ymm:+0.000;-0.000;0.000} mm at {measured.PixelsPerMm:F1} px/mm";
                    CalibrationUpdates.Add($"{angle:000}°  {detail}");
                    Pass(step,detail,MachineOperationVerificationKind.VisionVerified,measured.PixelsPerMm);
                }

                var result=NozzleRunoutMath.Analyze(samples);
                if (result.MaxRadialDeviationMm > 2.0)
                {
                    Stop(Step("high-point"),
                        $"Measured nozzle deviation {result.MaxRadialDeviationMm:F3} mm exceeds 2 mm sanity limit.");
                    return false;
                }
                var high=Step("high-point");
                high.Begin($"Revisiting maximum-Y measured at commanded {result.LogicalZeroAngle}°.");
                machine.SetAbsoluteMode();
                machine.RotateToolHeadAngleAbsolute(result.LogicalZeroAngle);
                if (!await MotionIdleAsync(CancellationToken.None) ||
                    !ValidContext(out contextError))
                {
                    Stop(high,contextError ?? "High-point rotation failed to settle.");
                    return false;
                }

                var second=await ObserveFreshCircleAsync(result.LogicalZeroAngle,CancellationToken.None);
                var expected=samples.Single(s=>s.CommandedAngle==result.LogicalZeroAngle);
                if (!NozzleRunoutMath.IsRepeatable(expected,second,0.1))
                {
                    Stop(high,"High-point repeatability failed (greater than 0.1 mm difference or no stable circle).");
                    return false;
                }
                Pass(high,$"Logical zero candidate at {result.LogicalZeroAngle}°; repeat XY within 0.1 mm.",
                    MachineOperationVerificationKind.VisionVerified,second.PixelsPerMm);

                var resultStep=Step("result");
                resultStep.Begin("Calculating mean-centered compensation and logical zero; no firmware re-zero.");
                CandidateResult=result;
                RaisePropertyChanged(nameof(CandidateZero));
                RaisePropertyChanged(nameof(RunoutSummary));
                Pass(resultStep,"Twelve calibrated points computed. Preview only; no machine settings or placement compensation were changed.",
                    MachineOperationVerificationKind.VisionVerified,_cameraScale);
                // Valid only for this specific installed nozzle/head and for
                // this application session. Do not write G92 or persist the
                // offset table to machine configuration.
                if (!ValidContext(out contextError))
                {
                    Stop(resultStep, contextError ?? "Nozzle changed before calibration could be committed.");
                    return false;
                }
                var corrections = result.Samples.Select(sample => new RotationOffset
                {
                    Angle = sample.CommandedAngle,
                    Offset = NozzleRunoutMath.Correction(result, sample.CommandedAngle)
                }).ToList();
                tool.CompleteRunoutCalibration(_activeNozzleId, result.LogicalZeroAngle, corrections);
                _lastMeasuredHead = tool;
                Status = $"Head {tool.HeadIndex} ({tool.CurrentNozzle.Text}) runout calibrated for this session; 12 offsets and high-Y zero validated.";

                // Only after the final installed nozzle (normally Head 2 /
                // right head) has passed all twelve angles and high-point
                // repeatability, return to the machine's EXISTING configured
                // safe move height. Do not capture or change that setting.
                if (isFinalHead)
                {
                    var safeHeightStep = Step("safe-height");
                    safeHeightStep.Begin(
                        $"Moving to the configured machine safe move height ({machine.Settings.SafMoveHeight:F2} mm).");

                    if (!ValidContext(out contextError) ||
                        !await MotionIdleAsync(CancellationToken.None))
                    {
                        Stop(safeHeightStep, contextError ??
                            "Machine was not idle before the final safe-height move.");
                        return false;
                    }

                    var targetSafeHeight = machine.Settings.SafMoveHeight;
                    if (Double.IsNaN(targetSafeHeight) || Double.IsInfinity(targetSafeHeight))
                    {
                        Stop(safeHeightStep,
                            "Configured machine safe move height is not a valid finite value; no final Z move sent.");
                        return false;
                    }

                    machine.SendSafeMoveHeight();
                    if (!await MotionIdleAsync(CancellationToken.None))
                    {
                        Stop(safeHeightStep,
                            "Machine did not settle after the final safe move height command.");
                        return false;
                    }

                    // M114 refreshes the controller-reported position; the
                    // parser updates ToolCommonZ for both supported PnP
                    // firmware paths. Never mark this step passed on queue
                    // completion alone.
                    var finalPosition = await machine.GetCurrentLocationAsync(5000);
                    if (!finalPosition.Successful ||
                        !ValidContext(out contextError) ||
                        Math.Abs(machine.ToolCommonZ - targetSafeHeight) > 0.20)
                    {
                        Stop(safeHeightStep, contextError ??
                            $"Safe-height verification failed. Expected Z={targetSafeHeight:F2} mm; " +
                            $"controller reports Z={machine.ToolCommonZ:F2} mm.");
                        return false;
                    }

                    PassMachineStep(safeHeightStep,
                        $"Machine reached its existing configured safe move height: " +
                        $"Z={machine.ToolCommonZ:F2} mm. Setting unchanged; procedure stopped here.");
                }
                return true;
        }

    }
}
