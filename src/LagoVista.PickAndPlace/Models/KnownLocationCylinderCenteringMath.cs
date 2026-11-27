using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Pure top/position-camera centering math for the physical white calibration
    /// cylinder at Machine.KnownCalibrationPoint. The detector supplies raw pixels
    /// (not MVLocatedCircle.OffsetMM, which rounds to 0.1 mm).
    /// These offsets have the same sign as the existing moving-camera fiducial jog.
    /// </summary>
    public static class KnownLocationCylinderCenteringMath
    {
        public const double MinimumDiameterMm = 6.4;
        public const double MaximumDiameterMm = 8.7;
        public const double CenterToleranceMm = 0.06;
        public const double MaximumJogPerAxisMm = 1.0;
        public const double TravelClearanceMm = 5.0;
        public const int MaximumCenteringAttempts = 4;

        public static bool TryPlan(
            Point2D<double> controllerXY,
            Point2D<double> knownPointXY,
            Point2D<double> circleOffsetPixels,
            double circleDiameterPixels,
            double pixelsPerMm,
            bool positionStable,
            Point2D<double> workAreaMm,
            out Point2D<double> relativeJogMm,
            out Point2D<double> measuredCircleXY,
            out double diameterMm,
            out string reason)
        {
            relativeJogMm = null;
            measuredCircleXY = null;
            diameterMm = Double.NaN;
            if (!Valid(controllerXY) || !Valid(knownPointXY) ||
                !Valid(circleOffsetPixels) || !Valid(workAreaMm) ||
                !Finite(pixelsPerMm) || pixelsPerMm <= 0 ||
                !Finite(circleDiameterPixels) || circleDiameterPixels <= 0 ||
                workAreaMm.X <= 2 * TravelClearanceMm ||
                workAreaMm.Y <= 2 * TravelClearanceMm)
            {
                reason = "Missing or invalid Known Location, circle, camera scale, or machine travel area.";
                return false;
            }
            if (!positionStable)
            {
                reason = "The top camera does not have a stable cylinder circle.";
                return false;
            }
            diameterMm = circleDiameterPixels / pixelsPerMm;
            if (diameterMm < MinimumDiameterMm || diameterMm > MaximumDiameterMm)
            {
                reason = "Detected circle diameter does not match the 7 mm calibration cylinder.";
                return false;
            }
            // MVLocatedCircle already defines the position-camera convention:
            // X grows with image X, machine Y grows opposite image Y.
            var dx = circleOffsetPixels.X / pixelsPerMm;
            var dy = -circleOffsetPixels.Y / pixelsPerMm;
            measuredCircleXY = new Point2D<double>(controllerXY.X + dx, controllerXY.Y + dy);
            if (!Safe(measuredCircleXY.X, measuredCircleXY.Y, workAreaMm))
            {
                reason = "Measured cylinder center is outside the safe machine area.";
                return false;
            }
            if (Math.Abs(dx) <= CenterToleranceMm && Math.Abs(dy) <= CenterToleranceMm)
            {
                relativeJogMm = new Point2D<double>(0, 0);
                reason = null;
                return true;
            }
            if (Math.Abs(dx) > MaximumJogPerAxisMm || Math.Abs(dy) > MaximumJogPerAxisMm)
            {
                reason = "Cylinder is too far from Known Location for a bounded centering jog.";
                return false;
            }
            if (!Safe(controllerXY.X, controllerXY.Y, workAreaMm) ||
                !Safe(controllerXY.X + dx, controllerXY.Y + dy, workAreaMm))
            {
                reason = "Top-camera centering jog would approach a physical XY travel limit.";
                return false;
            }
            relativeJogMm = new Point2D<double>(dx, dy);
            reason = null;
            return true;
        }

        // The first stable observation in a full calibration may refresh
        // the stored seed once. Never persist accumulated large jogs or
        // sub-tolerance noise. Validation-only does not call this writer.
        public static bool TryPlanKnownLocationUpdate(Point2D<double> previous,
            Point2D<double> measured, Point2D<double> workArea, out bool shouldWrite)
        {
            shouldWrite = false;
            if (!Valid(previous) || !Valid(measured) || !Valid(workArea) ||
                !Safe(previous.X, previous.Y, workArea) ||
                !Safe(measured.X, measured.Y, workArea))
                return false;
            var dx = Math.Abs(measured.X - previous.X);
            var dy = Math.Abs(measured.Y - previous.Y);
            if (dx > MaximumJogPerAxisMm || dy > MaximumJogPerAxisMm)
                return false;
            shouldWrite = dx > CenterToleranceMm || dy > CenterToleranceMm;
            return true;
        }

        private static bool Valid(Point2D<double> value) =>
            value != null && Finite(value.X) && Finite(value.Y);
        private static bool Finite(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);
        private static bool Safe(double x, double y, Point2D<double> bounds) =>
            x >= TravelClearanceMm && y >= TravelClearanceMm &&
            x <= bounds.X - TravelClearanceMm &&
            y <= bounds.Y - TravelClearanceMm;
    }
}
