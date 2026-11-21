using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace LagoVista.PickAndPlace.App.Controls.Machine.Calibration
{
    /// <summary>
    /// Stagingplatehole centroid overlay, separate from Hough fiducial overlay.
    /// Always draws the applied (not draft) search region, even before a
    /// candidate circle exists. Its optical center stays fixed at frame center.
    /// </summary>
    public sealed class StagingHoleMarkersConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (values == null || values.Length < 8 ||
                !(values[1] is VisionProfile profile) ||
                profile.Key != VisionProfile.VisionProfile_StagingPlateHole ||
                !(values[2] is string activeKey) ||
                activeKey != VisionProfile.VisionProfile_StagingPlateHole ||
                !(values[6] is Size frameSize) ||
                frameSize.Width <= 0 || frameSize.Height <= 0 ||
                !(values[7] is double scale) ||
                scale <= 0 || Double.IsNaN(scale) || Double.IsInfinity(scale))
                return null;

            var markers = new List<CalibrationVideoMarker>();
            if (profile.TargetImageRadius > 0)
            {
                var roi = StagingHoleSearchRegion.Center(frameSize.Width,
                    frameSize.Height, scale, profile.StagingSearchOffsetXmm,
                    profile.StagingSearchOffsetYmm);
                markers.Add(new CalibrationVideoMarker
                {
                    Kind = CalibrationMarkerKind.Target,
                    CenterX = roi.X,
                    CenterY = roi.Y,
                    RadiusPixels = profile.TargetImageRadius * scale,
                    Label = "SEARCH ROI"
                });
            }

            // Draw fixed optical target separately from moved search ROI.
            markers.Add(new CalibrationVideoMarker
            {
                Kind = CalibrationMarkerKind.Target,
                CenterX = frameSize.Width / 2,
                CenterY = frameSize.Height / 2,
                RadiusPixels = 0,
                Label = "OPTICAL CENTER"
            });

            var observation = values[0] as FiducialCircleObservation;
            if (observation != null && observation.FrameWidth == frameSize.Width &&
                observation.FrameHeight == frameSize.Height &&
                observation.RadiusPx > 0)
            {
                markers.Add(new CalibrationVideoMarker
                {
                    Kind = observation.Qualified && observation.Centered
                        ? CalibrationMarkerKind.Accepted
                        : observation.Qualified ? CalibrationMarkerKind.Candidate
                                                : CalibrationMarkerKind.Rejected,
                    CenterX = observation.CenterXpx,
                    CenterY = observation.CenterYpx,
                    RadiusPixels = observation.RadiusPx,
                    Label = observation.Qualified && observation.Centered
                        ? "CENTERED" : observation.Qualified ? "STABLE" : "CANDIDATE"
                });
            }
            return markers;
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
