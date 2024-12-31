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


        private bool _areToolSettingsVisible;
        public bool AreToolSettingsVisible
        {
            get { return _areToolSettingsVisible; }
            set { Set(ref _areToolSettingsVisible, value); }
        }

        //private bool _hasPositionFrame = false;
        //public bool HasPositionFrame
        //{
        //    get { return _hasPositionFrame; }
        //    set
        //    {
        //        var oldHasFrame = _hasPositionFrame;
        //        Set(ref _hasPositionFrame, value);

        //        //if (value && !oldHasFrame)
        //        //{
        //        //    CaptureStarted();
        //        //}

        //        //if (!value && oldHasFrame)
        //        //{
        //        //    CaptureEnded();
        //        //}
        //    }
        //}

        //private bool _hasInspectionFrame = false;
        //public bool HasInspectionFrame
        //{
        //    get { return _hasInspectionFrame; }
        //    set
        //    {
        //        var oldHasFrame = _hasInspectionFrame;
        //        Set(ref _hasInspectionFrame, value);

        //        if (value && !oldHasFrame)
        //        {
        //            CaptureStarted();
        //        }

        //        if (!value && oldHasFrame)
        //        {
        //            CaptureEnded();
        //        }
        //    }
        //}

        //protected virtual void CaptureStarted() { }

        //protected virtual void CaptureEnded() { }



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
