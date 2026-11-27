using System;
using LagoVista.Manufacturing.Models;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>Explicit opt-ins; polarity remains independently configured by Invert.</summary>
    public static class CircleAutoThresholdProfiles
    {
        public static bool IsAdditional(string key) =>
            key == VisionProfile.VisionProfile_MachineFiducual ||
            key == VisionProfile.VisionProfile_FeederFiducial ||
            key == VisionProfile.VisionProfile_TapeHole ||
            key == VisionProfile.VisionProfile_TapeHoleWhiteTape ||
            key == VisionProfile.VisionProfile_TapeHoleBlackTape ||
            key == VisionProfile.VisionProfile_TapeHoleClearTape;

        public static bool IsSupported(string key) =>
            key == VisionProfile.VisionProfile_KnownLocation ||
            key == VisionProfile.VisionProfile_BoardFiducial || IsAdditional(key);

        // A quarter of the minimum nominal target area allows for edges/blur.
        // Contrast and raw clipping gates apply independently.
        public static double MinimumClassFraction(double minimumRadiusMm, double scale, int samplingRadius)
        {
            if (minimumRadiusMm <= 0 || scale <= 0 || samplingRadius < 3 ||
                double.IsNaN(minimumRadiusMm) || double.IsInfinity(minimumRadiusMm) ||
                double.IsNaN(scale) || double.IsInfinity(scale))
                return 0.05;
            var radius = minimumRadiusMm * scale;
            return Math.Max(0.001, Math.Min(0.05,
                Math.PI * radius * radius / (16.0 * samplingRadius * samplingRadius)));
        }

        public static int BlurKernel(int configured) =>
            configured <= 1 ? 1 : configured % 2 == 0 ? configured - 1 : configured;
    }
}
