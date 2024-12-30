using LagoVista.Core.Models;
using System;

namespace LagoVista.PickAndPlace.Models
{
    public class VisionProfile : ModelBase
    {
        public string Name { get; set; }
        public string Id { get; set; }

        public VisionSettings BottomProfile {  get; set; }
        public VisionSettings TopProfile { get; set; }
    }

    public class VisionSettings : ModelBase
    {
        private string _name;
        public string Name
        {
            get { return _name; }
            set { _name = value; RaisePropertyChanged(); }
        }

        /* 1-3*/
        private double _brightness = 1.0;
        public double Brightness
        {
            get { return _brightness; }
            set { _brightness = Math.Round(value, 1); RaisePropertyChanged(); }
        }

        /* 0-100*/
        private double _contrast = 50;
        public double Contrast
        {
            get { return _contrast; }
            set { _contrast = Math.Round(value, 1); RaisePropertyChanged(); }
        }

        private int _targetImageRadius = 50;
        public int TargetImageRadius
        {
            get { return _targetImageRadius; }
            set { _targetImageRadius = value; RaisePropertyChanged(); }
        }

        private double _focus = 50;
        public double Focus
        {
            get { return _focus; }
            set { _focus = Math.Round(value, 1); RaisePropertyChanged(); }
        }

        private bool _invert;
        public bool Invert
        {
            get => _invert;
            set => Set(ref _invert, value);
        }


        private double _exposure = 50;
        public double Exposure
        {
            get { return _exposure; }
            set { _exposure = Math.Round(value, 1); RaisePropertyChanged(); }
        }

        private bool _findRectangles;
        public bool FindRectangles
        {
            get => _findRectangles;
            set {  Set(ref _findRectangles, value); }
        }

        private bool _findCircles;
        public bool FindCircles
        {
            get => _findCircles;
            set { Set(ref _findCircles, value); }
        }

        private bool _findLines;
        public bool FindLines
        {
            get => _findLines;
            set { Set(ref _findLines, value); }
        }

        private bool _findCorners;
        public bool FindCorners
        {
            get => _findCorners;
            set { Set(ref _findCorners, value); }
        }

        private double _polygonEpsilonFactor = 0.05;
        public double PolygonEpsilonFactor
        {
            get { return _polygonEpsilonFactor; }
            set { _polygonEpsilonFactor = Math.Round(value, 3); RaisePropertyChanged(); }
        }

        private bool _findIrregularPolygons = true;
        public bool FindIrregularPolygons
        {
            get { return _findIrregularPolygons; }
            set { _findIrregularPolygons = value; RaisePropertyChanged(); }
        }


        private bool _findLandScape = true;
        public bool FindLandScape
        {
            get { return _findLandScape; }
            set { _findLandScape = value; RaisePropertyChanged(); }
        }

        private bool _findPortrait = true;
        public bool FindPortrait
        {
            get { return _findPortrait; }
            set { _findPortrait = value; RaisePropertyChanged(); }
        }


        private bool _contourFindOnlyClosed = true;
        public bool ContourFindOnlyClosed
        {
            get { return _contourFindOnlyClosed; }
            set { _contourFindOnlyClosed = value; RaisePropertyChanged(); }
        }


        private double _contourMinArea = 100;
        public double ContourMinArea
        {
            get { return _contourMinArea; }
            set { _contourMinArea = value; RaisePropertyChanged(); }
        }

        private double _contourMaxArea = 800;
        public double CountourMaxArea
        {
            get { return _contourMaxArea; }
            set { _contourMaxArea = value; RaisePropertyChanged(); }
        }

        private int _harrisCornerApeture = 3;
        public int HarrisCornerAperture
        {
            get { return _harrisCornerApeture; }
            set { _harrisCornerApeture = value; RaisePropertyChanged(); }
        }

        private int _harrisCornerBlockSize = 2;
        public int HarrisCornerBlockSize
        {
            get { return _harrisCornerBlockSize; }
            set { _harrisCornerBlockSize = value; RaisePropertyChanged(); }
        }

        private double _harrisCornerK = 0.04;
        public double HarrisCornerK
        {
            get { return _harrisCornerK; }
            set { _harrisCornerK = value; RaisePropertyChanged(); }
        }

        private int _harrisCornerThreshold = 200;
        public int HarrisCornerThreshold
        {
            get { return _harrisCornerThreshold; }
            set { _harrisCornerThreshold = value; RaisePropertyChanged(); }
        }

        private double _guassianSigmaX = 2;
        public double GaussianSigmaX
        {
            get { return _guassianSigmaX; }
            set { _guassianSigmaX = Math.Round(value, 2); RaisePropertyChanged(); }
        }

        private double _thresholdEdgeDetection = 5;
        public double ThresholdEdgeDetection
        {
            get { return _thresholdEdgeDetection; }
            set
            {
                _thresholdEdgeDetection = value;
                RaisePropertyChanged();
            }
        }

        private bool _useCannyEdgeDetection;
        public bool UseCannyEdgeDetection
        {
            get { return _useCannyEdgeDetection; }
            set { Set(ref _useCannyEdgeDetection, value); }
        }

