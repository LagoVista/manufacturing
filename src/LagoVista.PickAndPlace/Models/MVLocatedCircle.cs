using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Util;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public class MVLocatedCircle
    {
        const int FILTER_SIZE = 9;
        const int THROW_AWAY = 3;
        
        public Point2D<float>[] _centers = new Point2D<float>[FILTER_SIZE];
        public float[] _radiuses = new float[FILTER_SIZE];
        
        int _head;

        private double _pixelsPerMM;
        private double _errorMargin;
        private int _stabilizationCount;
        private Point2D<float> _viewCenter;

        private int _pointCount;

        private Point2D<float> _filteredCenter;

       double _filteredRadius;

        private bool _populated;


        public MVLocatedCircle(CameraTypes cameraType, Point2D<int> viewCenter, double pixelsPerMM, double erorMargin, int stabilizationCount)
        {
            _viewCenter = new Point2D<float>(viewCenter.X, viewCenter.Y);
            _pixelsPerMM = pixelsPerMM;
            _errorMargin = erorMargin;
            CameraType = cameraType;
            _stabilizationCount = stabilizationCount;
        }

        public void Add(System.Drawing.PointF point, float radius)
        {
            _radiuses[_head] = radius;
            _centers[_head++] = new Point2D<float>( point.X, point.Y);
            if (_head == FILTER_SIZE)
            {
                _populated = true;
                _head = 0;
            }

            _pointCount++;

            var sortedX = _centers.Where(pt => pt != null).Select(pt => pt.X).OrderBy(pt => pt);
            var sortedY = _centers.Where(pt => pt != null).Select(pt => pt.Y).OrderBy(pt => pt);

            if (_populated)
            {
                var subsetX = sortedX.Skip(THROW_AWAY).Take(FILTER_SIZE - (THROW_AWAY * 2));
                var subsetY = sortedY.Skip(THROW_AWAY).Take(FILTER_SIZE - (THROW_AWAY * 2));
        
                 _filteredCenter = new Point2D<float>(subsetX.Average(), subsetY.Average());
                 _filteredRadius = _radiuses.Skip(THROW_AWAY).Take(FILTER_SIZE - (THROW_AWAY * 2)).Average();
            }
            else
            {
                _filteredCenter = new Point2D<float>(sortedX.Average(), sortedY.Average());
                _filteredRadius = Convert.ToInt32(_radiuses.Average());
            }
        }

        public bool Stabilized => _pointCount >= _stabilizationCount;

        public bool Centered => Math.Abs(OffsetPixels.X) < _errorMargin && Math.Abs(OffsetPixels.Y) < _errorMargin;

        public Point2D<float> CenterPixels  { get => _filteredCenter;  }

        public Point2D<float> OffsetPixels { get => _filteredCenter - _viewCenter; }
        
        public Point2D<double> OffsetMM { get => new Point2D<double>(OffsetPixels.X / _pixelsPerMM, OffsetPixels.Y / _pixelsPerMM); }
        public double RadiusPixels { get => _filteredRadius; }
        public double RadiusMM { get => _filteredRadius / _pixelsPerMM; }
       
        public Point2D<double> StandardDeviation { get; set; }

        public int FoundCount { get => _pointCount; }

        public CameraTypes CameraType { get; }
    }
}
