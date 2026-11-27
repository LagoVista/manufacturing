using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace LagoVista.PickAndPlace.App.Controls.Machine.Calibration
{
    /// <summary>
    /// Maps actual Hough-tracked observations into the shared T008 video markers.
    /// Detector data remains in processed-image pixel coordinates; the shared
    /// surface applies the identical Uniform image-to-screen transform.
    /// </summary>
    public sealed class FiducialMarkersConverter : IValueConverter
    {
        public object Convert(object value, Type targetType,
            object parameter, CultureInfo culture)
        {
            var observation = value as FiducialCircleObservation;
            if (observation == null || observation.FrameWidth <= 0 ||
                observation.FrameHeight <= 0)
                return null;
            var markers = new List<CalibrationVideoMarker>();
            if (observation.SearchRadiusPx > 0)
                markers.Add(new CalibrationVideoMarker
                {
                    Kind = CalibrationMarkerKind.Target,
                    CenterX = observation.FrameWidth / 2.0,
                    CenterY = observation.FrameHeight / 2.0,
                    RadiusPixels = observation.SearchRadiusPx,
                    Label = "SEARCH"
                });
            if (observation.RadiusPx > 0)
                markers.Add(new CalibrationVideoMarker
                {
                    Kind = observation.Qualified && observation.Centered
                        ? CalibrationMarkerKind.Accepted
                        : observation.Qualified
                            ? CalibrationMarkerKind.Candidate
                            : CalibrationMarkerKind.Rejected,
                    CenterX = observation.CenterXpx,
                    CenterY = observation.CenterYpx,
                    RadiusPixels = observation.RadiusPx,
                    Label = observation.Qualified && observation.Centered
                        ? "CENTERED" : observation.Qualified ? "STABLE" : "CANDIDATE"
                });
            return markers;
        }

        public object ConvertBack(object value, Type targetType,
            object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
