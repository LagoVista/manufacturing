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

        public static bool TryValidate(Point2D<double> current,
            Point2D<double> headRelativeOffset, bool requiresHeadSelection,
            Point2D<double> cameraTarget, Point2D<double> workArea,
            out string reason, Point2D<double> previousHeadOffset = null)
        {
            reason = null;
            if (!Valid(workArea) || workArea.X <= 2 * EdgeClearanceMm ||
                workArea.Y <= 2 * EdgeClearanceMm ||
                !Valid(current) || !Valid(cameraTarget) ||
                (requiresHeadSelection && !Valid(headRelativeOffset)) ||
                (requiresHeadSelection && previousHeadOffset != null &&
                    !Valid(previousHeadOffset)))
            {
                reason = "Current XY, tool-head offset, camera location or work-area limits are missing or invalid.";
                return false;
            }
            if (!WithinWorkArea(current, workArea))
            {
                reason = "Controller XY is outside the configured machine work area; calibration cannot start.";
                return false;
            }
            // A selected-to-selected head transition is TWO G91 moves:
            // 1) from old head back to the camera by -oldHead.Offset;
            // 2) from camera to new head by +newHead.Offset.
            // Check every physical intermediate endpoint, not just the
            // relative move of the incoming head.
            var afterOldHead = requiresHeadSelection && previousHeadOffset != null
                ? new Point2D<double>(
                    current.X - previousHeadOffset.X,
                    current.Y - previousHeadOffset.Y)
                : current;
            if (!WithinSafeArea(afterOldHead, workArea))
            {
                reason = $"Returning from the previous head would approach an XY limit: " +
                         $"({afterOldHead.X:F2}, {afterOldHead.Y:F2}) mm.";
                return false;
            }
            var predicted = requiresHeadSelection
                ? new Point2D<double>(
                    afterOldHead.X + headRelativeOffset.X,
                    afterOldHead.Y + headRelativeOffset.Y)
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
            // The rectangular work area is convex: with safe Z and both
            // endpoints inside its margin, the direct XY segment remains
            // inside. A known, in-bounds camera is not a user prerequisite
            // to move near manually. Reject only unsafe geometry.
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
