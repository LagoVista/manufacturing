using Emgu.CV.Structure;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using System;
using System.Drawing;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public class MVLocatedRectangle : ModelBase
    {
        int _head;
        
        const int THROW_AWAY = 6;

        const int FILTER_SIZE = 18;
        public RotatedRect?[] _rects = new RotatedRect?[FILTER_SIZE];
        public float?[] _angles = new float?[FILTER_SIZE];
        public Point2D<float>[] _centers = new Point2D<float>[FILTER_SIZE];
        private double _pixelsPerMM;
        private double _errorMargin;
        private int _stabilizationCount;
        private Point2D<float> _viewCenter;
        private bool _populated;

        private Point2D<float> _filteredCenter;
        private float _filteredAngle;

        public MVLocatedRectangle(CameraTypes cameraType, Point2D<int> viewCenter, double pixelsPerMM, double erorMargin, int stabilizationCount)
        {
            _viewCenter = new Point2D<float>(viewCenter.X, viewCenter.Y);
            _pixelsPerMM = pixelsPerMM;
            _errorMargin = erorMargin;
            _stabilizationCount = stabilizationCount;
            CameraType = cameraType;
            for(var idx = 0; idx < FILTER_SIZE; idx++)
            {
                _rects[idx] = null;
                _angles[idx] = 0;  
                _centers[idx] = null;
            }
        }

        public void Add(RotatedRect rect)
        {
            _rects[_head] = rect;
            _angles[_head] = rect.Angle;
            _centers[_head++] = new Point2D<float>(rect.Center.X, rect.Center.Y);
            
            if (_head == FILTER_SIZE)
            {
                _populated = true;
                _head = 0;
            }

            FoundCount++;

            var sortedX = _centers.Where(pt => pt != null).Select(pt => pt.X).OrderBy(pt => pt);
            var sortedY = _centers.Where(pt => pt != null).Select(pt => pt.Y).OrderBy(pt => pt);
            var sortedAngles = _angles.Where(rad => rad.HasValue).OrderBy(rd => rd);

            if (_populated)
            {
                var subsetX = sortedX.Skip(THROW_AWAY).Take(FILTER_SIZE - (THROW_AWAY * 2));
                var subsetY = sortedY.Skip(THROW_AWAY).Take(FILTER_SIZE - (THROW_AWAY * 2));

                _filteredCenter = new Point2D<float>(subsetX.Average(), subsetY.Average());
                Angle = Math.Round(sortedAngles.Select(val=>val.Value).Skip(THROW_AWAY).Take(FILTER_SIZE - (THROW_AWAY * 2)).Average(), 2);

            }
            else
            {
                _filteredCenter = new Point2D<float>(sortedX.Average(), sortedY.Average());
                Angle = Math.Round(sortedAngles.Select(val => val.Value).Average(), 2);
                RotatedRect = rect;
            }

            var size = new SizeF(_rects.Where(rect => rect != null).Average(rct => rct.Value.Size.Width),
                                  _rects.Where(rect => rect != null).Average(rct => rct.Value.Size.Height));
            var center = new PointF(_rects.Where(rect => rect != null).Average(rct => rct.Value.Center.X),
                                    _rects.Where(rect => rect != null).Average(rct => rct.Value.Center.X));

            RotatedRect = new RotatedRect(center, size, (float)Angle);

            Size = new Point2D<double>(rect.Size.Width, rect.Size.Height);
            
            RaisePropertyChanged(nameof(Angle));
            RaisePropertyChanged(nameof(OffsetPixels));
            RaisePropertyChanged(nameof(OffsetMM));
            RaisePropertyChanged(nameof(Centered));
            RaisePropertyChanged(nameof(FoundCount));
            RaisePropertyChanged(nameof(Size));
            RaisePropertyChanged(nameof(Summary));
        }

        /// <summary>
        /// Bounding rectangle including angle of the object.
        /// </summary>
        public RotatedRect RotatedRect { get; set; }

        /// <summary>
        /// Angle of the object in degrees.
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// CameraType that was used to identify the object.
        /// </summary>
        public CameraTypes CameraType { get;  }

        public Point2D<float> OffsetPixels { get => _filteredCenter - _viewCenter; }

        public Point2D<double> OffsetMM { get => new Point2D<double>(-Math.Round(OffsetPixels.X / _pixelsPerMM, 2), Math.Round(OffsetPixels.Y / _pixelsPerMM, 2)); }

        /// <summary>
        /// True if the object was cenered in the image with respect to the error toolerance of the of vision profile.
        /// </summary>
        public bool Centered => Math.Abs(OffsetPixels.X / _pixelsPerMM) < _errorMargin && Math.Abs(OffsetPixels.Y / _pixelsPerMM) < _errorMargin;

        /// <summary>
        /// Size of the object in MM
        /// </summary>
        public Point2D<double> Size { get; set; }
        public Point2D<double> SizeMM { get => new Point2D<double>(Math.Round(Size.X / _pixelsPerMM, 2), -Math.Round(Size.Y / _pixelsPerMM, 2)); }

        /// <summary>
        /// Number of times this object was found in the image within the error tolerance of the vision profile.
        /// </summary>
        public int FoundCount { get; private set; }

        public bool Stabilized => FoundCount >= _stabilizationCount;


        public string Summary
        {
             get => $"{OffsetMM.X:0.00}x{OffsetMM.Y:0.00}xmm ({SizeMM.X:0.0}x{SizeMM.Y:0.0})mm {Angle:000.00}° ";
        }

        public int Iteration { get; set; }

        public void Reset()
        {
            FoundCount = 0;
            _populated = false;
            _head = 0;

            for(var idx = 0; idx < FILTER_SIZE; ++idx)
            {
                _angles[idx] = null;
                _centers[idx] = null;
            }
        }
    }
}
