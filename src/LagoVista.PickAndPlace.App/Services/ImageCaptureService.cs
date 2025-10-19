// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9b0fa03d4c6f50ced258c2080b20ba6d7736e2198efe1def70c5c368ef1fabf6
// IndexVersion: 0
// --- END CODE INDEX META ---
using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels;
using System;
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
        private double _lastBrightness = -9999;
        private double _lastFocus = -9999;
        private double _lastExposure = -9999;
        private double _lastTopContrast = -9999;
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
                RaisePropertyChanged(nameof(Profile));
            }
        }

        private void CenterFoundItem()
        {
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

        public void StopCapture()
        {
            try
            {
                _running = false;

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
                    if (_lastBrightness != Profile.Brightness)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Brightness, Profile.Brightness);
                        _lastBrightness = Profile.Brightness;
                    }

                    if (_lastFocus != Profile.Focus)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Focus, Profile.Focus);
                        _lastFocus = Profile.Focus;
                    }

                    if (_lastTopContrast != Profile.Contrast)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Contrast, Profile.Contrast);
                        _lastTopContrast = Profile.Contrast;
                    }

                    if (_lastExposure != Profile.Exposure)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Exposure, Profile.Exposure);
                        _lastExposure = Profile.Exposure;
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
                                        if (Profile.PerformShapeDetection)
                                        {
                                            using (var results = _shapeDetectorService.PerformShapeDetection(new MVImage<Image<Bgr, byte>>(resized), Camera, resized.Size))
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
            set => Set(ref _cameraType, value);
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
