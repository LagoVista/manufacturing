using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public abstract partial class MachineVisionViewModelBase
    {
        VideoCapture _topCameraCapture;
        VideoCapture _bottomCameraCapture;

        Object _videoCaptureLocker = new object();
       
        private string _status = "Idle";
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
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

        private bool _running;

        private double _lastTopBrightness = -9999;
        private double _lastBottomBrightness = -9999;


        private double _lastTopFocus = -9999;
        private double _lastBottomFocus = -9999;

        private double _lastTopExposure = -9999;
        private double _lastBottomExposure = -9999;

        private double _lastTopContrast = -9999;
        private double _lastBottomContrast = -9999;

        public virtual bool UseTopCamera { get; set; } = true;
        public virtual bool UseBottomCamera { get; set; } = false;

        private async void StartImageRecognization()
        {
            _running = true;

            while (_running)
            {

                if (_topCameraCapture != null)
                {
                    if (_lastTopBrightness != _visionProfileManagerViewModel.TopCameraProfile.Brightness)
                    {
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Brightness, _visionProfileManagerViewModel.TopCameraProfile.Brightness);
                        _lastTopBrightness = _visionProfileManagerViewModel.TopCameraProfile.Brightness;
                    }

                    if (_lastTopFocus != _visionProfileManagerViewModel.TopCameraProfile.Focus)
                    {

                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Focus, _visionProfileManagerViewModel.TopCameraProfile.Focus);
                        _lastTopFocus = _visionProfileManagerViewModel.TopCameraProfile.Focus;
                    }

                    if (_lastTopContrast != _visionProfileManagerViewModel.TopCameraProfile.Contrast)
                    {
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Contrast, _visionProfileManagerViewModel.TopCameraProfile.Contrast);
                        _lastTopContrast = _visionProfileManagerViewModel.TopCameraProfile.Contrast;
                    }

                    if (_lastTopExposure != _visionProfileManagerViewModel.TopCameraProfile.Exposure)
                    {
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Exposure, _visionProfileManagerViewModel.TopCameraProfile.Exposure);
                        _lastTopExposure = _visionProfileManagerViewModel.TopCameraProfile.Exposure;
                    }

                    if (LoadingMask)
                    {
                        LoadingMask = false;
                    }

                    using (var originalFrame = _topCameraCapture.QueryFrame())
                    {
                        if (originalFrame != null)
                        {
                            using (var img = originalFrame.ToImage<Bgr, byte>())
                            {
                                img.ROI = new System.Drawing.Rectangle() { X = (img.Size.Width - img.Size.Height) / 2, Width = img.Size.Height, Y = 0, Height = img.Size.Height };
                                using (var cropped = img.Copy())
                                {
                                    using (var resized = cropped.Resize(_visionProfileManagerViewModel.TopCameraProfile.ZoomLevel, Emgu.CV.CvEnum.Inter.LinearExact))
                                    {
                                        if (_visionProfileManagerViewModel.TopCameraProfile.PerformShapeDetection)
                                        {
                                            using (var results = PerformShapeDetection(resized, Machine.Settings.PositioningCamera, _visionProfileManagerViewModel.TopCameraProfile))
                                            {
                                                PrimaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                                            }
                                        }
                                        else
                                        {
                                            PrimaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(resized.ToUMat());
                                        }
                                    }
                                }
                            }
                        }

                        HasPositionFrame = true;
                    }
                }
                else
                    PrimaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));

                if (_bottomCameraCapture != null)
                {
                    //  _bottomCameraCapture.SetCaptureProperty(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                    if (_lastBottomBrightness != _visionProfileManagerViewModel.BottomCameraProfile.Brightness)
                    {
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Brightness, _visionProfileManagerViewModel.BottomCameraProfile.Brightness);
                        _lastBottomBrightness = _visionProfileManagerViewModel.BottomCameraProfile.Brightness;
                    }

                    if (_lastBottomFocus != _visionProfileManagerViewModel.BottomCameraProfile.Focus)
                    {
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Focus, _visionProfileManagerViewModel.BottomCameraProfile.Focus);
                        _lastBottomFocus = _visionProfileManagerViewModel.BottomCameraProfile.Focus;
                    }

                    if (_lastBottomContrast != _visionProfileManagerViewModel.BottomCameraProfile.Contrast)
                    {
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Contrast, _visionProfileManagerViewModel.BottomCameraProfile.Contrast);
                        _lastBottomContrast = _visionProfileManagerViewModel.BottomCameraProfile.Contrast;
                    }

                    if (_lastBottomExposure != _visionProfileManagerViewModel.BottomCameraProfile.Exposure)
                    {
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.Exposure, _visionProfileManagerViewModel.BottomCameraProfile.Exposure);
                        _lastBottomExposure = _visionProfileManagerViewModel.BottomCameraProfile.Exposure;
                    }

                    using (var originalFrame = _bottomCameraCapture.QueryFrame())
                    {
                        if (originalFrame != null)
                        {
                            using (var img = originalFrame.ToImage<Bgr, byte>())
                            {

                                img.ROI = new System.Drawing.Rectangle() { X = (img.Size.Width - img.Size.Height) / 2, Width = img.Size.Height, Y = 0, Height = img.Size.Height };
                                using (var cropped = img.Copy())
                                {
                                    using (var resized = cropped.Resize(_visionProfileManagerViewModel.BottomCameraProfile.ZoomLevel, Emgu.CV.CvEnum.Inter.LinearExact))
                                    {
                                        if (_visionProfileManagerViewModel.BottomCameraProfile.PerformShapeDetection)
                                        {
                                            using (var results = PerformShapeDetection(resized, Machine.Settings.PositioningCamera, _visionProfileManagerViewModel.BottomCameraProfile))
                                            {

                                                SecondaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(results);
                                            }
                                        }
                                        else
                                        {
                                            SecondaryCapturedImage = Emgu.CV.WPF.BitmapSourceConvert.ToBitmapSource(img.ToUMat());
                                        }
                                    }
                                    HasInspectionFrame = true;
                                }
                            }

                            if (LoadingMask)
                            {
                                LoadingMask = false;
                            }
                        }
                    }
                }
                else
                    SecondaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));

                await Task.Delay(50);
            }

            HasInspectionFrame = false;
            HasPositionFrame = false;
        }

        public void StartCapture()
        {
            if (_topCameraCapture != null || _bottomCameraCapture != null)
            {
                return;
            }

            if (Machine.Settings.PositioningCamera == null && Machine.Settings.PartInspectionCamera == null)
            {
                MessageBox.Show("Please Select a Camera");
                new SettingsWindow(Machine, Machine.Settings, false, 2).ShowDialog();
                return;
            }

            try
            {
                LoadingMask = true;

                var positionCameraIndex = Machine.Settings.PositioningCamera == null ? null : (int?)Machine.Settings.PositioningCamera.CameraIndex;
                var inspectionCameraIndex = Machine.Settings.PartInspectionCamera == null ? null : (int?)Machine.Settings.PartInspectionCamera.CameraIndex;

                if (positionCameraIndex.HasValue && inspectionCameraIndex.HasValue)
                {
                    if (positionCameraIndex.Value < inspectionCameraIndex.Value)
                    {
                        _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.Name);
                        _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.Name);
                    }
                    else
                    {
                        _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.Name);
                        _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.Name);
                    }

                    if (_topCameraCapture != null)
                    {
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 480);
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);
                    }

                    if (_bottomCameraCapture != null)
                    {
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 480);
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);

                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);
                    }

                    StartImageRecognization();
                }
                else if (positionCameraIndex.HasValue)
                {
                    _topCameraCapture = InitCapture(Machine.Settings.PositioningCamera.Name);
                    if (_topCameraCapture != null)
                    {
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 480);
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
                        _topCameraCapture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

                        StartImageRecognization();
                    }
                }
                else if (inspectionCameraIndex.HasValue)
                {
                    if (_topCameraCapture != null)
                    {
                        _bottomCameraCapture = InitCapture(Machine.Settings.PartInspectionCamera.Name);
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameWidth, 480);
                        _bottomCameraCapture.Set(Emgu.CV.CvEnum.CapProp.FrameHeight, 480);
                        StartImageRecognization();
                    }
                }
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

                lock (_videoCaptureLocker)
                {
                    if (_topCameraCapture != null)
                    {
                        _topCameraCapture.Stop();
                        _topCameraCapture.Dispose();
                        _topCameraCapture = null;
                    }

                    if (_bottomCameraCapture != null)
                    {
                        _bottomCameraCapture.Stop();
                        _bottomCameraCapture.Dispose();
                        _bottomCameraCapture = null;
                    }
                }

                PrimaryCapturedImage = new BitmapImage(new Uri("pack://application:,,/Imgs/TestPattern.jpg"));
                SecondaryCapturedImage = new BitmapImage(new Uri("pack://application:,,/Imgs/TestPattern.jpg"));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error Shutting Down Video, please restart the application." + ex.Message);
            }
            finally
            {
            }
        }

    }
}
