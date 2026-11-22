using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Solve the bottom camera's temporary inspection-frame alignment from
    /// actual supervised nozzle observations. Do not infer image X/Y polarity
    /// or rotation from the camera configuration. No machine settings written.
    /// </summary>
    public static class CameraToolBottomCameraFraming
    {
        public const double ProbeMm = 0.8;
        public const double MaximumFramingCorrectionMm = 4.0;
        public const double NozzleCameraToleranceMm = 0.12;
        public const double DiscCameraToleranceMm = 0.20;
        private const double SafeTravelMarginMm = 5.0;

        private static bool Finite(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);
        private static bool Valid(Point2D<double> value) =>
            value != null && Finite(value.X) && Finite(value.Y);
        public static double Radius(Point2D<double> p) =>
            Valid(p) ? Math.Sqrt(p.X * p.X + p.Y * p.Y) : Double.PositiveInfinity;

        public static bool TryValidateInspectionXY(
            Point2D<double> cameraXY,
            Point2D<double> headOffset,
            Point2D<double> inspectionJog,
            Point2D<double> workArea,
            out Point2D<double> commandedXY, out string error)
        {
            commandedXY = null;
            error = null;
            if (!Valid(cameraXY) || !Valid(headOffset) ||
                !Valid(inspectionJog) || !Valid(workArea) ||
                workArea.X <= 2 * SafeTravelMarginMm ||
                workArea.Y <= 2 * SafeTravelMarginMm ||
                Radius(inspectionJog) > MaximumFramingCorrectionMm)
            {
                error = "Invalid or excessive bottom-camera inspection correction.";
                return false;
            }
            var x = cameraXY.X + inspectionJog.X;
            var y = cameraXY.Y + inspectionJog.Y;
            // The active head has an established G92 coordinate shift.
            // Check the physical target, not just the displayed logical XY.
            var physicalX = x + headOffset.X;
            var physicalY = y + headOffset.Y;
            if (physicalX < SafeTravelMarginMm ||
                physicalY < SafeTravelMarginMm ||
                physicalX > workArea.X - SafeTravelMarginMm ||
                physicalY > workArea.Y - SafeTravelMarginMm)
            {
                error = "Inspection framing target exceeds the physical XY safe margin.";
                return false;
            }
            commandedXY = new Point2D<double>(x, y);
            return true;
        }

        public static bool TrySolve(
            Point2D<double> initialNozzle,
            Point2D<double> xProbeNozzle,
            Point2D<double> yProbeNozzle,
            double probeMm,
            out Point2D<double> framingJog, out string error)
        {
            framingJog = null;
            error = null;
            if (!Valid(initialNozzle) || !Valid(xProbeNozzle) ||
                !Valid(yProbeNozzle) || !Finite(probeMm) ||
                probeMm < 0.4 || probeMm > 1.2)
            {
                error = "Missing nozzle positions or invalid XY probe distance.";
                return false;
            }
            // Measured 2x2 camera response (mm image offset per mm gantry).
            // This handles axis inversion and a rotated camera image.
            var a = (xProbeNozzle.X - initialNozzle.X) / probeMm;
            var c = (xProbeNozzle.Y - initialNozzle.Y) / probeMm;
            var b = (yProbeNozzle.X - initialNozzle.X) / probeMm;
            var d = (yProbeNozzle.Y - initialNozzle.Y) / probeMm;
            var xGain = Math.Sqrt(a * a + c * c);
            var yGain = Math.Sqrt(b * b + d * d);
            var determinant = a * d - b * c;
            if (!Finite(determinant) ||
                xGain < 0.65 || xGain > 1.35 ||
                yGain < 0.65 || yGain > 1.35 ||
                Math.Abs(determinant) < 0.50)
            {
                error = "Bottom-camera XY probe does not have a trustworthy 2-axis response; " +
                    $"X gain {xGain:F2}, Y gain {yGain:F2}, det {determinant:F2}. No framing move.";
                return false;
            }
            // Solve J * delta = -observed nozzle image error.
            var dx = (-d * initialNozzle.X + b * initialNozzle.Y) / determinant;
            var dy = (c * initialNozzle.X - a * initialNozzle.Y) / determinant;
            framingJog = new Point2D<double>(dx, dy);
            if (Radius(framingJog) > MaximumFramingCorrectionMm)
            {
                error = $"Computed bottom-camera inspection jog {Radius(framingJog):F3} mm exceeds " +
                    $"{MaximumFramingCorrectionMm:F1} mm bound; nothing saved.";
                framingJog = null;
                return false;
            }
            return true;
        }

        public static bool TryVerifyCenteredPickup(
            Point2D<double> nozzleImageOffset,
            Point2D<double> discImageOffset,
            out double nozzleFromCameraMm,
            out double discFromCameraMm,
            out string error)
        {
            nozzleFromCameraMm = Radius(nozzleImageOffset);
            discFromCameraMm = Radius(discImageOffset);
            error = null;
            if (nozzleFromCameraMm > NozzleCameraToleranceMm)
            {
                error = $"Nozzle is {nozzleFromCameraMm:F3} mm from bottom-camera center; " +
                    $"must be <= {NozzleCameraToleranceMm:F3} mm.";
                return false;
            }
            if (discFromCameraMm > DiscCameraToleranceMm)
            {
                error = $"Picked disc is {discFromCameraMm:F3} mm from bottom-camera center; " +
                    $"must be <= {DiscCameraToleranceMm:F3} mm.";
                return false;
            }
            return true;
        }
    }
}
