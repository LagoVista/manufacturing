using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public abstract partial class MachineVisionViewModelBase
    {
        bool _show100PixelSquare = false;
        bool _showCrossHairs = true;
        bool _showOriginalImage = true;
        bool _useBlurredImage = true;
       
        public bool Show200PixelSquare
        {
            get { return _show100PixelSquare; }
            set { Set(ref _show100PixelSquare, value); }
        }

        public bool ShowCrossHairs
        {
            get { return _showCrossHairs; }
            set { Set(ref _showCrossHairs, value); }
        }

        public bool ShowOriginalImage
        {
            get { return _showOriginalImage; }
            set { Set(ref _showOriginalImage, value); }
        }

        public bool UseBlurredImage
        {
            get { return _useBlurredImage; }
            set { Set(ref _useBlurredImage, value); }
        }

        public string PolygonHelp { get { return "http://docs.opencv.org/2.4/doc/tutorials/imgproc/shapedescriptors/bounding_rects_circles/bounding_rects_circles.html?highlight=approxpolydp"; } }
        public string PolygonEpsilonHelp { get { return "Parameter specifying the approximation accuracy. This is the maximum distance between the original curve and its approximation"; } }

        public string HarrisCornerLink { get { return "http://docs.opencv.org/2.4/doc/tutorials/features2d/trackingmotion/harris_detector/harris_detector.html"; } }
        public string HarrisCornerApertureHelp { get { return "Apertur parameter for Sobel operation"; } }
        public string HarrisCornerBlockSizeString { get { return "Neighborhood Size"; } }
        public string HarrisCornerKHelp { get { return "Harris detector free parameter."; } }

        public string CannyLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/feature_detection.html"; } }
        public string CannyLink2 { get { return "https://en.wikipedia.org/wiki/Canny_edge_detector"; } }
        public string CannyLowThresholdHelp { get { return "Threshold for Line Detection"; } }
        public string CannyHighThresholdHelp { get { return "Recommended to ve set to three times the lower threshold"; } }
        public string CannyHighThresholdTracksLowThresholdHelp { get { return "Force High Threshold to Map to 3x Low Threshold"; } }
        public string CannyApetureSizeHelp { get { return "The size of the Sobel kernel to be used internally"; } }
        public string CannyGradientHelp { get { return "a flag, indicating whether a more accurate  norm  should be used to calculate the image gradient magnitude ( L2gradient=true ), or whether the default  norm  is enough ( L2gradient=false )."; } }

        public string HoughLinesLink { get { return "http://docs.opencv.org/2.4/doc/tutorials/imgproc/imgtrans/hough_lines/hough_lines.html"; } }
        public string HoughLinesRHOHelp { get { return "The resolution of the parameter R in pixels."; } }
        public string HoughLinesThetaHelp { get { return "The resolution of the parameter Theta in Degrees."; } }
        public string HoughLinesThresholdHelp { get { return "The minimum number of intersections to detect a line."; } }
        public string HoughLinesMinLineHelp { get { return "The minimum number of points that can form a line. Lines with less than this number of points are disregarded."; } }
        public string HoughLinesMaxLineGapHelp { get { return "The maximum gap between two points to be considered in the same line."; } }

        public string HoughCirclesLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/feature_detection.html#houghcircles"; } }
        public string HoughCirclesDPHelp { get { return "Inverse ratio of the accumulator resolution to the image resolution. For example, if dp=1 , the accumulator has the same resolution as the input image. If dp=2 , the accumulator has half as big width and height"; } }
        public string HoughCirclesMinDistanceHelp { get { return "Minimum distance between the centers of the detected circles. If the parameter is too small, multiple neighbor circles may be falsely detected in addition to a true one. If it is too large, some circles may be missed."; } }
        public string HoughCirclesParam1Help { get { return "Higher threshold of the two passed to the Canny() edge detector (the lower one is twice smaller)."; } }
        public string HoughCirclesParam2Help { get { return " it is the accumulator threshold for the circle centers at the detection stage. The smaller it is, the more false circles may be detected. Circles, corresponding to the larger accumulator values, will be returned first."; } }
        public string HoughCirclesMinRadiusHelp { get { return "Minimum Radius"; } }
        public string HoughCirclesMaxRadiusHelp { get { return "Maximum Radius"; } }

        public string GaussianBlurLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/filtering.html?highlight=gaussianblur#cv2.GaussianBlur"; } }
        public string GaussianKSizeHelp { get { return "Gaussian kernel size. ksize.width and ksize.height can differ but they both must be positive and odd. Or, they can be zero’s and then they are computed from sigma* "; } }
        public string GaussianSigmaXHelp { get { return "Gaussian kernel standard deviation in X direction."; } }
        public string GaussianSigmaYHelp { get { return "Gaussian kernel standard deviation in Y direction; if sigmaY is zero, it is set to be equal to sigmaX, if both sigmas are zeros, they are computed from ksize.width and ksize.height , respectively (see getGaussianKernel() for details); to fully control the result regardless of possible future modifications of all this semantics, it is recommended to specify all of ksize, sigmaX, and sigmaY"; } }

        private bool _loadingMask;
        public bool LoadingMask
        {
            get { return _loadingMask; }
            set { Set(ref _loadingMask, value); }
        }

        private BitmapSource _primaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
        public BitmapSource PrimaryCapturedImage
        {
            get { return _primaryCapturedImage; }
            set { Set(ref _primaryCapturedImage, value); }
        }

        private BitmapSource _secondaryCapturedImage = new BitmapImage(new Uri("/Imgs/TestPattern.jpg", UriKind.Relative));
        public BitmapSource SecondaryCapturedImage
        {
            get { return _secondaryCapturedImage; }
            set { Set(ref _secondaryCapturedImage, value); }
        }

        public MachineControlViewModel MachineControls { get; private set; }

        private bool _areToolSettingsVisible;
        public bool AreToolSettingsVisible
        {
            get { return _areToolSettingsVisible; }
            set { Set(ref _areToolSettingsVisible, value); }
        }

        private bool _hasPositionFrame = false;
        public bool HasPositionFrame
        {
            get { return _hasPositionFrame; }
            set
            {
                var oldHasFrame = _hasPositionFrame;
                Set(ref _hasPositionFrame, value);

                if (value && !oldHasFrame)
                {
                    CaptureStarted();
                }

                if (!value && oldHasFrame)
                {
                    CaptureEnded();
                }
            }
        }

        private bool _hasInspectionFrame = false;
        public bool HasInspectionFrame
        {
            get { return _hasInspectionFrame; }
            set
            {
                var oldHasFrame = _hasInspectionFrame;
                Set(ref _hasInspectionFrame, value);

                if (value && !oldHasFrame)
                {
                    CaptureStarted();
                }

                if (!value && oldHasFrame)
                {
                    CaptureEnded();
                }
            }
        }

        private bool _adjustingTopCamera;
        public bool AdjustingTopCamera
        {
            get { return _adjustingTopCamera; }
            set
            {
                Set(ref _adjustingTopCamera, value);
                Profile = value ? _topCameraProfile : _bottomCameraProfile;
                if (value)
                {
                    Machine.TopLightOn = _topCameraProfile.LightOn;
                    Machine.BottomLightOn = false;
                    _topCameraProfile.ForTopCamera = true;
                }
                else
                {
                    Machine.BottomLightOn = _bottomCameraProfile.LightOn;
                    Machine.TopLightOn = false;
                    _topCameraProfile.ForTopCamera = false;
                }
            }
        }

        private VisionProfile _profile;
        public VisionProfile Profile
        {
            get { return _profile; }
            set { Set(ref _profile, value); }
        }

        public bool LightOn
        {
            get => Profile.LightOn;
            set
            {
                Profile.LightOn = value;

                if (AdjustingTopCamera)
                    Machine.TopLightOn = value;
                else
                    Machine.BottomLightOn = value;
            }
        }


        public double ZoomLevel
        {
            get => Profile.ZoomLevel;
            set
            {
                Profile.ZoomLevel = value;
                if (AdjustingTopCamera)
                    RaisePropertyChanged(nameof(TopZoomLevel));
                else
                    RaisePropertyChanged(nameof(BottomZoomLevel));
            }
        }

        public double BottomZoomLevel
        {
            get { return _bottomCameraProfile == null ? 1 : _bottomCameraProfile.ZoomLevel; }
            set 
            { 
                _bottomCameraProfile.ZoomLevel = value;
                RaisePropertyChanged(nameof(BottomZoomLevel));
            }
        }        

       
        public double TopZoomLevel
        {
            get { return _topCameraProfile == null ? 1 : _topCameraProfile.ZoomLevel; }
            set 
            {
                _topCameraProfile.ZoomLevel = value;
                RaisePropertyChanged(nameof(TopZoomLevel));
            }
        }


        protected virtual void CaptureStarted() { }

        protected virtual void CaptureEnded() { }



        private Point2D<double> _circleCenter;
        public Point2D<double> CircleCenter
        {
            get { return _circleCenter; }
            set
            {
                if (value == null)
                {
                    Set(ref _circleCenter, null);
                }
                else if (_circleCenter == null || (_circleCenter.X != value.X ||
                    _circleCenter.Y != value.Y))
                {
                    Set(ref _circleCenter, value);
                }
            }
        }

        private Point2D<double> _standardDeviation;
        public Point2D<double> StandardDeviation
        {
            get { return _standardDeviation; }
            set
            {
                if (value == null)
                {
                    Set(ref _standardDeviation, null);
                }
                else if (_standardDeviation == null || (_standardDeviation.X != value.X || _standardDeviation.Y != value.Y))
                {
                    Set(ref _standardDeviation, value);
                }
            }
        }

    }
}
