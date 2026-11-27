using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public sealed class CameraToolRepeatabilitySample
    {
        public Point2D<double> Nozzle { get; set; }
        public Point2D<double> Disc { get; set; }
        public Point2D<double> Pickup { get; set; }
        public bool Passed { get; set; }
    }

    public static class CameraToolRepeatabilityStatistics
    {
        public static string Summarize(IEnumerable<Point2D<double>> samples)
        {
            var values = samples.Where(p => p != null &&
                !Double.IsNaN(p.X) && !Double.IsInfinity(p.X) &&
                !Double.IsNaN(p.Y) && !Double.IsInfinity(p.Y)).ToArray();
            if (values.Length == 0) return "no valid samples";
            var x = values.Average(p => p.X);
            var y = values.Average(p => p.Y);
            var rms = Math.Sqrt(values.Average(p => (p.X-x)*(p.X-x) + (p.Y-y)*(p.Y-y)));
            var dx = values.Last().X - values.First().X;
            var dy = values.Last().Y - values.First().Y;
            return FormattableString.Invariant($"n={values.Length}; mean XY=({x:F3},{y:F3}); radial mean={values.Average(CameraToolBottomCameraFraming.Radius):F3}; radial max={values.Max(CameraToolBottomCameraFraming.Radius):F3}; RMS scatter={rms:F3}; first-to-last XY=({dx:F3},{dy:F3}) mm");
        }
    }
}
