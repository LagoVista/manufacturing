using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Camera-detected corner, including a short history used to reject
    /// unstable locations before the existing camera Jog button moves.
    /// Pixel offsets use the same X / -Y sign convention as MVLocatedCircle.
    /// </summary>
    public class MVLocatedCorner : ModelBase
    {
        private const int HistorySize = 7;
        private readonly Queue<Point2D<double>> _samples = new Queue<Point2D<double>>();
        private double _pixelsPerMm;
        private int _requiredSamples = 5;
        private double _errorToleranceMm = 0.15;
        private Point2D<double> _imageCenter;

        public CameraTypes Camera { get; set; }
        public Point2D<double> Position { get; set; }
        public int FoundCount { get; set; }
        public int Iteration { get; set; }
        public bool UsesStartBarScale { get; private set; }
        public double PixelsPerMm => _pixelsPerMm;
        public Point2D<double> TopEdgeStart { get; private set; }
        public Point2D<double> TopEdgeEnd { get; private set; }
        public Point2D<double> LeftEdgeStart { get; private set; }
        public Point2D<double> LeftEdgeEnd { get; private set; }
        public string EdgeSummary { get; private set; } = String.Empty;

        public void SetFittedEdges(FeederWhiteCornerObservation observation)
        {
            TopEdgeStart = observation?.TopEdgeStart;
            TopEdgeEnd = observation?.TopEdgeEnd;
            LeftEdgeStart = observation?.LeftEdgeStart;
            LeftEdgeEnd = observation?.LeftEdgeEnd;
            EdgeSummary = observation == null || !observation.Found
                ? String.Empty
                : $"TOP {observation.TopEdgeSamples}px LEFT {observation.LeftEdgeSamples}px " +
                  $"RMS {observation.EdgeFitErrorPixels:F2}px";
            RaisePropertyChanged(nameof(Summary));
        }

        public Point2D<double> OffsetPixels => Position == null || _imageCenter == null
            ? new Point2D<double>(0, 0)
            : new Point2D<double>(Position.X - _imageCenter.X,
                Position.Y - _imageCenter.Y);

        public Point2D<double> OffsetMM => _pixelsPerMm <= 0
            ? new Point2D<double>(0, 0)
            : new Point2D<double>(OffsetPixels.X / _pixelsPerMm,
                -OffsetPixels.Y / _pixelsPerMm);

        public bool Stabilized => _samples.Count >= _requiredSamples;

        public bool PositionStable
        {
            get
            {
                if (!Stabilized || _pixelsPerMm <= 0)
                    return false;
                var spread = Math.Max(3.0, _pixelsPerMm * 0.10);
                return _samples.Max(pt => pt.X) - _samples.Min(pt => pt.X) <= spread &&
                       _samples.Max(pt => pt.Y) - _samples.Min(pt => pt.Y) <= spread;
            }
        }

        public bool Centered => PositionStable &&
            Math.Abs(OffsetMM.X) <= _errorToleranceMm &&
            Math.Abs(OffsetMM.Y) <= _errorToleranceMm;

        public string Summary => Position == null
            ? "WHITE CORNER: not found"
            : $"WHITE CORNER ({Position.X:F0},{Position.Y:F0}) " +
              $"X {OffsetMM.X:+0.00;-0.00;0.00} / Y {OffsetMM.Y:+0.00;-0.00;0.00} mm " +
              $"{(PositionStable ? "STABLE" : "ACQUIRING")} " +
              $"{(UsesStartBarScale ? "START SCALE" : "CAMERA SCALE")} " +
              EdgeSummary;

        public void Add(int x, int y, int imageWidth, int imageHeight,
            double pixelsPerMm, double toleranceMm, int stabilizationCount,
            bool usesStartBarScale)
        {
            if (pixelsPerMm <= 0 || double.IsNaN(pixelsPerMm) ||
                double.IsInfinity(pixelsPerMm))
            {
                Reset();
                return;
            }

            var sample = new Point2D<double>(x, y);
            if (_samples.Count > 0)
            {
                // Never combine camera-calibrated and start-calibrated
                // measurements in one stable history. An optical-scale
                // change must reacquire stability before a physical jog.
                if (UsesStartBarScale != usesStartBarScale ||
                    Math.Abs(_pixelsPerMm - pixelsPerMm) >
                        Math.Max(1.0, _pixelsPerMm * 0.05))
                    Reset();
            }
            if (_samples.Count > 0)
            {
                var previous = _samples.Last();
                // A substantially different corner is a new candidate;
                // never jog based on history from a previous location.
                if (Math.Abs(previous.X - x) > pixelsPerMm * 0.75 ||
                    Math.Abs(previous.Y - y) > pixelsPerMm * 0.75)
                    Reset();
            }

            _pixelsPerMm = pixelsPerMm;
            _imageCenter = new Point2D<double>(imageWidth / 2.0, imageHeight / 2.0);
            _requiredSamples = Math.Min(HistorySize, Math.Max(5, stabilizationCount));
            _errorToleranceMm = toleranceMm > 0 ? toleranceMm : 0.15;
            UsesStartBarScale = usesStartBarScale;

            _samples.Enqueue(sample);
            while (_samples.Count > HistorySize)
                _samples.Dequeue();

            var xs = _samples.Select(point => point.X).OrderBy(value => value).ToArray();
            var ys = _samples.Select(point => point.Y).OrderBy(value => value).ToArray();
            Position = new Point2D<double>(xs[xs.Length / 2], ys[ys.Length / 2]);
            FoundCount++;
            RaisePropertyChanged(nameof(Summary));
            RaisePropertyChanged(nameof(Position));
            RaisePropertyChanged(nameof(OffsetMM));
            RaisePropertyChanged(nameof(PositionStable));
            RaisePropertyChanged(nameof(Stabilized));
            RaisePropertyChanged(nameof(Centered));
        }

        public void Reset()
        {
            _samples.Clear();
            FoundCount = 0;
            Position = null;
            UsesStartBarScale = false;
            SetFittedEdges(null);
            RaisePropertyChanged(nameof(Summary));
            RaisePropertyChanged(nameof(PositionStable));
            RaisePropertyChanged(nameof(Stabilized));
            RaisePropertyChanged(nameof(Centered));
        }
    }
}
