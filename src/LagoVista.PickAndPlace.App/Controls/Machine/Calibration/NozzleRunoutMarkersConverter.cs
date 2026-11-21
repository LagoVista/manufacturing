using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.Controls.Machine.Calibration
{
    /// <summary>
    /// Draw the ACTUAL nozzle acquisition search region in processed-frame
    /// coordinates, including the 72px optical-opening floor while adaptive
    /// detection is locked. Display never changes centering/machine motion.
    /// </summary>
    public sealed class NozzleRunoutMarkersConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType,
            object parameter, CultureInfo culture)
        {
            if (values == null || values.Length < 4 ||
                !(values[1] is BitmapSource frame) ||
                !(values[2] is VisionProfile profile) ||
                profile.Key != VisionProfile.VisionProfile_Nozzle ||
                !(values[3] is double scale) ||
                scale <= 0 || double.IsNaN(scale) || double.IsInfinity(scale))
                return null;

            var width = frame.PixelWidth;
            var height = frame.PixelHeight;
            if (width <= 0 || height <= 0) return null;
            var radius = profile.UseAdaptiveDarkOpening
                ? NozzleRunoutVisionTuning.EffectiveSearchRadiusPixels(
                    width, height, scale, profile.TargetImageRadius)
                : profile.TargetImageRadius * scale;
            var markers = new List<CalibrationVideoMarker>();

            if (radius > 0)
                markers.Add(new CalibrationVideoMarker
                {
                    Kind = CalibrationMarkerKind.Target,
                    CenterX = width / 2.0,
                    CenterY = height / 2.0,
                    RadiusPixels = radius,
                    Label = "NOZZLE SEARCH · " + radius.ToString("F0") + " px"
                });

            var obs = values[0] as FiducialCircleObservation;
            if (obs != null && obs.FrameWidth == width &&
                obs.FrameHeight == height && obs.RadiusPx > 0 &&
                obs.CameraType == CameraTypes.PartInspection &&
                obs.ProfileKey == VisionProfile.VisionProfile_Nozzle)
                markers.Add(new CalibrationVideoMarker
                {
                    Kind = obs.Qualified
                        ? obs.Centered ? CalibrationMarkerKind.Accepted
                                       : CalibrationMarkerKind.Candidate
                        : CalibrationMarkerKind.Rejected,
                    CenterX = obs.CenterXpx,
                    CenterY = obs.CenterYpx,
                    RadiusPixels = obs.RadiusPx,
                    Label = obs.Qualified
                        ? obs.Centered ? "CENTERED OPENING" : "LOCKED OPENING"
                        : "NOT QUALIFIED"
                });

            return markers;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
