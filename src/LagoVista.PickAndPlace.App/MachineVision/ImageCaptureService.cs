using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using LagoVista.PickAndPlace.Models;
using System;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public class ImageCaptureService : ViewModelBase, IImageCaptureService, INotifyPropertyChanged
    {
        private bool _running;
        private double _lastBrightness = -9999;
        private double _lastFocus = -9999;
        private double _lastExposure = -9999;
        private double _lastTopContrast = -9999;
        VideoCapture _capture;

        private object _captureLocker = new object();

        private readonly IMachineRepo _machineRepo;
        private VisionSettings _visionSettings;
        ShapeDetectionService _shapeDetectorService;
        ILocatorViewModel _locatorViewModel;
        LocatedByCamera _camera;

        private readonly IVisionProfileManagerViewModel _visionProfileManager;

        public LocatedByCamera Camera 
        { 
            get { return _camera; } 
            set
            {
                Set(ref _camera, value);
                switch(_camera)
                {
                    case LocatedByCamera.PartInspection:
                        _visionSettings = _visionProfileManager.BottomCameraProfile;
                        break;
                    case LocatedByCamera.Position:
                        _visionSettings = _visionProfileManager.TopCameraProfile;
                        break;
                }
            }
        }

        public ImageCaptureService(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel, IVisionProfileManagerViewModel visionProfileManager)
        {
            _visionProfileManager = visionProfileManager ?? throw new ArgumentNullException(nameof(visionProfileManager));
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));

            StartCaptureCommand = new RelayCommand(StartCapture);
            StopCaptureCommand = new RelayCommand(StopCapture);
        }


        public VisionSettings Profile
        {
            get { return _visionSettings; }
        }

        public void StartCapture()
        {

            if (_capture != null)
            {
                return;
            }

            _shapeDetectorService = new ShapeDetectionService(_machineRepo, _locatorViewModel, _camera);


            try
            {
                LoadingMask = true;

                var cameraName = _camera == LocatedByCamera.Position ? _machineRepo.CurrentMachine.Settings.PositioningCamera?.Name : _machineRepo.CurrentMachine.Settings.PartInspectionCamera?.Name;

                if (String.IsNullOrEmpty(cameraName))
                {
                    MessageBox.Show("Please Select a Camera");
                    return;
                }

                _capture = InitCapture(cameraName);
                if (_capture == null)
                {
                    MessageBox.Show($"Could not load {cameraName} camera.");
                }

                _capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 480);
                _capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
                _capture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                Run();                
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start video, please restart your application: " + ex.Message);
            }
        }

        private VideoCapture InitCapture(string cameraName)
        {
            try
            {
                var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
                //    var camera = cameras.FirstOrDefault(cam => cam.N == cameraName);
                var camera = cameras.Select((cam, idx) => new { cam = cam, index = idx }).FirstOrDefault(cam => cam.cam.Name == cameraName);
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
                    if (_lastBrightness != _visionSettings.Brightness)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Brightness, _visionSettings.Brightness);
                        _lastBrightness = _visionSettings.Brightness;
                    }

                    if (_lastFocus != _visionSettings.Focus)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Focus, _visionSettings.Focus);
                        _lastFocus = _visionSettings.Focus;
                    }

                    if (_lastTopContrast != _visionSettings.Contrast)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Contrast, _visionSettings.Contrast);
                        _lastTopContrast = _visionSettings.Contrast;
                    }

                    if (_lastExposure != _visionSettings.Exposure)
                    {
                        _capture.Set(Emgu.CV.CvEnum.CapProp.Exposure, _visionSettings.Exposure);
                        _lastExposure = _visionSettings.Exposure;
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
                                    using (var resized = cropped.Resize(_visionSettings.ZoomLevel, Emgu.CV.CvEnum.Inter.LinearExact))
                                    {
                                        if (_visionSettings.PerformShapeDetection)
                                        {
                                            using (var results = _shapeDetectorService.PerformShapeDetection(resized, _machineRepo.CurrentMachine.Settings.PositioningCamera, _visionSettings))
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
