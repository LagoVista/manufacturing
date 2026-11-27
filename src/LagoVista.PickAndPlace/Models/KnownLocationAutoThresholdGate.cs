using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>Conservative opt-in Known Location photometric and temporal gates.
    /// These pilot quality limits are not changes to the machine's motion tolerances.</summary>
    public sealed class KnownLocationAutoThresholdGate
    {
        public const double MinimumClassSeparation = 24;
        public const double MaximumThresholdSpread = 8;
        public const double MaximumGeometrySpreadMm = 0.10;
        private double? _threshold;
        private (double X, double Y, double Diameter)? _circle;

        public void Reset()
        {
            _threshold = null;
            _circle = null;
        }

        // Anchor each acquisition rather than comparing adjacent frames only:
        // slow illumination drift must not silently accumulate.
        public bool ObserveThreshold(double threshold)
        {
            var consistent = _threshold.HasValue &&
                Math.Abs(threshold - _threshold.Value) <= MaximumThresholdSpread;
            if (!consistent) _threshold = threshold;
            return consistent;
        }

        public bool ObserveCircle(double x, double y, double diameter, double scale)
        {
            var consistent = _circle.HasValue && scale > 0 &&
                Math.Sqrt(Math.Pow(x - _circle.Value.X, 2) + Math.Pow(y - _circle.Value.Y, 2))
                    <= MaximumGeometrySpreadMm * scale &&
                Math.Abs(diameter - _circle.Value.Diameter) <= MaximumGeometrySpreadMm * scale;
            if (!consistent) _circle = (x, y, diameter);
            return consistent;
        }

        // Evaluate class separation in smoothed grayscale, and clipping in
        // the original ROI. The threshold itself is OpenCV's existing Otsu.
        public static bool Assess(byte[,,] smoothed, byte[,,] raw, int threshold,
            out double separation, out string reason, double minimumClassFraction = 0.05)
        {
            separation = 0;
            long dark = 0, bright = 0, darkSum = 0, brightSum = 0;
            long darkClipped = 0, brightClipped = 0;
            for (var y = 0; y < smoothed.GetLength(0); y++)
            for (var x = 0; x < smoothed.GetLength(1); x++)
            {
                var value = smoothed[y, x, 0];
                var original = raw[y, x, 0];
                if (value <= threshold)
                {
                    dark++;
                    darkSum += value;
                    if (original <= 2) darkClipped++;
                }
                else
                {
                    bright++;
                    brightSum += value;
                    if (original >= 253) brightClipped++;
                }
            }
            var count = dark + bright;
            if (count == 0 || dark < count * minimumClassFraction || bright < count * minimumClassFraction)
            {
                reason = "INSUFFICIENT FOREGROUND/BACKGROUND";
                return false;
            }
            separation = (double)brightSum / bright - (double)darkSum / dark;
            if (separation < MinimumClassSeparation)
            {
                reason = "LOW CLASS SEPARATION";
                return false;
            }
            if (darkClipped > dark * 0.25 || brightClipped > bright * 0.25)
            {
                reason = "SATURATED ROI";
                return false;
            }
            reason = "PHOTOMETRIC GATE PASSED";
            return true;
        }
    }
}
