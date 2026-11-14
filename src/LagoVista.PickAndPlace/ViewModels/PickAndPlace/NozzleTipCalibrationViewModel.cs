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
    public class NozzleTipCalibrationViewModel : ViewModelBase,
        INozzleTipCalibrationViewModel, ICircleLocatedHandler
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILocatorViewModel _locator;
        private readonly object _observationGate = new object();
        private TaskCompletionSource<NozzleRunoutSample> _pendingSample;
        private IMachine _activeMachine;
        private MachineToolHead _activeHead;
        private MachineToolHead _lastMeasuredHead;
        private string _activeNozzleId;
        private string _cameraId;
        private double _cameraScale;
        private bool _running;
        private bool _finished;
        private string _status = "Ready. Installed nozzles are discovered from the machine tool-head assignments. The bottom camera must be running.";
        private int _targetAngle;
        private int _currentAngle;
        private NozzleRunoutResult _result;
        private MachineOperationSession _session;

        public NozzleTipCalibrationViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _locator = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
            CalibrateBottomCameraCommand = new RelayCommand(async () => await StartAsync());
        }

        public ObservableCollection<FeederCalibrationFlowStep> RunoutSteps { get; private set; } =
            new ObservableCollection<FeederCalibrationFlowStep>();
        public ObservableCollection<string> CalibrationUpdates { get; } = new ObservableCollection<string>();
        public RelayCommand CalibrateBottomCameraCommand { get; }
        public bool RunoutRunning { get => _running; private set => Set(ref _running, value); }
        public bool RunoutFinished { get => _finished; private set => Set(ref _finished, value); }
        public string Status { get => _status; private set => Set(ref _status, value); }
        private MachineToolHead DisplayHead
        {
            get
            {
                var machine = _machineRepo.CurrentMachine;
                return _activeHead ?? _lastMeasuredHead ?? InstalledNozzleCalibrationPlan.ChooseFirst(
                    InstalledNozzleCalibrationPlan.InstalledHeads(machine?.Settings),
                    machine?.CurrentMachineToolHead);
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
            foreach (var head in installed)
            {
                var prefix = $"head-{head.HeadIndex}/";
                foreach (var step in NozzleRunoutProcedureDefinition.Create().Steps)
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
            if (pending == null || circle == null ||
                circle.CameraType != CameraTypes.PartInspection ||
                !circle.Stabilized || !circle.PositionStable || circle.RadiusMM <= 0 ||
                !ValidContext(out _)) return;

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

        /// <summary>
        /// Machine startup / manual calibration procedure. Iterates every
        /// configured, installed nozzle head. Never auto-moves on mere
        /// connection: the supervised modal invokes this after homing.
        /// </summary>
        public async Task StartAsync()
        {
            if (RunoutRunning) return;
            var machine = _machineRepo.CurrentMachine;
            var installed = InstalledNozzleCalibrationPlan.InstalledHeads(machine?.Settings);
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
            Status = $"Starting runout checks for {installed.Count} installed nozzle head(s).";
            try
            {
                if (installed.Count == 0)
                {
                    Status = "Stopped: no installed nozzles assigned to Head 1 or Head 2.";
                    return;
                }
                _activeHead = installed[0];
                RaisePropertyChanged(nameof(ToolHeadName));
                RaisePropertyChanged(nameof(NozzleName));
                if (machine == null || !machine.Connected ||
                    machine.Mode != OperatingMode.Manual || !machine.WasMachineHomed ||
                    !machine.WasMachinOriginCalibrated || machine.Busy)
                {
                    Stop(Step("position"),
                        "Machine must be connected, homed, idle and in Manual mode.");
                    return;
                }
                var cameraConfig = machine.Settings?.Cameras?.FirstOrDefault(c =>
                    c.CameraType.Value == CameraTypes.PartInspection);
                if (cameraConfig?.AbsolutePosition == null ||
                    cameraConfig.AbsolutePosition.X < 0 || cameraConfig.AbsolutePosition.Y < 0 ||
                    cameraConfig.AbsolutePosition.X > machine.Settings.WorkAreaSize.X ||
                    cameraConfig.AbsolutePosition.Y > machine.Settings.WorkAreaSize.Y ||
                    machine.PartInspectionCaptureService?.Camera == null)
                {
                    Stop(Step("position"), "Bottom camera is unavailable or its configured XY is outside the work area.");
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
                    if (!await CalibrateInstalledHeadAsync(machine, tool, cameraConfig))
                        return;
                }
                Status = $"All {installed.Count} installed nozzle head(s) calibrated this session. Measurements are invalidated on machine restart or nozzle changes; production compensation remains separate.";
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
                _session = null;
                RunoutRunning = false;
                RunoutFinished = true;
                RaisePropertyChanged(nameof(ToolHeadName));
                RaisePropertyChanged(nameof(NozzleName));
            }
        }

        private async Task<bool> CalibrateInstalledHeadAsync(
            IMachine machine, MachineToolHead tool, MachineCamera cameraConfig)
        {
                _activeMachine = machine;
                _activeHead = tool;
                _activeNozzleId = Convert.ToString(tool.CurrentNozzle.Id);
                _cameraId = machine.PartInspectionCaptureService.Camera.Id.ToString();
                machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_Nozzle);
                _cameraScale = machine.PartInspectionCaptureService.Camera.EffectivePixelsPerMM;
                RaisePropertyChanged(nameof(ToolHeadName));
                RaisePropertyChanged(nameof(NozzleName));

                // The configuration tells us which nozzle is installed.
                // Navigation selection is a separate runtime state. Select
                // the installed head through the existing awaited safe
                // tool-change path, rather than demanding a prior UI click.
                _session=new MachineOperationSession(
                    NozzleRunoutProcedureDefinition.Create(),
                    new MachineOperationTarget
                    {
                        MachineId=Convert.ToString(machine.Settings.Id),
                        CameraId=_cameraId
                    },DateTimeOffset.UtcNow);
                _session.Start(DateTimeOffset.UtcNow);

                var positionStep = Step("position");
                positionStep.Begin($"Preflighting Head {tool.HeadIndex}, nozzle {tool.CurrentNozzle.Text}, bottom camera and XY targets before motion.");

                // The prior implementation selected the tool head and queued
                // a relative XY move before checking the camera. That caused
                // unplanned machine movement even with an invalid profile.
                if (!ValidCameraBeforeMotion(machine, cameraConfig, out var cameraError))
                {
                    Stop(positionStep, "No movement sent: " + cameraError);
                    return false;
                }

                // This only reads the current controller XY (M114); it does
                // not command any axes. Check BOTH the existing relative
                // tool-selection move and the later bottom-camera target.
                var location = await machine.GetCurrentLocationAsync(5000);
                if (!location.Successful || location.Result == null)
                {
                    Stop(positionStep, "No movement sent: controller XY could not be read.");
                    return false;
                }
                var needsSelection = !ReferenceEquals(machine.CurrentMachineToolHead, tool);
                if (!NozzleRunoutMotionPreflight.TryValidate(
                    location.Result, tool.Offset, needsSelection,
                    cameraConfig.AbsolutePosition, machine.Settings.WorkAreaSize,
                    out var geometryError))
                {
                    Stop(positionStep, "No movement sent: " + geometryError);
                    return false;
                }
                machine.AddStatusMessage(StatusMessageTypes.Info,
                    $"Nozzle calibration XY preflight passed: " +
                    $"head {tool.HeadIndex} offset ({tool.Offset?.X:F2}, {tool.Offset?.Y:F2}), " +
                    $"current ({location.Result.X:F2}, {location.Result.Y:F2}), " +
                    $"camera ({cameraConfig.AbsolutePosition.X:F2}, {cameraConfig.AbsolutePosition.Y:F2}), " +
                    $"work area ({machine.Settings.WorkAreaSize.X:F2}, {machine.Settings.WorkAreaSize.Y:F2}).");
                positionStep.Detail = "Validated live bottom camera, Nozzle profile, pixels/mm, and bounded XY moves. Selecting head.";
                if (needsSelection)
                {
                    // MoveToToolHeadAsync performs safe-Z and its coordinate
                    // change internally. Never bypass it by assigning an
                    // unselected tool head as though navigation had changed.
                    await machine.MoveToToolHeadAsync(tool);
                    if (!ReferenceEquals(machine.CurrentMachineToolHead, tool) ||
                        !await MotionIdleAsync(CancellationToken.None))
                    {
                        Stop(positionStep,
                            $"Could not safely select Head {tool.HeadIndex} with nozzle {tool.CurrentNozzle.Text}. Machine navigation did not settle.");
                        return false;
                    }
                }
                if (!ValidContext(out var contextError))
                {
                    Stop(positionStep, contextError +
                        " Start the bottom camera and configure the Nozzle circle profile.");
                    return false;
                }

                machine.SetAbsoluteMode();
                machine.SendSafeMoveHeight();
                if (!await MotionIdleAsync(CancellationToken.None, 10000))
                {
                    Stop(positionStep, "Safe-height move did not reach idle.");
                    return false;
                }

                await machine.GoToPartInspectionCameraAsync();
                if (!await MotionIdleAsync(CancellationToken.None) ||
                    !ValidContext(out contextError))
                {
                    Stop(positionStep, contextError ?? "Bottom camera approach did not settle.");
                    return false;
                }
                var cameraXY = await machine.GetCurrentLocationAsync(5000);
                if (!cameraXY.Successful ||
                    !FeederOriginCalibrationMath.SamePosition(
                        cameraXY.Result, cameraConfig.AbsolutePosition, 0.5))
                {
                    Stop(positionStep, "Machine was not at the configured bottom-camera location.");
                    return false;
                }
                Pass(positionStep,$"Head {tool.HeadIndex}, nozzle {_activeNozzleId}, {_cameraScale:F1} px/mm.",
                    MachineOperationVerificationKind.MachineObserved,_cameraScale);
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
                return true;
        }

    }
}
