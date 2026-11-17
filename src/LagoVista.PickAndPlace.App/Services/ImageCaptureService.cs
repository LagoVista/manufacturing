// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9b0fa03d4c6f50ced258c2080b20ba6d7736e2198efe1def70c5c368ef1fabf6
// IndexVersion: 2
// --- END CODE INDEX META ---
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.Services
{
    public class ImageCaptureService : MachineViewModelBase, IImageCaptureService
    {
        private bool _running;
        private DateTimeOffset _lastFrameUtc;

        // A connected device without fresh processed images is NOT ready.
        public bool IsCapturing => _running && _capture != null &&
            _lastFrameUtc != default(DateTimeOffset) &&
            (DateTimeOffset.UtcNow - _lastFrameUtc).TotalSeconds <= 3.0;

        public async Task<InvokeResult> EnsureCaptureReadyAsync()
        {
            if (Camera == null)
                return InvokeResult.FromError("Bottom-camera service is not bound to the machine.");
            if (EntityHeader.IsNullOrEmpty(Camera.CameraDevice))
                return InvokeResult.FromError("The configured bottom camera has no connected capture device.");
            if (IsCapturing)
                return InvokeResult.Success;
            if (_capture == null)
                StartCapture(); // Existing WPF device initialization.
            for (var poll = 0; poll < 80; poll++)
            {
                if (IsCapturing && CaptureFrameSize.Width > 0 &&
                    CaptureFrameSize.Height > 0)
                    return InvokeResult.Success;
                await Task.Delay(100);
            }
            return InvokeResult.FromError(
                "Bottom camera did not produce a fresh frame within 8 seconds. " +
                "Check device connection; no nozzle movement will be sent.");
        }
        public System.Drawing.Size CaptureFrameSize { get; private set; }

        private long _processedRectangleFrameId;
        private long _lastRectangleDetectedFrameId;
        private PocketRectangleFrameObservation _latestRectangleFrame;
        public PocketRectangleFrameObservation LatestRectangleFrame =>
            System.Threading.Volatile.Read(ref _latestRectangleFrame);

        public bool? FeederBarcodeVertical { get; set; }
        private double _lastBrightness = -9999;
        private double _lastFocus = -9999;
        private double _lastExposure = -9999;
        private double _lastTopContrast = -9999;
        private const double AutoTuneExposureMinimum = -10.0;
        private const double AutoTuneExposureMaximum = 10.0;
        private const double AutoTuneInitialStep = 1.0;
        private const double AutoTuneMinimumStep = 0.1;
        private const double AutoTuneDarkTarget = 64.0;
        private const double AutoTuneDarkTolerance = 8.0;
        private const double AutoTuneWhiteMinimum = 225.0;
        private const double AutoTuneWhiteCeiling = 248.0;
        private const int AutoTuneSettleFrames = 4;
        private const int AutoTuneSampleFrames = 10;
        private const int AutoTuneMaximumIterations = 24;

        private bool _autoTuneExposureRequested;
        private TaskCompletionSource<InvokeResult> _autoTuneExposureCompletion;
        private bool _nozzleTuneRequested;
        private TaskCompletionSource<InvokeResult<DarkCircularOpeningLocator.TuneResult>> _nozzleTuneCompletion;
        VideoCapture _capture;

        private object _captureLocker = new object();
        private TaskCompletionSource<byte[]> _pendingBarcodeFrame;

        IShapeDetectorService<Image<Bgr, byte>> _shapeDetectorService;
        CameraTypes _cameraType;
        public ImageCaptureService(IMachineRepo machineRepo, IShapeDetectorService<Image<Bgr, byte>> shapeDetectorService) : base(machineRepo)
        {
            _shapeDetectorService = shapeDetectorService ?? throw new ArgumentNullException(nameof(shapeDetectorService));

            StartCaptureCommand = new RelayCommand(StartCapture);
            StopCaptureCommand = new RelayCommand(StopCapture);
            CenterFoundItemCommand = CreatedMachineConnectedCommand(CenterFoundItem);
        }

        protected override void MachineChanged(IMachine machine)
        {
            BindCamera(machine);
        }

        private VisionProfile _observedScaleProfile;

        private void ObserveCurrentProfileScale()
        {
            if (_observedScaleProfile != null)
                _observedScaleProfile.PropertyChanged -= VisionProfile_PropertyChanged;

            _observedScaleProfile = Camera?.CurrentVisionProfile;
            if (_observedScaleProfile != null)
                _observedScaleProfile.PropertyChanged += VisionProfile_PropertyChanged;
        }

        private void VisionProfile_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(VisionProfile.PixelsPerMM))
            {
                // MVLocatedCircle captures px/mm at construction. Invalidate
                // those tracked circles immediately when a profile override changes.
                _shapeDetectorService.Reset();
            }
        }

        private void BindCamera(IMachine machine)
        {
            if (Camera != null)
                Camera.PropertyChanged -= Camera_PropertyChanged;
            if (_observedScaleProfile != null)
                _observedScaleProfile.PropertyChanged -= VisionProfile_PropertyChanged;
            _observedScaleProfile = null;

            Camera = machine.Settings.Cameras.FirstOrDefault(c => c.CameraType.Value == CameraType);
            if (Camera != null)
            {
                Camera.CurrentVisionProfile = Camera.VisionProfiles.FirstOrDefault(prf => prf.Key == VisionProfile.VisionProfile_Defauilt);
                if (Camera.CurrentVisionProfile == null)
                {
                    var profile = new VisionProfile()
                    {
                        Id = Guid.NewGuid().ToId(),
                        Key = VisionProfile.VisionProfile_Defauilt,
                        Name = ManufacturingResources.VisionProfile_Defauilt
                    };

                    Camera.VisionProfiles.Add(profile);
                    Camera.CurrentVisionProfile = profile;
                }

                Camera.PropertyChanged += Camera_PropertyChanged;
                ObserveCurrentProfileScale();
                _shapeDetectorService.Reset();
            }
        }

        private void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Camera.CurrentVisionProfile))
            {
                // A new vision profile may be looking for an entirely different circle.
                ObserveCurrentProfileScale();
                _shapeDetectorService.Reset();
                RaisePropertyChanged(nameof(Profile));
            }
            else if (e.PropertyName == nameof(Camera.PixelsPerMM))
            {
                // Camera-level calibration also invalidates all tracked circles;
                // they retain their original px/mm value otherwise.
                _shapeDetectorService.Reset();
            }
        }

        public void ResetDetections()
        {
            _shapeDetectorService.Reset();
        }

        public async Task<InvokeResult<byte[]>> CaptureFeederBarcodeSignalAsync()
        {
            if (!_running || _capture == null ||
                Profile?.Key != VisionProfile.VisionProfile_FeederBarcode ||
                !FeederBarcodeVertical.HasValue)
                return InvokeResult<byte[]>.FromError(
                    "Start the position camera, select Feeder Barcode, and select a feeder orientation.");

            TaskCompletionSource<byte[]> request;
            lock (_captureLocker)
            {
                if (_pendingBarcodeFrame != null)
                    return InvokeResult<byte[]>.FromError("A barcode frame is already being requested.");

                request = new TaskCompletionSource<byte[]>(TaskCreationOptions.RunContinuationsAsynchronously);
                _pendingBarcodeFrame = request;
            }

            var finished = await Task.WhenAny(request.Task, Task.Delay(2500));
            if (finished != request.Task)
            {
                lock (_captureLocker)
                {
                    if (_pendingBarcodeFrame == request)
                        _pendingBarcodeFrame = null;
                }
                return InvokeResult<byte[]>.FromError("Timed out capturing a fresh feeder barcode frame.");
            }

            var sample = await request.Task;
            return sample == null
                ? InvokeResult<byte[]>.FromError("Camera capture stopped during feeder barcode scan.")
                : InvokeResult<byte[]>.Create(sample);
        }

        private static bool IsFeederTapeProfile(string key) =>
            key == VisionProfile.VisionProfile_TapeHoleBlackTape ||
            key == VisionProfile.VisionProfile_TapeHoleWhiteTape ||
            key == VisionProfile.VisionProfile_TapeHoleClearTape ||
            key == VisionProfile.VisionProfile_PartInWhiteTape;

        /// <summary>
        /// Tape-hole counterpart of the second Play corner jog. The requested
        /// circle must still be the exact same stable detector object; do not
        /// substitute the nearest/newest circle during an automated move.
        /// </summary>
        public InvokeResult TryJogToObservedCircle(Guid circleId,
            double expectedOffsetX, double expectedOffsetY, string expectedProfileKey)
        {
            if (CameraType != CameraTypes.Position ||
                !IsFeederTapeProfile(expectedProfileKey) ||
                Profile?.Key != expectedProfileKey ||
                !Machine.Connected || Machine.Mode != OperatingMode.Manual ||
                !Machine.WasMachineHomed || !Machine.WasMachinOriginCalibrated ||
                Machine.Busy)
                return InvokeResult.FromError(
                    "Tape-hole Jog requires an idle, connected, homed machine in Manual mode with the original tape profile.");

            var circle = _shapeDetectorService.FoundCircles
                .SingleOrDefault(item => item.Id == circleId);
            if (circle == null || circle.CameraType != CameraTypes.Position ||
                !circle.Stabilized || !circle.PositionStable || circle.RadiusMM <= 0)
                return InvokeResult.FromError(
                    "The expected stable tape-hole observation is no longer available.");

            var correction = circle.OffsetMM;
            if (Double.IsNaN(expectedOffsetX) || Double.IsInfinity(expectedOffsetX) ||
                Double.IsNaN(expectedOffsetY) || Double.IsInfinity(expectedOffsetY) ||
                Math.Abs(correction.X - expectedOffsetX) > 0.1 ||
                Math.Abs(correction.Y - expectedOffsetY) > 0.1)
                return InvokeResult.FromError(
                    "Tape-hole observation changed before jog; reacquire it.");

            var machineXY = new Point2D<double>(
                Machine.MachinePosition.X,
                Machine.MachinePosition.Y);

            if (!FeederAutoCornerJogMath.TryTarget(machineXY,
                    correction.X, correction.Y, Machine.Settings.WorkAreaSize, out _))
                return InvokeResult.FromError(
                    "Tape-hole correction exceeds the 10 mm per-axis limit or the machine work area.");

            Machine.AddStatusMessage(StatusMessageTypes.Info,
                $"Tape-hole second Play jog X={correction.X:+0.00;-0.00;0.00}, " +
                $"Y={correction.Y:+0.00;-0.00;0.00} mm.");
            Machine.SendSafeMoveHeight();
            Machine.GotoPoint(correction, relativeMove: true);
            _shapeDetectorService.Reset();
            return InvokeResult.Success;
        }

        private bool _bottomNozzleJogRunning;

        /// <summary>
        /// The fixed bottom camera sees the moving nozzle, not a stationary
        /// feature beneath a moving camera. Unlike top-camera centering, its
        /// observed offset must be negated to move the nozzle toward center.
        /// This does one small operator-triggered XY jog, never an unbounded
        /// sequence or a change to Z/tool selection.
        /// </summary>
        private async Task CenterBottomNozzleAsync()
        {
            if (_bottomNozzleJogRunning)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning,
                    "A bottom-camera nozzle jog is already in progress.");
                return;
            }
            _bottomNozzleJogRunning = true;
            try
            {
                var machine = Machine;
                var camera = Camera;
                var profile = Profile;
                var head = machine?.CurrentMachineToolHead;
                if (camera == null || camera.CameraType?.Value != CameraTypes.PartInspection ||
                    (profile?.Key != VisionProfile.VisionProfile_Nozzle &&
                     profile?.Key != VisionProfile.VisionProfile_NozzleCalibration) ||
                    !IsCapturing || machine == null || !machine.Connected ||
                    machine.Mode != OperatingMode.Manual || !machine.WasMachineHomed ||
                    !machine.WasMachinOriginCalibrated || machine.Busy ||
                    head?.CurrentNozzle == null || head.Offset == null ||
                    camera.EffectivePixelsPerMM <= 0)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        "Nozzle jog requires a live bottom camera, stable Nozzle profile, " +
                        "installed selected head, and an idle, homed machine in Manual mode.");
                    return;
                }

                var candidates = _shapeDetectorService.FoundCircles
                    .Where(item => item.CameraType == CameraTypes.PartInspection &&
                                   item.Stabilized && item.PositionStable &&
                                   item.RadiusMM > 0)
                    .ToList();
                if (candidates.Count != 1)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        $"Bottom-camera jog requires exactly one stable nozzle circle; found {candidates.Count}. No movement sent.");
                    return;
                }

                var circle = candidates[0];
                var pixelsPerMm = camera.EffectivePixelsPerMM;
                if (Math.Abs(circle.PixelsPerMM - pixelsPerMm) > 0.0001)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        $"Nozzle circle was detected at {circle.PixelsPerMM:F2} px/mm, but the active " +
                        $"camera profile uses {pixelsPerMm:F2} px/mm. Reacquire before jogging.");
                    _shapeDetectorService.Reset();
                    return;
                }
                // Use full pixel resolution. MVLocatedCircle.OffsetMM rounds to
                // 0.1 mm and can leave a visible residual centering error.
                var observation = new Point2D<double>(
                    circle.OffsetPixels.X / pixelsPerMm,
                    -circle.OffsetPixels.Y / pixelsPerMm);
                if (Math.Abs(observation.X) <= profile.ErrorToleranceMM &&
                    Math.Abs(observation.Y) <= profile.ErrorToleranceMM)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Info,
                        "Nozzle is already centered within the vision profile's tolerance.");
                    return;
                }

                var location = await machine.GetCurrentLocationAsync(5000);
                if (!location.Successful || location.Result == null)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        "Could not read controller XY for bottom-camera nozzle jog; no movement sent.");
                    return;
                }

                // Awaiting M114 gives another opportunity for motion, tool
                // selection, or the tracked target to change. Revalidate all.
                if (!ReferenceEquals(Machine, machine) || !ReferenceEquals(Camera, camera) ||
                    !ReferenceEquals(Profile, profile) ||
                    !ReferenceEquals(machine.CurrentMachineToolHead, head) ||
                    !machine.Connected || machine.Mode != OperatingMode.Manual ||
                    !machine.WasMachineHomed || machine.Busy || !IsCapturing ||
                    !_shapeDetectorService.FoundCircles.Any(c => ReferenceEquals(c, circle)) ||
                    !circle.Stabilized || !circle.PositionStable ||
                    Math.Abs(circle.PixelsPerMM - pixelsPerMm) > 0.0001 ||
                    Math.Abs(circle.OffsetPixels.X / pixelsPerMm - observation.X) > 0.10 ||
                    Math.Abs(-circle.OffsetPixels.Y / pixelsPerMm - observation.Y) > 0.10)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        "Machine or nozzle observation changed during jog preflight; reacquire the circle. No movement sent.");
                    return;
                }

                if (!NozzleOpticalJogMath.TryPlan(location.Result, observation,
                    head.Offset, machine.Settings.WorkAreaSize,
                    out var relativeJog, out var reason))
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        "Bottom-camera nozzle jog blocked: " + reason);
                    return;
                }

                Machine.AddStatusMessage(StatusMessageTypes.Info,
                    $"Bottom-camera nozzle jog: observed X={observation.X:+0.000;-0.000;0.000}, " +
                    $"Y={observation.Y:+0.000;-0.000;0.000} mm; physical XY correction " +
                    $"X={relativeJog.X:+0.000;-0.000;0.000}, " +
                    $"Y={relativeJog.Y:+0.000;-0.000;0.000} mm; " +
                    $"scale={pixelsPerMm:F2} px/mm " +
                    $"({(profile.PixelsPerMM.HasValue ? "profile override" : "camera calibration")}).");

                // The nozzle is already at bottom-camera inspection height.
                // Keep Z stationary, send only the bounded XY correction.
                machine.GotoPoint(relativeJog, relativeMove: true);
                _shapeDetectorService.Reset(); // Require fresh stable detection for another jog.
            }
            catch (Exception ex)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning,
                    "Bottom-camera nozzle jog stopped: " + ex.Message);
            }
            finally
            {
                _bottomNozzleJogRunning = false;
            }
        }

        private void CenterFoundItem()
        {
            if (CameraType == CameraTypes.PartInspection &&
                (Profile?.Key == VisionProfile.VisionProfile_Nozzle ||
                 Profile?.Key == VisionProfile.VisionProfile_NozzleCalibration))
            {
                _ = CenterBottomNozzleAsync();
                return;
            }

            if (Profile?.Key == VisionProfile.VisionProfile_FeederOrigin)
            {
                // Existing second Play button: only a stable white cut corner
                // from the selected Feeder Origin profile authorizes a jog.
                if (!Machine.Connected || Machine.Mode != OperatingMode.Manual ||
                    !Machine.WasMachineHomed)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        "White corner jog requires a homed machine in Manual mode.");
                    return;
                }

                var corner = _shapeDetectorService.FoundCorners
                    .Where(item => item.PositionStable && item.Stabilized)
                    .OrderBy(item => Math.Abs(item.OffsetMM.X) + Math.Abs(item.OffsetMM.Y))
                    .FirstOrDefault();
                if (corner == null)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        "White corner is not stable. Tune the Feeder Origin vision profile " +
                        "until the corner marker stays green before jogging.");
                    return;
                }

                // Feeder Origin always uses the active camera/profile
                // calibration for the white-corner jog.
                var correction = corner.OffsetMM;

                // The calculated row/column approach can legitimately be
                // several millimeters away. Keep a single operator-triggered
                // correction; use work-area and homing checks as before.
                const double maxCornerJogMm = 10.0;
                if (Math.Abs(correction.X) > maxCornerJogMm ||
                    Math.Abs(correction.Y) > maxCornerJogMm)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        $"White corner correction X={correction.X:F2}, Y={correction.Y:F2} mm " +
                        $"exceeds the {maxCornerJogMm:F0} mm per-axis jog limit.");
                    return;
                }

                var current = Machine.MachinePosition;
                var targetX = current.X + correction.X;
                var targetY = current.Y + correction.Y;
                if (targetX < 0 || targetY < 0 ||
                    targetX > Machine.Settings.WorkAreaSize.X ||
                    targetY > Machine.Settings.WorkAreaSize.Y)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        "White corner jog would move outside the configured work area.");
                    return;
                }

                Machine.AddStatusMessage(StatusMessageTypes.Info,
                    $"White corner jog CAMERA SCALE {corner.PixelsPerMm:F1} px/mm: " +
                    $"X={correction.X:+0.00;-0.00;0.00}, " +
                    $"Y={correction.Y:+0.00;-0.00;0.00} mm; " +
                    $"from ({current.X:F2},{current.Y:F2}) " +
                    $"to ({targetX:F2},{targetY:F2}).");
                Machine.SendSafeMoveHeight();
                Machine.GotoPoint(correction, relativeMove: true);
                // Do not reuse the old location after initiating motion.
                _shapeDetectorService.Reset();
                return;
            }

            if (Profile?.Key == VisionProfile.VisionProfile_StagingPlateHole)
            {
                var stableHole = _shapeDetectorService.FoundCircles
                    .Where(circle => circle.Stabilized && circle.PositionStable)
                    .OrderBy(circle => Math.Pow(circle.OffsetPixels.X, 2) +
                                       Math.Pow(circle.OffsetPixels.Y, 2))
                    .FirstOrDefault();

                if (stableHole == null)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        "Staging-plate hole is not stable yet. Wait for the detection crosshairs to settle before centering.");
                    return;
                }

                Machine.GotoPoint(stableHole.OffsetMM, relativeMove: true);
                return;
            }

            if (IsFeederTapeProfile(Profile?.Key))
            {
                var stableHole = _shapeDetectorService.FoundCircles
                    .Where(item => item.Stabilized && item.PositionStable && item.RadiusMM > 0)
                    .OrderBy(item => Math.Abs(item.OffsetMM.X) + Math.Abs(item.OffsetMM.Y))
                    .FirstOrDefault();
                if (stableHole == null)
                {
                    Machine.AddStatusMessage(StatusMessageTypes.Warning,
                        "No stable tape hole is available for the second Play jog.");
                    return;
                }

                var offset = stableHole.OffsetMM;
                var result = TryJogToObservedCircle(stableHole.Id, offset.X, offset.Y, Profile.Key);
                if (!result.Successful)
                    Machine.AddStatusMessage(StatusMessageTypes.Warning, result.ErrorMessage);
                return;
            }

            var firstCircle = _shapeDetectorService.FoundCircles.FirstOrDefault();
            if (firstCircle != null)
            {
                Machine.GotoPoint(firstCircle.OffsetMM, relativeMove: true);
            }
            else
            {
                var firstRect = _shapeDetectorService.FoundRectangles.FirstOrDefault();
                if (firstRect != null)
                {
                    Machine.GotoPoint(firstRect.OffsetMM, relativeMove: true);
                }
            }
        }

        public async void StartCapture()
        {
            CaptureImage = new BitmapImage(new Uri("pack://application:,,/Imgs/PleaseWait.jpg"));

            await Task.Delay(100);

            if (Camera == null)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"There are not cameras confirgured for {CameraType} for this machine..");
                MessageBox.Show($"Please add a camera to this machine and set it's type to {CameraType}.");
                return;
            }

            if (EntityHeader.IsNullOrEmpty(Camera.CameraDevice))
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, $"Need to associate a physical camera with this camera definition.");
                MessageBox.Show("Please open the camera settings and associate a physical camera.");
                return;
            }

            // If we already have a capture instance, don't try to create another one. 
            if (_capture != null)
            {
                return;
            }

            try
            {
                LoadingMask = true;

                _capture = InitCapture(Camera.CameraDevice.Id);
                if (_capture == null)
                {
                    MessageBox.Show($"Could not load {Camera.CameraDevice.Text}");
                    Machine.AddStatusMessage(StatusMessageTypes.Warning, $"Could not load {Camera.CameraDevice.Text}");
                    return;
                }

                // A new capture device does not inherit the previous device's settings.
                // Force the first frame to apply the active profile's effective camera values.
                _lastBrightness = -9999;
                _lastFocus = -9999;
                _lastExposure = -9999;
                _lastTopContrast = -9999;

                if (Camera.CameraType.Value == CameraTypes.Position)
                {
                    _capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 1920);
                    _capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 1920);
                }
                else
                {
                    _capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, Camera.ImageSize.X);
                    _capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, Camera.ImageSize.Y);
                }
                _capture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                Run();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start video, please restart your application: " + ex.Message);
            }
        }

        private VideoCapture InitCapture(string devicePath)
        {
            try
            {
                var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

                //    var camera = cameras.FirstOrDefault(cam => cam.N == devicePath);
                var camera = cameras.Select((cam, idx) => new { cam.DevicePath, index = idx }).FirstOrDefault(cam => cam.DevicePath == devicePath);
                if (camera != null)
                    return new VideoCapture(camera.index);
                else
                    return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open camera: " + ex.Message);
                return null;
            }
        }

        public async Task<InvokeResult> AutoTuneExposureAsync()
        {
            if (_autoTuneExposureCompletion != null)
                return InvokeResult.FromError("Camera Auto-Tune is already running.");

            if (_capture == null || !_running)
            {
                StartCapture();

                for (var attempt = 0; attempt < 30 && (_capture == null || !_running); attempt++)
                    await Task.Delay(100);

                if (_capture == null || !_running)
                    return InvokeResult.FromError("Camera capture could not be started for Auto-Tune.");
            }

            _autoTuneExposureCompletion = new TaskCompletionSource<InvokeResult>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _autoTuneExposureRequested = true;
            return await _autoTuneExposureCompletion.Task;
        }

        public async Task<InvokeResult<DarkCircularOpeningLocator.TuneResult>> TuneNozzleOpeningAsync()
        {
            if (CameraType != CameraTypes.PartInspection || Camera == null ||
                (Profile?.Key != VisionProfile.VisionProfile_Nozzle &&
                 Profile?.Key != VisionProfile.VisionProfile_NozzleCalibration))
                return InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                    "Select the bottom camera's Nozzle or Nozzle Calibration profile.");
            if (_nozzleTuneCompletion != null || _autoTuneExposureCompletion != null)
                return InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                    "Another camera-tuning operation is running.");

            var readiness = await EnsureCaptureReadyAsync();
            if (!readiness.Successful)
                return InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                    readiness.ErrorMessage);

            if (_nozzleTuneCompletion != null || _autoTuneExposureCompletion != null)
                return InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                    "Another camera-tuning operation started.");

            var request = new TaskCompletionSource<InvokeResult<DarkCircularOpeningLocator.TuneResult>>(
                TaskCreationOptions.RunContinuationsAsynchronously);
            _nozzleTuneCompletion = request;
            _nozzleTuneRequested = true;
            var finished = await Task.WhenAny(request.Task, Task.Delay(30000));
            if (finished != request.Task)
            {
                _nozzleTuneRequested = false;
                if (ReferenceEquals(_nozzleTuneCompletion, request))
                    _nozzleTuneCompletion = null;
                return InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                    "Nozzle tuning timed out; profile unchanged.");
            }
            return await request.Task;
        }

        private async Task<InvokeResult<DarkCircularOpeningLocator.TuneResult>> RunNozzleTuneAsync()
        {
            var camera = Camera;
            var profile = Profile;
            var selectedHead = Machine?.CurrentMachineToolHead;
            var selectedNozzleId = selectedHead?.CurrentNozzle == null ? null
                : Convert.ToString(selectedHead.CurrentNozzle.Id);
            var observations = new List<DarkCircularOpeningLocator.Observation>();
            var rejections = new List<string>();
            const int frameCount = 24;
            for (var index = 0; index < frameCount; index++)
            {
                if (!_running || _capture == null || !ReferenceEquals(Camera, camera) ||
                    !ReferenceEquals(Profile, profile))
                    return InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                        "Camera or vision profile changed while tuning.");
                if (selectedHead != null &&
                    (!ReferenceEquals(Machine?.CurrentMachineToolHead, selectedHead) ||
                     !String.Equals(Convert.ToString(selectedHead.CurrentNozzle?.Id),
                         selectedNozzleId, StringComparison.Ordinal)))
                    return InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                        "Selected nozzle changed during image tuning; discarded all samples.");

                using (var original = _capture.QueryFrame())
                {
                    if (original == null)
                    {
                        observations.Add(null);
                        rejections.Add("CAMERA FRAME UNAVAILABLE");
                    }
                    else
                    using (var image = original.ToImage<Bgr, byte>())
                    {
                        image.ROI = new System.Drawing.Rectangle(
                            (image.Width - image.Height) / 2, 0, image.Height, image.Height);
                        using (var cropped = image.Copy())
                        using (var grayscale = cropped.Convert<Gray, byte>())
                        {
                            var diagnostic = new DarkCircularOpeningLocator.RejectionDiagnostics();
                            var observation = DarkCircularOpeningLocator.Find(
                                grayscale.Data, grayscale.Width, grayscale.Height,
                                camera.EffectivePixelsPerMM, profile.TargetImageRadius,
                                expectedDiameterMm: 0, diagnostics: diagnostic);
                            observations.Add(observation);
                            if (observation == null)
                                rejections.Add(diagnostic.PrimaryReason);
                            if (index == 0)
                                CaptureImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(cropped.ToUMat());
                        }
                    }
                }
                await Task.Delay(50);
            }

            if (!DarkCircularOpeningLocator.TryValidate(observations,
                camera.EffectivePixelsPerMM, out var tuned, out var reason))
            {
                var prominentRejections = rejections
                    .GroupBy(item => item)
                    .OrderByDescending(group => group.Count())
                    .Take(2)
                    .Select(group => $"{group.Key} ({group.Count()} frames)");
                var diagnosticSummary = rejections.Count > 0
                    ? " Dominant raw-image rejections: " +
                      String.Join("; ", prominentRejections) + "."
                    : String.Empty;
                return InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                    "Nozzle image tune did not lock: " + reason + diagnosticSummary);
            }
            if (selectedHead != null)
            {
                if (!ReferenceEquals(Machine?.CurrentMachineToolHead, selectedHead) ||
                    !String.Equals(Convert.ToString(selectedHead.CurrentNozzle?.Id),
                        selectedNozzleId, StringComparison.Ordinal))
                    return InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                        "Selected nozzle changed before tune completion; no diameter was associated.");
                if (!String.IsNullOrWhiteSpace(selectedNozzleId))
                    _shapeDetectorService.RegisterVerifiedNozzleOpening(
                        $"{selectedHead.HeadIndex}:{selectedNozzleId}",
                        tuned.DiameterMm, camera.EffectivePixelsPerMM);
            }
            return InvokeResult<DarkCircularOpeningLocator.TuneResult>.Create(tuned);
        }

        private async Task<InvokeResult> RunAutoTuneExposureAsync()
        {
            var originalExposure = Camera.Exposure;
            var candidate = Math.Max(AutoTuneExposureMinimum,
                Math.Min(AutoTuneExposureMaximum, Camera.EffectiveExposure));
            var step = AutoTuneInitialStep;
            var previousDirection = 0;
            (double Dark, double White, double DarkRange, double WhiteRange)? lastSample = null;

            Machine.AddStatusMessage(StatusMessageTypes.Info,
                $"Camera Auto-Tune started at exposure {candidate:F1}.");

            for (var iteration = 0; iteration < AutoTuneMaximumIterations; iteration++)
            {
                var sample = await MeasureReferenceTargetAsync(candidate);
                lastSample = sample;
                Machine.DebugWriteLine(
                    $"[AutoTuneExposure] exposure={candidate:F1}, dark={sample.Dark:F1}, white={sample.White:F1}, darkRange={sample.DarkRange:F1}, whiteRange={sample.WhiteRange:F1}");

                var darkLow = AutoTuneDarkTarget - AutoTuneDarkTolerance;
                var darkHigh = AutoTuneDarkTarget + AutoTuneDarkTolerance;
                var darkInRange = sample.Dark >= darkLow && sample.Dark <= darkHigh;
                var whiteInRange =
                    sample.White >= AutoTuneWhiteMinimum &&
                    sample.White <= AutoTuneWhiteCeiling;
                var stable = sample.DarkRange <= 6.0 && sample.WhiteRange <= 10.0;

                // The narrow dark/white bands are a preferred reference, not
                // the only usable exposure. A camera may have excellent,
                // stable black/white contrast without hitting both exact
                // brightness targets, especially with different lighting.
                var preferred = darkInRange && whiteInRange && stable;
                var usable = CameraExposureQuality.IsUsable(
                    sample.Dark, sample.White, sample.DarkRange, sample.WhiteRange);
                if (preferred || usable)
                {
                    Camera.Exposure = candidate;
                    _lastExposure = candidate;
                    Machine.AddStatusMessage(StatusMessageTypes.Info,
                        $"Camera Auto-Tune completed ({(preferred ? "preferred" : "usable contrast")}). " +
                        $"Exposure={candidate:F1}, dark={sample.Dark:F1}, white={sample.White:F1}, " +
                        $"contrast={sample.White - sample.Dark:F1}.");
                    return InvokeResult.Success;
                }

                if (darkInRange && whiteInRange && !stable)
                    continue;

                // Conflicting preferred targets do not prove the scene is
                // unusable. Continue the bounded exposure search rather than
                // aborting immediately; the broader contrast policy above
                // may still find a safe, stable exposure.

                var direction =
                    sample.White > AutoTuneWhiteCeiling || sample.Dark > darkHigh
                        ? -1
                        : 1;

                if (previousDirection != 0 && direction != previousDirection)
                    step = Math.Max(AutoTuneMinimumStep, step / 2.0);

                previousDirection = direction;
                var next = Math.Round(
                    Math.Max(AutoTuneExposureMinimum,
                        Math.Min(AutoTuneExposureMaximum, candidate + (direction * step))),
                    1);

                if (next == candidate)
                    break;

                candidate = next;
            }

            RestoreExposure(originalExposure);
            if (lastSample.HasValue)
            {
                var sample = lastSample.Value;
                return InvokeResult.FromError(
                    $"Camera Auto-Tune did not find stable high-contrast exposure after {AutoTuneMaximumIterations} sampling attempts. " +
                    $"Last exposure={candidate:F1}, dark={sample.Dark:F1}, white={sample.White:F1}, " +
                    $"contrast={sample.White - sample.Dark:F1}, darkRange={sample.DarkRange:F1}, " +
                    $"whiteRange={sample.WhiteRange:F1}. Check fiducial framing, lighting and camera manual-exposure support.");
            }

            return InvokeResult.FromError(
                $"Camera Auto-Tune did not capture a usable Machine Fiducial sample.");
        }

        private void RestoreExposure(double exposure)
        {
            Camera.Exposure = exposure;
            _capture.Set(Emgu.CV.CvEnum.CapProp.Exposure, exposure);
            _lastExposure = exposure;
        }

        private async Task<(double Dark, double White, double DarkRange, double WhiteRange)> MeasureReferenceTargetAsync(double exposure)
        {
            _capture.Set(Emgu.CV.CvEnum.CapProp.Exposure, exposure);
            _lastExposure = exposure;

            for (var idx = 0; idx < AutoTuneSettleFrames; idx++)
            {
                using (var settleFrame = _capture.QueryFrame())
                {
                }

                await Task.Delay(50);
            }

            var darkSamples = new List<double>();
            var whiteSamples = new List<double>();

            for (var idx = 0; idx < AutoTuneSampleFrames; idx++)
            {
                using (var originalFrame = _capture.QueryFrame())
                {
                    if (originalFrame == null)
                        continue;

                    using (var img = originalFrame.ToImage<Bgr, byte>())
                    {
                        img.ROI = new System.Drawing.Rectangle()
                        {
                            X = (img.Size.Width - img.Size.Height) / 2,
                            Width = img.Size.Height,
                            Y = 0,
                            Height = img.Size.Height
                        };

                        using (var cropped = img.Copy())
                        using (var gray = cropped.Convert<Gray, byte>())
                        {
                            // Auto-Tune owns frame capture while the normal preview loop is paused.
                            // Show a sampled frame so the camera view does not appear frozen.
                            if (idx == 0)
                                CaptureImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(cropped.ToUMat());

                            var levels = MeasureReferenceLevels(gray);
                            darkSamples.Add(levels.Dark);
                            whiteSamples.Add(levels.White);
                        }
                    }
                }

                await Task.Delay(50);
            }

            if (darkSamples.Count < AutoTuneSampleFrames / 2)
                throw new InvalidOperationException("Camera Auto-Tune could not capture enough reference frames.");

            return (
                Median(darkSamples),
                Median(whiteSamples),
                darkSamples.Max() - darkSamples.Min(),
                whiteSamples.Max() - whiteSamples.Min());
        }

        private static (double Dark, double White) MeasureReferenceLevels(Image<Gray, byte> image)
        {
            // The Machine Fiducial is a centered circular target with a dark field,
            // bright ring/cross, and metal center. Restrict sampling to an annulus inside
            // the fiducial so the black machine background cannot dominate the dark sample.
            const int stride = 8;
            var samples = new List<byte>();

            var centerX = image.Width / 2.0;
            var centerY = image.Height / 2.0;
            var radiusScale = Math.Min(image.Width, image.Height);
            var innerRadius = radiusScale * 0.15;
            var outerRadius = radiusScale * 0.35;
            var innerRadiusSquared = innerRadius * innerRadius;
            var outerRadiusSquared = outerRadius * outerRadius;

            for (var y = 0; y < image.Height; y += stride)
            {
                var deltaY = y - centerY;
                for (var x = 0; x < image.Width; x += stride)
                {
                    var deltaX = x - centerX;
                    var radiusSquared = (deltaX * deltaX) + (deltaY * deltaY);
                    if (radiusSquared < innerRadiusSquared ||
                        radiusSquared > outerRadiusSquared)
                        continue;

                    samples.Add(image.Data[y, x, 0]);
                }
            }

            if (samples.Count == 0)
                throw new InvalidOperationException("Camera Auto-Tune reference sampling region was empty.");

            samples.Sort();
            return (
                Percentile(samples, 0.20),
                Percentile(samples, 0.95));
        }

        private static double Percentile(IReadOnlyList<byte> sortedValues, double percentile)
        {
            var index = (int)Math.Round((sortedValues.Count - 1) * percentile);
            return sortedValues[Math.Max(0, Math.Min(sortedValues.Count - 1, index))];
        }

        private static double Median(List<double> values)
        {
            values.Sort();
            var middle = values.Count / 2;
            if (values.Count % 2 == 0)
                return (values[middle - 1] + values[middle]) / 2.0;

            return values[middle];
        }

        public void StopCapture()
        {
            try
            {
                _running = false;
                _lastFrameUtc = default(DateTimeOffset);
                CaptureFrameSize = System.Drawing.Size.Empty;

                _autoTuneExposureRequested = false;
                _autoTuneExposureCompletion?.TrySetResult(
                    InvokeResult.FromError("Camera capture stopped before Auto-Tune completed."));
                _autoTuneExposureCompletion = null;
                _nozzleTuneRequested = false;
                _nozzleTuneCompletion?.TrySetResult(
                    InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                        "Camera capture stopped before nozzle tuning completed."));
                _nozzleTuneCompletion = null;

                lock (_captureLocker)
                {
                    _pendingBarcodeFrame?.TrySetResult(null);
                    _pendingBarcodeFrame = null;
                    if (_capture != null)
                    {
                        _capture.Stop();
                        _capture.Dispose();
                        _capture = null;
                    }
                }

                CaptureImage = new BitmapImage(new Uri("pack://application:,,/Imgs/TestPattern.jpg"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Shutting Down Video, please restart the application." + ex.Message);
            }
            finally
            {
            }
        }

        public async void Run()
        {
            _running = true;
            while (_running)
            {
                if (_capture != null)
                {
                    if (_autoTuneExposureRequested)
                    {
                        var completion = _autoTuneExposureCompletion;
                        _autoTuneExposureRequested = false;

                        // A failed sample can throw after changing the USB camera's
                        // physical exposure. Restore the saved baseline on every
                        // exceptional path, not just on normal non-convergence.
                        var originalExposure = Camera?.Exposure ?? 0;
                        try
                        {
                            completion?.TrySetResult(await RunAutoTuneExposureAsync());
                        }
                        catch (Exception ex)
                        {
                            try
                            {
                                if (_capture != null && Camera != null)
                                    RestoreExposure(originalExposure);
                            }
                            catch (Exception restoreEx)
                            {
                                Machine.DebugWriteLine(
                                    $"[AutoTuneExposure] Could not restore exposure: {restoreEx.Message}");
                            }
                            completion?.TrySetResult(
                                InvokeResult.FromError($"Camera Auto-Tune failed: {ex.Message}. Previous exposure was preserved in machine settings."));
                        }
                        finally
                        {
                            // Exposure sampling bypasses the normal shape detector. Discard
                            // pre-tune circle history so origin centering uses fresh frames.
                            _shapeDetectorService.Reset();
                            _autoTuneExposureCompletion = null;
                        }

                        continue;
                    }

                    if (_nozzleTuneRequested)
                    {
                        var completion = _nozzleTuneCompletion;
                        _nozzleTuneRequested = false;
                        try
                        {
                            completion?.TrySetResult(await RunNozzleTuneAsync());
                        }
                        catch (Exception ex)
                        {
                            completion?.TrySetResult(
                                InvokeResult<DarkCircularOpeningLocator.TuneResult>.FromError(
                                    "Nozzle image tuning failed: " + ex.Message));
                        }
                        finally
                        {
                            _shapeDetectorService.Reset();
                            if (ReferenceEquals(_nozzleTuneCompletion, completion))
                                _nozzleTuneCompletion = null;
                        }
                        continue;
                    }

                    var brightness = Camera.EffectiveBrightness;
                    if (_lastBrightness != brightness)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Brightness, brightness);
                        _lastBrightness = brightness;
                    }

                    var focus = Camera.EffectiveFocus;
                    if (_lastFocus != focus)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Focus, focus);
                        _lastFocus = focus;
                    }

                    var contrast = Camera.EffectiveContrast;
                    if (_lastTopContrast != contrast)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Contrast, contrast);
                        _lastTopContrast = contrast;
                    }

                    var exposure = Camera.EffectiveExposure;
                    if (_lastExposure != exposure)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Exposure, exposure);
                        _lastExposure = exposure;
                    }

                    using (var originalFrame = _capture.QueryFrame())
                    {
                        if (originalFrame != null)
                        {
                            using (var img = originalFrame.ToImage<Bgr, byte>())
                            {
                                img.ROI = new System.Drawing.Rectangle() { X = (img.Size.Width - img.Size.Height) / 2, Width = img.Size.Height, Y = 0, Height = img.Size.Height };
                                using (var cropped = img.Copy())
                                {
                                    //using (var resized = cropped.Resize(Profile.ZoomLevel, Emgu.CV.CvEnum.Inter.LinearExact))
                                    using (var resized = cropped.Resize(1, Emgu.CV.CvEnum.Inter.LinearExact))
                                    {
                                        CaptureFrameSize = resized.Size;
                                        _lastFrameUtc = DateTimeOffset.UtcNow;                                        
                                        if (Profile.PerformShapeDetection || Profile.Key == VisionProfile.VisionProfile_FeederBarcode)
                                        {
                                            using (var results = _shapeDetectorService.PerformShapeDetection(new MVImage<Image<Bgr, byte>>(resized), Camera, resized.Size, FeederBarcodeVertical))
                                            {
                                                if (Profile.Key == VisionProfile.VisionProfile_FeederBarcode && results != null)
                                                {
                                                    var signal = _shapeDetectorService.LatestFeederBarcodeSignal;
                                                    if (signal != null)
                                                    {
                                                        TaskCompletionSource<byte[]> pending;
                                                        lock (_captureLocker)
                                                        {
                                                            pending = _pendingBarcodeFrame;
                                                            _pendingBarcodeFrame = null;
                                                        }
                                                        pending?.TrySetResult((byte[])signal.Clone());
                                                    }
                                                }

                                                // Publish immutable per-frame rectangle statistics
                                                // only after real camera capture and successful
                                                // shape detection. This is the sole basis for
                                                // an automated EMPTY observation; no rectangles
                                                // in a stale frame cannot establish absence.
                                                if (results != null && CameraType == CameraTypes.Position &&
                                                    Profile.FindRectangles)
                                                {
                                                    var frameId = System.Threading.Interlocked.Increment(
                                                        ref _processedRectangleFrameId);
                                                    var rectangles = _shapeDetectorService.FoundRectangles.ToList();
                                                    if (rectangles.Count > 0)
                                                        _lastRectangleDetectedFrameId = frameId;

                                                    var centered = rectangles.Where(r =>
                                                        r.CameraType == CameraTypes.Position &&
                                                        r.Stabilized && r.Centered)
                                                        .OrderByDescending(r => r.FoundCount)
                                                        .ToList();
                                                    var best = centered.FirstOrDefault();
                                                    var offset = best?.OffsetMM;
                                                    var shape = best?.SizeMM;
                                                    var observation = new PocketRectangleFrameObservation
                                                    {
                                                        FrameId = frameId,
                                                        LastRectangleFrameId = _lastRectangleDetectedFrameId,
                                                        CapturedAtUtc = DateTimeOffset.UtcNow,
                                                        CameraId = Camera.Id.ToString(),
                                                        VisionProfileKey = Profile.Key,
                                                        Width = resized.Width,
                                                        Height = resized.Height,
                                                        RectangleCount = rectangles.Count,
                                                        StableCenteredCount = centered.Count,
                                                        StableCenteredFoundCount = best?.FoundCount ?? 0,
                                                        CenteredWidthMm = Math.Abs(shape?.X ?? 0),
                                                        CenteredHeightMm = Math.Abs(shape?.Y ?? 0),
                                                        CenteredOffsetXmm = offset?.X ?? 0,
                                                        CenteredOffsetYmm = offset?.Y ?? 0
                                                    };
                                                    System.Threading.Volatile.Write(
                                                        ref _latestRectangleFrame, observation);
                                                }

                                                if (results == null)
                                                    CaptureImage = new BitmapImage(new Uri("pack://application:,,/Imgs/ComputerBugFunny.jpg"));
                                                else
                                                    CaptureImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results.ToUMat());
                                            }
                                        }
                                        else
                                        {
                                            CaptureImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(resized.ToUMat());
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                    CaptureImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));

                await Task.Delay(50 );
            }
        }

        private BitmapSource _captureImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));

        public BitmapSource CaptureImage
        {
            get { return _captureImage; }
            set { Set(ref _captureImage, value); }
        }

        public RelayCommand StartCaptureCommand { get; }
        public RelayCommand StopCaptureCommand { get; }

        public RelayCommand CenterFoundItemCommand { get; }

        private bool _loadingMasking;
        public bool LoadingMask
        {
            get => _loadingMasking;
            set => Set(ref _loadingMasking, value);
        }

        public CameraTypes CameraType
        {
            get => _cameraType;
            set
            {
                Set(ref _cameraType, value);

                if (Machine != null)
                    BindCamera(Machine);
            }
        }

        MachineCamera _camera;
        public MachineCamera Camera
        {
            get => _camera;
            set
            {
                Set(ref _camera, value);

                if(value != null && value.CameraType.Value == CameraTypes.Position)
                    Machine.PositionImageCaptureService = this;
                else if (value != null && value.CameraType.Value == CameraTypes.PartInspection)
                    Machine.PartInspectionCaptureService = this;
            }
        }

        public IShapeDetectorService<Image<Bgr, byte>> ShapeDetector => _shapeDetectorService;

        public VisionProfile Profile
        {
            get => Camera?.CurrentVisionProfile;
        }
    }
}
