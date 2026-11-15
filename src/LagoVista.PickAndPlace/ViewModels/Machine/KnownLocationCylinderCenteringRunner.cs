using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    /// <summary>
    /// The first supervised T003 machine test: reuses the operator's existing
    /// Known Location and its position-camera circle profile. Finds the 7 mm
    /// calibration cylinder, safely jogs only if necessary, then records the
    /// measured center. Does NOT pick anything, change the known point,
    /// change a tool offset, or alter any camera/profile settings.
    /// </summary>
    internal sealed class KnownLocationCylinderCenteringRunner : ICircleLocatedHandler
    {
        private readonly IMachineRepo _repo;
        private readonly ILocatorViewModel _locator;
        private readonly object _observationGate = new object();
        private TaskCompletionSource<CircleObservation> _pending;
        private IMachine _machine;
        private string _cameraId;
        private double _scale;

        private sealed class CircleObservation
        {
            public Point2D<double> OffsetPixels { get; set; }
            public double DiameterPixels { get; set; }
        }

        public bool Running { get; private set; }
        public string Status { get; private set; } = "Ready to verify the cylinder at the configured Known Location.";

        public KnownLocationCylinderCenteringRunner(IMachineRepo machineRepo,
            ILocatorViewModel locator)
        {
            _repo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _locator = locator ?? throw new ArgumentNullException(nameof(locator));
        }

        public async Task<InvokeResult<Point2D<double>>> RunAsync()
        {
            if (Running)
                return InvokeResult<Point2D<double>>.FromError("Cylinder centering is already running.");

            Running = true;
            var machine = _repo.CurrentMachine;
            _machine = machine;
            try
            {
                if (machine?.Settings == null || !machine.Connected ||
                    machine.Mode != OperatingMode.Manual ||
                    !machine.WasMachineHomed || !machine.WasMachinOriginCalibrated)
                    return Failure("Requires a connected, homed, origin-calibrated machine in Manual mode.");

                var known = machine.Settings.KnownCalibrationPoint;
                if (known == null ||
                    !Valid(known.X) || !Valid(known.Y) ||
                    (known.X == 0 && known.Y == 0))
                    return Failure("Set the existing Known Location before verifying the cylinder.");

                var capture = machine.PositionImageCaptureService;
                if (capture?.Camera == null ||
                    capture.CameraType != CameraTypes.Position ||
                    capture.CaptureFrameSize.Width <= 0 ||
                    capture.CaptureFrameSize.Height <= 0)
                    return Failure("Position-camera stream is unavailable. Start live capture first.");

                // Moving-camera operation. Never inherit a selected tool's G92.
                machine.SendSafeMoveHeight();
                await machine.MoveToCameraAsync();
                var safe = await machine.SpinUntilIdleAsync(10000);
                if (!safe.Successful || machine.CurrentMachineToolHead != null)
                    return Failure("Could not establish safe camera navigation.");

                machine.SetVisionProfile(CameraTypes.Position, VisionProfile.VisionProfile_KnownLocation);
                if (capture.Profile?.Key != VisionProfile.VisionProfile_KnownLocation ||
                    !capture.Profile.FindCircles)
                    return Failure("Existing Known Location camera profile is not active with circle detection.");

                _cameraId = Convert.ToString(capture.Camera.Id);
                _scale = capture.Camera.EffectivePixelsPerMM;
                if (!Valid(_scale) || _scale <= 0)
                    return Failure("Position camera has no usable calibrated pixel scale.");

                Status = $"Moving position camera to configured Known Location X={known.X:F3}, Y={known.Y:F3}.";
                machine.GotoPoint(known);
                var arrived = await machine.SpinUntilIdleAsync(20000);
                if (!arrived.Successful)
                    return Failure("Could not reach Known Location: " + arrived.ErrorMessage);

                for (var attempt = 0; attempt < KnownLocationCylinderCenteringMath.MaximumCenteringAttempts; attempt++)
                {
                    if (!ContextValid())
                        return Failure("Machine, selected camera, profile or optical scale changed during centering.");

                    // Clear previous detection history after each movement; never
                    // accept yesterday's stable circle as evidence for this move.
                    capture.ResetDetections();
                    var request = new TaskCompletionSource<CircleObservation>(
                        TaskCreationOptions.RunContinuationsAsynchronously);
                    lock (_observationGate)
                        _pending = request;
                    _locator.RegisterCircleLocatedHandler(this, 10000);

                    CircleObservation observation;
                    try
                    {
                        var completed = await Task.WhenAny(request.Task, Task.Delay(11000));
                        observation = completed == request.Task ? await request.Task : null;
                    }
                    finally
                    {
                        _locator.UnregisterCircleLocatedHandler(this);
                        lock (_observationGate)
                        {
                            if (_pending == request)
                                _pending = null;
                        }
                    }

                    if (observation == null)
                        return Failure("No stable 7 mm cylinder circle at Known Location within 10 seconds.");
                    if (!ContextValid())
                        return Failure("Machine or Known Location vision profile changed after circle measurement.");

                    var position = await machine.GetCurrentLocationAsync(5000);
                    if (!position.Successful)
                        return Failure("Could not read camera position after circle detection.");

                    var physicalXY = new Point2D<double>(position.Result.X, position.Result.Y);
                    if (!KnownLocationCylinderCenteringMath.TryPlan(
                        physicalXY, known, observation.OffsetPixels,
                        observation.DiameterPixels, _scale, true,
                        machine.Settings.WorkAreaSize, out var jog, out var actual,
                        out var diameterMm, out var reason))
                        return Failure(reason);

                    if (jog.X == 0 && jog.Y == 0)
                    {
                        Status = $"Cylinder CENTERED: X={actual.X:F3}, Y={actual.Y:F3}, " +
                                 $"D={diameterMm:F2} mm, after {attempt} corrections. " +
                                 "Known Location and tool offsets were not changed.";
                        return InvokeResult<Point2D<double>>.Create(actual);
                    }

                    if (attempt == KnownLocationCylinderCenteringMath.MaximumCenteringAttempts - 1)
                        return Failure("Cylinder could not be centered within the bounded correction attempts.");

                    Status = $"Cylinder offset X={jog.X:+0.000;-0.000;0.000}, " +
                             $"Y={jog.Y:+0.000;-0.000;0.000} mm. Bounded jog " +
                             $"{attempt + 1} of {KnownLocationCylinderCenteringMath.MaximumCenteringAttempts - 1}.";
                    machine.GotoPoint(jog, relativeMove: true);
                    var moved = await machine.SpinUntilIdleAsync(10000);
                    if (!moved.Successful)
                        return Failure("Cylinder centering jog failed: " + moved.ErrorMessage);
                }
                return Failure("Cylinder circle could not be centered.");
            }
            catch (Exception ex)
            {
                return Failure("Cylinder centering stopped: " + ex.Message);
            }
            finally
            {
                _locator.UnregisterCircleLocatedHandler(this);
                lock (_observationGate) _pending = null;
                Running = false;
                _machine = null;
            }
        }

        private bool ContextValid()
        {
            var m = _machine;
            var capture = m?.PositionImageCaptureService;
            return m != null && ReferenceEquals(_repo.CurrentMachine, m) &&
                m.Connected && m.Mode == OperatingMode.Manual &&
                m.WasMachineHomed && m.WasMachinOriginCalibrated &&
                !m.Busy && m.CurrentMachineToolHead == null &&
                capture?.Camera != null &&
                capture.CameraType == CameraTypes.Position &&
                capture.Profile?.Key == VisionProfile.VisionProfile_KnownLocation &&
                capture.Profile.FindCircles &&
                String.Equals(Convert.ToString(capture.Camera.Id), _cameraId, StringComparison.Ordinal) &&
                Math.Abs(capture.Camera.EffectivePixelsPerMM - _scale) < 0.001;
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            if (circle == null || circle.CameraType != CameraTypes.Position ||
                !circle.Stabilized || !circle.PositionStable || !ContextValid())
                return;

            TaskCompletionSource<CircleObservation> request;
            lock (_observationGate) request = _pending;
            if (request == null) return;
            var offset = circle.OffsetPixels;
            if (offset == null || !Valid(offset.X) || !Valid(offset.Y) ||
                !Valid(circle.RadiusPixels) || circle.RadiusPixels <= 0)
                return;
            request.TrySetResult(new CircleObservation
            {
                OffsetPixels = new Point2D<double>(offset.X, offset.Y),
                DiameterPixels = 2 * circle.RadiusPixels
            });
        }

        public void CircleLocatorTimeout()
        {
            lock (_observationGate) _pending?.TrySetResult(null);
        }

        public void CircleLocatorAborted()
        {
            lock (_observationGate) _pending?.TrySetResult(null);
        }

        private InvokeResult<Point2D<double>> Failure(string reason)
        {
            Status = reason;
            _machine?.AddStatusMessage(StatusMessageTypes.Warning, reason);
            return InvokeResult<Point2D<double>>.FromError(reason);
        }

        private static bool Valid(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);
    }
}
