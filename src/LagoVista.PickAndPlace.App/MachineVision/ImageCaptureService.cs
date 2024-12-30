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
using System.Windows;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public class ImageCaptureService : ViewModelBase, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private bool _running;
        private double _lastBrightness = -9999;
        private double _lastFocus = -9999;
        private double _lastExposure = -9999;
        private double _lastTopContrast = -9999;
        VideoCapture _capture;

        private object _captureLocker = new object();

        private readonly IMachine _machine;
        private VisionSettings _visionSettings;
        ShapeDetectionService _shapeDetectorService;
        ILocatorViewModel _locatorViewModel;
        LocatedByCamera _camera;

        public ImageCaptureService(IMachine machine, ILocatorViewModel locatorViewModel, LocatedByCamera camera, VisionSettings visionSettings)
        {
            _visionSettings = visionSettings;
            _machine = machine;
            _locatorViewModel = locatorViewModel;
            _camera = camera;
            _shapeDetectorService = new ShapeDetectionService(machine, _locatorViewModel, camera);
        }


        public void StartCapture()
        {
            if (_capture != null)
            {
                return;
            }

            try
            {
                LoadingMask = true;

                var cameraName = _camera == LocatedByCamera.Position ? _machine.Settings.PositioningCamera?.Name : _machine.Settings.PartInspectionCamera?.Name;

                if (String.IsNullOrEmpty(cameraName))
                {
                    MessageBox.Show("Please Select a Camera");
                    return;
                }

                _capture = InitCapture(cameraName);
                if(_capture == null)
                {
                    MessageBox.Show($"Could not load {cameraName} camera.");
                }

                _capture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 480);
                _capture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
                _capture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not start video, please restart your application: " + ex.Message);
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


        public void Run()
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
                                            using (var results = _shapeDetectorService.PerformShapeDetection(resized, _machine.Settings.PositioningCamera, _visionSettings))
                                            {
                                                CaptureImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
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
    }
}
