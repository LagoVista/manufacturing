using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
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
        private string _activeNozzleId;
        private string _cameraId;
        private double _cameraScale;
        private bool _running;
        private bool _finished;
        private string _status = "Ready. Select a tool head with an installed nozzle. The bottom camera must be running.";
        private int _targetAngle;
        private int _currentAngle;
        private NozzleRunoutResult _result;

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
        public string ToolHeadName => _machineRepo.CurrentMachine?.CurrentMachineToolHead?.Name ?? "No tool selected";
        public string NozzleName => _machineRepo.CurrentMachine?.CurrentMachineToolHead?.CurrentNozzle?.Text ?? "No nozzle";
        public int TargetAngle { get => _targetAngle; set => Set(ref _targetAngle, value); }
        public int CurrentCalibrationAngle { get => _currentAngle; set => Set(ref _currentAngle, value); }
        public NozzleRunoutResult CandidateResult { get => _result; private set => Set(ref _result, value); }
        public string CandidateZero => CandidateResult == null
            ? "Not yet measured" : $"{CandidateResult.LogicalZeroAngle}° (logical zero, not firmware G92)";
        public string RunoutSummary => CandidateResult == null
            ? "No calibrated runout table. Placement compensation remains unchanged."
            : $"Peak radial deviation: {CandidateResult.MaxRadialDeviationMm:F3} mm | " +
              $"X span: {CandidateResult.RangeXmm:F3} mm | Y span: {CandidateResult.RangeYmm:F3} mm";

        private static ObservableCollection<FeederCalibrationFlowStep> NewPlan()
        {
            var steps = new ObservableCollection<FeederCalibrationFlowStep>();
            void Add(string id, string title) => steps.Add(new FeederCalibrationFlowStep
            {
                Number=steps.Count+1, StepKey=id, Title=title
            });
            Add("position", "Position installed nozzle over bottom camera");
            foreach (var angle in NozzleRunoutMath.SweepAngles())
                Add("angle-" + angle, $"Measure tip center at {angle:000}°");
            Add("high-point", "Revisit maximum-Y point and verify repeatability");
            Add("result", "Calculate logical zero and unsaved compensation preview");
            return steps;
        }

        private FeederCalibrationFlowStep Step(string key) =>
            RunoutSteps.First(x => x.StepKey == key);

        private bool ValidContext(out string reason)
        {
            reason = null;
            var m = _activeMachine;
            if (m == null || !m.Connected || m.Mode != OperatingMode.Manual ||
                !m.WasMachineHomed || !m.WasMachinOriginCalibrated || m.CurrentMachineToolHead == null ||
                !ReferenceEquals(m.CurrentMachineToolHead, _activeHead) ||
                !String.Equals(Convert.ToString(_activeHead?.CurrentNozzle?.Id),
                    _activeNozzleId, StringComparison.Ordinal) ||
                !ReferenceEquals(_machineRepo.CurrentMachine, m))
            {
                reason = "Machine is not homed/connected/Manual or the selected tool head/nozzle changed.";
                return false;
            }

            var camera = m.PartInspectionCaptureService;
            if (camera?.Camera == null || camera.Camera.Id.ToString() != _cameraId ||
                camera.CameraType != CameraTypes.PartInspection ||
                camera.Profile?.Key != VisionProfile.VisionProfile_Nozzle ||
                !camera.Profile.FindCircles ||
                camera.Camera.EffectivePixelsPerMM <= 0 ||
                Math.Abs(camera.Camera.EffectivePixelsPerMM - _cameraScale) > 0.1)
            {
                reason = "Bottom camera, Nozzle profile or calibrated pixels/mm changed.";
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
            Status = "Stopped: " + reason;
            return false;
        }

        public async Task StartAsync()
        {
            if (RunoutRunning) return;
            RunoutSteps = NewPlan();
            RaisePropertyChanged(nameof(RunoutSteps));
            CalibrationUpdates.Clear();
            CandidateResult = null;
            RaisePropertyChanged(nameof(CandidateZero));
            RaisePropertyChanged(nameof(RunoutSummary));
            RunoutFinished = false;
            RunoutRunning = true;
            Status = "Starting supervised nozzle runout sweep.";

            try
            {
                var machine = _machineRepo.CurrentMachine;
                var tool = machine?.CurrentMachineToolHead;
                if (tool == null || tool.HeadIndex < 1 || tool.HeadIndex > 2 ||
                    String.IsNullOrWhiteSpace(Convert.ToString(tool.CurrentNozzle?.Id)))
                {
                    Stop(Step("position"), "Select a tool head (1 or 2) with an installed nozzle first.");
                    return;
                }
                if (machine == null || !machine.Connected ||
                    machine.Mode != OperatingMode.Manual || !machine.WasMachineHomed ||
                    !machine.WasMachinOriginCalibrated || machine.Busy)
                {
                    Stop(Step("position"), "Machine must be connected, homed, idle and in Manual mode.");
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
                    Stop(Step("position"), "No configured/active bottom camera or unsafe camera XY.");
                    return;
                }

                _activeMachine = machine;
                _activeHead = tool;
                _activeNozzleId = Convert.ToString(tool.CurrentNozzle.Id);
                _cameraId = machine.PartInspectionCaptureService.Camera.Id.ToString();
                machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_Nozzle);
                _cameraScale = machine.PartInspectionCaptureService.Camera.EffectivePixelsPerMM;
                RaisePropertyChanged(nameof(ToolHeadName));
                RaisePropertyChanged(nameof(NozzleName));

                if (!ValidContext(out var contextError))
                {
                    Stop(Step("position"), contextError +
                        " Start the bottom camera and configure the Nozzle circle profile.");
                    return;
                }

                var positionStep = Step("position");
                positionStep.Begin("Raising to safe height, moving XY over bottom camera, then lowering selected nozzle to detection height.");
                machine.SendSafeMoveHeight();
                if (!await MotionIdleAsync(CancellationToken.None, 10000))
                {
                    Stop(positionStep, "Safe-height move did not reach idle.");
                    return;
                }

                await machine.GoToPartInspectionCameraAsync();
                if (!await MotionIdleAsync(CancellationToken.None) ||
                    !ValidContext(out contextError))
                {
                    Stop(positionStep, contextError ?? "Bottom camera approach did not settle.");
                    return;
                }
                var cameraXY = await machine.GetCurrentLocationAsync(5000);
                if (!cameraXY.Successful ||
                    !FeederOriginCalibrationMath.SamePosition(
                        cameraXY.Result, cameraConfig.AbsolutePosition, 0.5))
                {
                    Stop(positionStep, "Machine was not at the configured bottom-camera location.");
                    return;
                }
                positionStep.Pass($"Head {tool.HeadIndex}, nozzle {_activeNozzleId}, {_cameraScale:F1} px/mm.");
                var samples = new List<NozzleRunoutSample>();

                foreach (var angle in NozzleRunoutMath.SweepAngles())
                {
                    var step=Step("angle-" + angle);
                    step.Begin($"Rotating head {tool.HeadIndex} to {angle}°; waiting for new stable bottom-camera circle.");
                    if (!ValidContext(out contextError) ||
                        !await MotionIdleAsync(CancellationToken.None))
                    {
                        Stop(step, contextError ?? "Machine/vision context did not settle.");
                        return;
                    }

                    TargetAngle = angle;
                    machine.RotateToolHeadAngleAbsolute(angle);
                    if (!await MotionIdleAsync(CancellationToken.None) ||
                        !ValidContext(out contextError))
                    {
                        Stop(step, contextError ?? "Rotary motion did not settle.");
                        return;
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
                        return;
                    }
                    samples.Add(measured);
                    CurrentCalibrationAngle=angle;
                    var detail=$"X {measured.Xmm:+0.000;-0.000;0.000} Y {measured.Ymm:+0.000;-0.000;0.000} mm at {measured.PixelsPerMm:F1} px/mm";
                    CalibrationUpdates.Add($"{angle:000}°  {detail}");
                    step.Pass(detail);
                }

                var result=NozzleRunoutMath.Analyze(samples);
                if (result.MaxRadialDeviationMm > 2.0)
                {
                    Stop(Step("high-point"),
                        $"Measured nozzle deviation {result.MaxRadialDeviationMm:F3} mm exceeds 2 mm sanity limit.");
                    return;
                }
                var high=Step("high-point");
                high.Begin($"Revisiting maximum-Y measured at commanded {result.LogicalZeroAngle}°.");
                machine.RotateToolHeadAngleAbsolute(result.LogicalZeroAngle);
                if (!await MotionIdleAsync(CancellationToken.None) ||
                    !ValidContext(out contextError))
                {
                    Stop(high,contextError ?? "High-point rotation failed to settle.");
                    return;
                }

                var second=await ObserveFreshCircleAsync(result.LogicalZeroAngle,CancellationToken.None);
                var expected=samples.Single(s=>s.CommandedAngle==result.LogicalZeroAngle);
                if (!NozzleRunoutMath.IsRepeatable(expected,second,0.1))
                {
                    Stop(high,"High-point repeatability failed (greater than 0.1 mm difference or no stable circle).");
                    return;
                }
                high.Pass($"Logical zero candidate at {result.LogicalZeroAngle}°; repeat XY within 0.1 mm.");

                var resultStep=Step("result");
                resultStep.Begin("Calculating mean-centered compensation and logical zero; no firmware re-zero.");
                CandidateResult=result;
                RaisePropertyChanged(nameof(CandidateZero));
                RaisePropertyChanged(nameof(RunoutSummary));
                resultStep.Pass("Twelve calibrated points computed. Preview only; no machine settings or placement compensation were changed.");
                Status="Completed optical pilot: high-Y logical zero and runout curve verified. Data is unsaved pending Windows machine acceptance.";
            }
            catch(Exception ex)
            {
                var step=RunoutSteps.FirstOrDefault(s=>s.Status=="Running");
                Stop(step,"Nozzle runout procedure error: " + ex.Message);
            }
            finally
            {
                lock (_observationGate) _pendingSample=null;
                _activeMachine=null;
                _activeHead=null;
                RunoutRunning=false;
                RunoutFinished=true;
            }
        }
    }
}