        private double _cannyLowThreshold = 5;
        public double CannyLowThreshold
        {
            get { return _cannyLowThreshold; }
            set
            {
                _cannyLowThreshold = value;
                RaisePropertyChanged();
                RaisePropertyChanged(nameof(CannyHighThreshold));
            }
        }

        private bool _forTopCamera;
        public bool ForTopCamera
        {
            set { Set(ref _forTopCamera, value); }
            get { return _forTopCamera; }
        }

        private double _zoomLevel = 1;
        public double ZoomLevel 
        { 
            set { Set(ref _zoomLevel, value);  }
            get { return _zoomLevel; }
        }

        public double CannyHighThreshold { get { return CannyLowThreshold * 3; } set { } }
        private int _cannyApetureSize = 3;
        public int CannyApetureSize
        {
            get { return _cannyApetureSize; }
            set
            {
                if (value % 2 == 0)
                {
                    value += 1;
                }

                value = Math.Min(value, 7);
                value = Math.Max(value, 3);

                _cannyApetureSize = value;
                RaisePropertyChanged();
            }
        }

        private bool _cannyGradiant = true;
        public bool CannyGradient
        {
            get { return _cannyGradiant; }
            set { _cannyGradiant = value; RaisePropertyChanged(); }
        }


        private double _houghLinesRHO = 1;
        public double HoughLinesRHO
        {
            get { return _houghLinesRHO; }
            set { _houghLinesRHO = value; RaisePropertyChanged(); }
        }
        private double _houghLinesTheta = Math.PI / 180;
        public double HoughLinesTheta
        {
            get { return _houghLinesTheta; }
            set { _houghLinesTheta = value; RaisePropertyChanged(); }
        }
        private int _houghLinesThreshold = 80;
        public int HoughLinesThreshold
        {
            get { return _houghLinesThreshold; }
            set { _houghLinesThreshold = value; RaisePropertyChanged(); }
        }
        private double _houghLinesMinLength = 30;
        public double HoughLinesMinLineLength
        {
            get { return _houghLinesMinLength; }
            set { _houghLinesMinLength = value; RaisePropertyChanged(); }
        }
        private double _houghLinesMaxLineGap = 10;
        public double HoughLinesMaxLineGap
        {
            get { return _houghLinesMaxLineGap; }
            set { _houghLinesMaxLineGap = value; RaisePropertyChanged(); }
        }

        private double _houghCirclesDP = 2;
        public double HoughCirclesDP
        {
            get { return _houghCirclesDP; }
            set { _houghCirclesDP = Math.Round(value, 3); RaisePropertyChanged(); }
        }
        private double _houghLinesCircleMinDistance = 32;
        public double HoughCirclesMinDistance
        {
            get { return _houghLinesCircleMinDistance; }
            set { _houghLinesCircleMinDistance = Math.Round(value, 1); RaisePropertyChanged(); }
        }

        //Canny Threshold
        private double _houghCirclesParam1 = 30;
        public double HoughCirclesParam1
        {
            get { return _houghCirclesParam1; }
            set { _houghCirclesParam1 = Math.Round(value, 1); RaisePropertyChanged(); }
        }

        //Circle Accumulator Threshold
        private double _houghCirclesParam2 = 550;
        public double HoughCirclesParam2
        {
            get { return _houghCirclesParam2; }
            set { _houghCirclesParam2 = Math.Round(value, 1); RaisePropertyChanged(); }
        }
        private int _houghCirclesMinRadius = 5;
        public int HoughCirclesMinRadius
        {
            get { return _houghCirclesMinRadius; }
            set { _houghCirclesMinRadius = value; RaisePropertyChanged(); }
        }
        private int _houghCirclesMaxRadius = 150;
        public int HoughCirclesMaxRadius
        {
            get { return _houghCirclesMaxRadius; }
            set { _houghCirclesMaxRadius = value; RaisePropertyChanged(); }
        }

        private bool _applyMask = false;
        public bool ApplyMask
        {
            get { return _applyMask; }
            set { _applyMask = value; RaisePropertyChanged(); }
        }

        private bool _applyThreshold = false;
        public bool ApplyThreshold
        {
            get { return _applyThreshold; }
            set { _applyThreshold = value; RaisePropertyChanged(); }
        }

        public byte _primaryThreshold = 80;
        public byte PrimaryThreshold
        {
            get { return _primaryThreshold; }
            set { _primaryThreshold = value; RaisePropertyChanged(); }
        }

        private bool _lightOn = false;
        public bool LightOn
        {
            get { return _lightOn; }
            set { Set(ref _lightOn, value); }
        }

        private byte _red;
        public byte LightRed
        {
            get { return _red; }
            set { Set(ref _red, value); }
        }

        private byte _green;
        public byte LightGreen
        {
            get { return _green; }
            set { Set(ref _green, value); }
        }

        private byte _blue;
        public byte LightBlue
        {
            get { return _blue; }
            set { Set(ref _blue, value); }
        }

        private byte _power;
        public byte LightPower
        {
            get { return _power; }
            set { Set(ref _power, value); }
        }

        private bool _performShapeDetection;
        public bool PerformShapeDetection
        {
            get { return _performShapeDetection; }
            set { Set(ref _performShapeDetection, value); }
        }
    }
}
