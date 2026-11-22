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
    /// Display-only final acceptance ring. Green is driven ONLY by a fresh
    /// third pickup using the persisted offset, never by raw camera centering.
    /// The nozzle axis is a separate reference in the same calibrated pixels.
    /// </summary>
    public sealed class CameraToolSavedPickupMarkersConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter,
            CultureInfo culture)
        {
            if (values == null || values.Length < 5 ||
                !(values[0] is CameraToolSavedPickupProof proof) ||
                !(values[1] is BitmapSource frame) ||
                !(values[2] is double pixelsPerMm) || pixelsPerMm <= 0 ||
                Double.IsNaN(pixelsPerMm) || Double.IsInfinity(pixelsPerMm) ||
                !(values[3] is bool visible) || !visible ||
                !(values[4] is string profileKey) ||
                profileKey != VisionProfile.VisionProfile_CalibrationDisc ||
                frame.PixelWidth <= 0 || frame.PixelHeight <= 0)
                return null;

            var centerX = frame.PixelWidth / 2.0;
            var centerY = frame.PixelHeight / 2.0;
            var nozzleX = centerX + proof.NozzleOffsetXmm * pixelsPerMm;
            var nozzleY = centerY - proof.NozzleOffsetYmm * pixelsPerMm;
            var discX = centerX + proof.DiscOffsetXmm * pixelsPerMm;
            var discY = centerY - proof.DiscOffsetYmm * pixelsPerMm;

            return new List<CalibrationVideoMarker>
            {
                new CalibrationVideoMarker
                {
                    Kind = CalibrationMarkerKind.Target,
                    CenterX = nozzleX,
                    CenterY = nozzleY,
                    RadiusPixels = 3,
                    Label = "MEASURED NOZZLE CENTER"
                },
                new CalibrationVideoMarker
                {
                    Kind = proof.Verified ? CalibrationMarkerKind.Accepted
                                          : CalibrationMarkerKind.Rejected,
                    CenterX = discX,
                    CenterY = discY,
                    RadiusPixels = proof.DiscDiameterMm * pixelsPerMm / 2.0,
                    Label = proof.Verified
                        ? $"GREEN - SAVED OFFSET {proof.ResidualMm:F3} mm"
                        : $"NOT CENTERED {proof.ResidualMm:F3} mm"
                }
            };
        }

        public object[] ConvertBack(object value, Type[] targetTypes,
            object parameter, CultureInfo culture) =>
            throw new NotSupportedException();
    }
}
