using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using LagoVista.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Models.Resources;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public class ImageCaptureService : MachineViewModelBase, IImageCaptureService, INotifyPropertyChanged
    {
        private bool _running;
        private double _lastBrightness = -9999;
        private double _lastFocus = -9999;
        private double _lastExposure = -9999;
        private double _lastTopContrast = -9999;
        VideoCapture _capture;

        private object _captureLocker = new object();

        ShapeDetectionService _shapeDetectorService;
        ILocatorViewModel _locatorViewModel;
        CameraTypes _cameraType;


        public CameraTypes CameraType
        {
            get => _cameraType;
            set => Set(ref _cameraType, value);            
        }

        MachineCamera _camera;
        public MachineCamera Camera
        {
            get => _camera;
            set => Set(ref _camera, value);
        }

        public ImageCaptureService(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel) : base(machineRepo)
        {            
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));

            StartCaptureCommand = new RelayCommand(StartCapture);
            StopCaptureCommand = new RelayCommand(StopCapture);
        }

        protected override void MachineChanged(IMachine machine)
        {
            Camera = machine.Settings.Cameras.FirstOrDefault(c => c.CameraType.Value == CameraType);

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
        }

        public override Task InitAsync()
        {
            return base.InitAsync();
        }

        public VisionProfile Profile
        {
            get => Camera?.CurrentVisionProfile;
        }

        public void StartCapture()
        {
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


         
            if (_capture != null)
            {
                return;
            }

            _shapeDetectorService = new ShapeDetectionService(_machineRepo, _locatorViewModel, _cameraType);

            try
            {
                LoadingMask = true;

                _capture = InitCapture(Camera.CameraDevice.Id);
                if (_capture == null)
                {
                    MessageBox.Show($"Could not load {Camera.CameraDevice.Text}");
                    return;
                }

                _capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, Camera.ImageSize.X);
                _capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, Camera.ImageSize.Y);
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
                var camera = cameras.Select((cam, idx) => new { DevicePath = cam.DevicePath, index = idx }).FirstOrDefault(cam => cam.DevicePath == devicePath);
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
                                    using (var resized = cropped.Resize(Profile.ZoomLevel, Emgu.CV.CvEnum.Inter.LinearExact))
                                    {
                                        if (Profile.PerformShapeDetection)
                                        {
                                            using (var results = _shapeDetectorService.PerformShapeDetection(resized, _machineRepo.CurrentMachine.Settings.PositioningCamera, Profile))
                                            {
                                                CaptureImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                                            }
                                        }
                                        else
                                        {
                                            CaptureImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(img.ToUMat());
                                        }
                                    }
                                }
                            }
                        }

                    }
                }
                else
                    CaptureImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));

                await Task.Delay(50);
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

        private bool _loadingMasking;
        public bool LoadingMask
        {
            get => _loadingMasking;
            set => Set(ref _loadingMasking, value);
        }

        public IMachineRepo MachineRepo => _machineRepo;
    }
}
