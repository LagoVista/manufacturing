using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Fail-closed provisional motion envelope for the first physically
    /// supervised nozzle-calibration runs. No controller commands are sent.
    /// Values are current tool-navigation XY coordinates in millimeters.
    /// </summary>
    public static class NozzleRunoutMotionPreflight
    {
        public const double EdgeClearanceMm = 5.0;
        public const double MaxAutomaticAxisTravelMm = 85.0;

        public static bool TryValidate(Point2D<double> current,
            Point2D<double> headRelativeOffset, bool requiresHeadSelection,
            Point2D<double> cameraTarget, Point2D<double> workArea,
            out string reason)
        {
            reason = null;
            if (!Valid(workArea) || workArea.X <= 2 * EdgeClearanceMm ||
                workArea.Y <= 2 * EdgeClearanceMm ||
                !Valid(current) || !Valid(cameraTarget) ||
                (requiresHeadSelection && !Valid(headRelativeOffset)))
            {
                reason = "Current XY, tool-head offset, camera location or work-area limits are missing or invalid.";
                return false;
            }
            if (!WithinWorkArea(current, workArea))
            {
                reason = "Controller XY is outside the configured machine work area; calibration cannot start.";
                return false;
            }
            // The existing MoveToToolHeadAsync uses G91 then G0 X/Y by
            // head.Offset; inspect the target before allowing that command.
            var predicted = requiresHeadSelection
                ? new Point2D<double>(
                    current.X + headRelativeOffset.X,
                    current.Y + headRelativeOffset.Y)
                : current;
            if (!WithinSafeArea(predicted, workArea))
            {
                reason = $"Tool selection would approach or exceed an XY travel limit: " +
                         $"target ({predicted.X:F2}, {predicted.Y:F2}), work area " +
                         $"({workArea.X:F2}, {workArea.Y:F2}) mm.";
                return false;
            }
            if (!WithinSafeArea(cameraTarget, workArea))
            {
                reason = $"Configured bottom-camera XY ({cameraTarget.X:F2}, " +
                         $"{cameraTarget.Y:F2}) is on/near a machine travel limit. " +
                         "Verify and correct its AbsolutePosition before calibration.";
                return false;
            }
            // Temporary commissioning limit: no unexpected full-bed
            // traverses from one button press, even to an in-bounds point.
            if (Math.Abs(predicted.X - current.X) > MaxAutomaticAxisTravelMm ||
                Math.Abs(predicted.Y - current.Y) > MaxAutomaticAxisTravelMm ||
                Math.Abs(cameraTarget.X - current.X) > MaxAutomaticAxisTravelMm ||
                Math.Abs(cameraTarget.Y - current.Y) > MaxAutomaticAxisTravelMm)
            {
                reason = $"Calibration would traverse too far from current XY " +
                         $"({current.X:F2}, {current.Y:F2}) toward the tool/camera. " +
                         $"The commissioning limit is {MaxAutomaticAxisTravelMm:F0} mm per axis. " +
                         "Position the gantry near the bottom camera under manual supervision first.";
                return false;
            }
            return true;
        }

        private static bool Valid(Point2D<double> point) =>
            point != null && Finite(point.X) && Finite(point.Y);

        private static bool Finite(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);

        private static bool WithinWorkArea(Point2D<double> point,
            Point2D<double> work) =>
            point.X >= 0 && point.Y >= 0 && point.X <= work.X && point.Y <= work.Y;

        private static bool WithinSafeArea(Point2D<double> point,
            Point2D<double> work) =>
            point.X >= EdgeClearanceMm &&
            point.Y >= EdgeClearanceMm &&
            point.X <= work.X - EdgeClearanceMm &&
            point.Y <= work.Y - EdgeClearanceMm;
    }
}
