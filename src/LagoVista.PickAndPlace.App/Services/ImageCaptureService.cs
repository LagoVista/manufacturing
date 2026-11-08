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

        public System.Drawing.Size CaptureFrameSize { get; private set; }

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
        VideoCapture _capture;

        private object _captureLocker = new object();

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

        private void BindCamera(IMachine machine)
        {
            if (Camera != null)
                Camera.PropertyChanged -= Camera_PropertyChanged;

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
            }
        }

        private void Camera_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if(e.PropertyName == nameof(Camera.CurrentVisionProfile))
            {
                // A new vision profile may be looking for an entirely different circle.
                _shapeDetectorService.Reset();
                RaisePropertyChanged(nameof(Profile));
            }
        }

        public void ResetDetections()
        {
            _shapeDetectorService.Reset();
        }

        private void CenterFoundItem()
        {
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

                if (darkInRange && whiteInRange && stable)
                {
                    Camera.Exposure = candidate;
                    _lastExposure = candidate;
                    Machine.AddStatusMessage(StatusMessageTypes.Info,
                        $"Camera Auto-Tune completed. Exposure={candidate:F1}, dark={sample.Dark:F1}, white={sample.White:F1}.");
                    return InvokeResult.Success;
                }

                if (darkInRange && whiteInRange && !stable)
                    continue;

                if (sample.Dark < darkLow && sample.White > AutoTuneWhiteCeiling)
                {
                    RestoreExposure(originalExposure);
                    return InvokeResult.FromError(
                        $"Camera Auto-Tune cannot increase exposure without clipping white. Exposure={candidate:F1}, dark={sample.Dark:F1}, white={sample.White:F1}.");
                }

                if (sample.Dark > darkHigh && sample.White < AutoTuneWhiteMinimum)
                {
                    RestoreExposure(originalExposure);
                    return InvokeResult.FromError(
                        $"Camera Auto-Tune cannot decrease exposure without dimming white further. Exposure={candidate:F1}, dark={sample.Dark:F1}, white={sample.White:F1}.");
                }

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
                    $"Camera Auto-Tune did not converge after {AutoTuneMaximumIterations} sampling attempts. Last exposure={candidate:F1}, dark={sample.Dark:F1}, white={sample.White:F1}, darkRange={sample.DarkRange:F1}, whiteRange={sample.WhiteRange:F1}.");
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

                _autoTuneExposureRequested = false;
                _autoTuneExposureCompletion?.TrySetResult(
                    InvokeResult.FromError("Camera capture stopped before Auto-Tune completed."));
                _autoTuneExposureCompletion = null;

                lock (_captureLocker)
                {
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

                        try
                        {
                            completion?.TrySetResult(await RunAutoTuneExposureAsync());
                        }
                        catch (Exception ex)
                        {
                            completion?.TrySetResult(
                                InvokeResult.FromError($"Camera Auto-Tune failed: {ex.Message}"));
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
                                        if (Profile.PerformShapeDetection || Profile.Key == VisionProfile.VisionProfile_FeederOrigin)
                                        {
                                            using (var results = _shapeDetectorService.PerformShapeDetection(new MVImage<Image<Bgr, byte>>(resized), Camera, resized.Size, FeederBarcodeVertical))
                                            {
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
