// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1bc7b9e2bf1856a0478ab1d87ffe49acc2c4d215a1b9a6359ac682dc3970b8fa
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV.Cuda;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Util;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public class MVLocatedCircle : ModelBase
    {
        const int FILTER_SIZE = 9;
        const int THROW_AWAY = 3;
        
        public Point2D<float>[] _centers = new Point2D<float>[FILTER_SIZE];
        public float?[] _radiuses = new float?[FILTER_SIZE];
        
        int _head;

        private double _pixelsPerMM;
        private double _errorMargin;
        private int _stabilizationCount;
        private Point2D<float> _viewCenter;

        private int _foundCount;

        private Point2D<float> _filteredCenter;

        double _filteredRadius;

        private bool _populated;

        public Guid Id { get; }


        public MVLocatedCircle(CameraTypes cameraType, Point2D<int> viewCenter, double pixelsPerMM, double erorMargin, int stabilizationCount)
        {
            _viewCenter = new Point2D<float>(viewCenter.X, viewCenter.Y);
            _pixelsPerMM = pixelsPerMM;
            _errorMargin = erorMargin;
            CameraType = cameraType;
            _stabilizationCount = stabilizationCount;
            Id = Guid.NewGuid();
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

            _foundCount++;

            var sortedX = _centers.Where(pt => pt != null).Select(pt => pt.X).OrderBy(pt => pt);
            var sortedY = _centers.Where(pt => pt != null).Select(pt => pt.Y).OrderBy(pt => pt);
            var sortedRadiuses = _radiuses.Where(rad => rad.HasValue).Select(rad=>rad.Value).OrderBy(rd => rd);

            if (_populated)
            {
                var subsetX = sortedX.Skip(THROW_AWAY).Take(FILTER_SIZE - (THROW_AWAY * 2));
                var subsetY = sortedY.Skip(THROW_AWAY).Take(FILTER_SIZE - (THROW_AWAY * 2));
        
                 _filteredCenter = new Point2D<float>(subsetX.Average(), subsetY.Average());
                 _filteredRadius = sortedRadiuses.Skip(THROW_AWAY).Take(FILTER_SIZE - (THROW_AWAY * 2)).Average();

            }
            else
            {
                _filteredCenter = new Point2D<float>(sortedX.Average(), sortedY.Average());
                _filteredRadius = Math.Round(sortedRadiuses.Average(), 2);
            }

            RaisePropertyChanged(nameof(Summary));
            RaisePropertyChanged(nameof(Centered));
            RaisePropertyChanged(nameof(CenterPixels));
            RaisePropertyChanged(nameof(OffsetPixels));
            RaisePropertyChanged(nameof(OffsetMM));
            RaisePropertyChanged(nameof(RadiusPixels));
            RaisePropertyChanged(nameof(RadiusMM));
            RaisePropertyChanged(nameof(FoundCount));
            RaisePropertyChanged(nameof(Stabilized));
        }

        public bool Stabilized => _foundCount >= _stabilizationCount;

        public bool Centered => Math.Abs(OffsetPixels.X / _pixelsPerMM) < _errorMargin && Math.Abs(OffsetPixels.Y / _pixelsPerMM) < _errorMargin;

        public Point2D<float> CenterPixels  { get => _filteredCenter;  }

        public Point2D<float> OffsetPixels { get => _filteredCenter - _viewCenter; }
        
        public Point2D<double> OffsetMM { get => new Point2D<double>(Math.Round(OffsetPixels.X / _pixelsPerMM, 1), -Math.Round(OffsetPixels.Y / _pixelsPerMM, 1)); }
        public double RadiusPixels { get => _filteredRadius; }
        public double RadiusMM { get => Math.Round( _filteredRadius / _pixelsPerMM, 2); }
       
        public Point2D<double> StandardDeviation { get; set; }

        public int FoundCount { get => _foundCount; }

        public void Reset()
        {
            _foundCount = 0;
            _populated = false;
            _head = 0;
            for(int idx = 0; idx < FILTER_SIZE; ++idx)
            {
                _radiuses[idx] = null;
                _centers[idx] = null;
            }
        }

        public CameraTypes CameraType { get; }

        public int Iteration { get; set; }

        public string Summary
        {
            get => $"Err: {OffsetMM.X:0.00}x{OffsetMM.Y:0.00} mm Dia: {(RadiusMM * 2) :0.0} mm ";
        }
    }

    
}
