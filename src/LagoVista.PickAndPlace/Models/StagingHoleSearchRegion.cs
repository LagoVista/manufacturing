using System;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Position of the actual staging-hole acquisition ROI in processed image
    /// coordinates. The machine's centering target remains the optical center.
    /// Reused by detector, masks and WPF overlay to avoid visual/logic drift.
    /// </summary>
    public static class StagingHoleSearchRegion
    {
        public static Point2D<double> Center(int frameWidth, int frameHeight,
            double pixelsPerMm, double offsetXmm, double offsetYmm)
        {
            if (frameWidth <= 0 || frameHeight <= 0 ||
                !Valid(pixelsPerMm) || pixelsPerMm <= 0 ||
                !Valid(offsetXmm) || !Valid(offsetYmm))
                throw new ArgumentOutOfRangeException(nameof(pixelsPerMm),
                    "Camera frame, pixel scale and ROI offset must be finite and valid.");

            return new Point2D<double>(
                Math.Floor(frameWidth / 2.0) + offsetXmm * pixelsPerMm,
                Math.Floor(frameHeight / 2.0) + offsetYmm * pixelsPerMm);
        }

        public static bool FitsFrame(int frameWidth, int frameHeight,
            double pixelsPerMm, double offsetXmm, double offsetYmm, double radiusMm,
            bool entireCircleRequired)
        {
            if (frameWidth <= 0 || frameHeight <= 0 ||
                !Valid(pixelsPerMm) || pixelsPerMm <= 0 ||
                !Valid(offsetXmm) || !Valid(offsetYmm) ||
                !Valid(radiusMm) || radiusMm <= 0)
                return false;

            var center = Center(frameWidth, frameHeight, pixelsPerMm, offsetXmm, offsetYmm);
            var radiusPx = pixelsPerMm * radiusMm;
            return entireCircleRequired
                ? center.X - radiusPx >= 0 && center.Y - radiusPx >= 0 &&
                  center.X + radiusPx < frameWidth && center.Y + radiusPx < frameHeight
                : center.X >= 0 && center.Y >= 0 &&
                  center.X < frameWidth && center.Y < frameHeight;
        }

        private static bool Valid(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
