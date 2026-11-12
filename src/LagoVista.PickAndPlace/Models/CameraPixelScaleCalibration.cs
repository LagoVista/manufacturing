using LagoVista.Manufacturing.Models;
using System;
using System.Globalization;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Manual calibration from a 200-pixel camera overlay. A profile override
    /// and the parent camera baseline are deliberately separate settings.
    /// </summary>
    public static class CameraPixelScaleCalibration
    {
        public const double ReferenceSquarePixels = 200.0;

        public static bool TryCalculate(double? measuredMillimeters, out double pixelsPerMm)
        {
            pixelsPerMm = 0;
            if (!measuredMillimeters.HasValue || measuredMillimeters.Value <= 0 ||
                double.IsNaN(measuredMillimeters.Value) ||
                double.IsInfinity(measuredMillimeters.Value))
                return false;

            var calculated = Math.Round(ReferenceSquarePixels / measuredMillimeters.Value, 1);
            if (calculated <= 0 || double.IsNaN(calculated) || double.IsInfinity(calculated))
                return false;

            pixelsPerMm = calculated;
            return true;
        }

        // Parse the complete user-entered measurement, not each keystroke as
        // a double WPF binding. Support both the local decimal separator and
        // the explicit dot commonly used in machine calibration.
        public static bool TryCalculateText(string measuredMillimetersText, out double pixelsPerMm)
        {
            pixelsPerMm = 0;
            if (String.IsNullOrWhiteSpace(measuredMillimetersText))
                return false;

            if (!double.TryParse(measuredMillimetersText, NumberStyles.Float,
                    CultureInfo.CurrentCulture, out var millimeters) &&
                !double.TryParse(measuredMillimetersText, NumberStyles.Float,
                    CultureInfo.InvariantCulture, out millimeters))
                return false;

            return TryCalculate(millimeters, out pixelsPerMm);
        }

        public static void ApplyToProfile(VisionProfile profile, double pixelsPerMm)
        {
            if (profile == null)
                throw new ArgumentNullException(nameof(profile));

            ValidateScale(pixelsPerMm);
            profile.PixelsPerMM = pixelsPerMm;
        }

        public static void ApplyToCamera(MachineCamera camera, double pixelsPerMm,
            bool clearAllProfileOverrides)
        {
            if (camera == null)
                throw new ArgumentNullException(nameof(camera));

            ValidateScale(pixelsPerMm);
            camera.PixelsPerMM = pixelsPerMm;
            if (clearAllProfileOverrides && camera.VisionProfiles != null)
            {
                foreach (var profile in camera.VisionProfiles)
                {
                    if (profile != null)
                        profile.PixelsPerMM = null;
                }
            }
        }

        private static void ValidateScale(double pixelsPerMm)
        {
            if (pixelsPerMm <= 0 || double.IsNaN(pixelsPerMm) || double.IsInfinity(pixelsPerMm))
                throw new ArgumentOutOfRangeException(nameof(pixelsPerMm));
        }
    }
}
