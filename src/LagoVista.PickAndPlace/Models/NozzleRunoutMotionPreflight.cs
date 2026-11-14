using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Fail-closed provisional motion envelope for the first physically
    /// supervised nozzle-calibration runs. No controller commands are sent.
    /// Inputs are controller-reported XY; existing tool G92 shifts require
    /// reconstructing the physical gantry position before safety checks.
    /// The sequence is: (1) return any selected head to camera navigation,
    /// (2) travel in the camera frame to the bottom-camera target,
    /// (3) select the requested head there. Never shift the head first.
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
            // MoveToToolHeadAsync shifts the physical gantry by the head
            // offset and uses G92 to restore the prior reported XY.
            // Therefore M114 is in the selected tool's logical frame, not
            // the physical gantry frame, when an old head is selected.
            // At the start of MoveToCameraAsync, physical = M114 + offset;
            // after its relative -offset and G92, physical = M114.
            if (previousHeadOffset != null)
            {
                var selectedHeadPhysicalXY = new Point2D<double>(
                    current.X + previousHeadOffset.X,
                    current.Y + previousHeadOffset.Y);
                if (!WithinWorkArea(selectedHeadPhysicalXY, workArea))
                {
                    reason = $"The previously selected head's physical XY " +
                             $"({selectedHeadPhysicalXY.X:F2}, {selectedHeadPhysicalXY.Y:F2}) mm " +
                             "is outside the machine work area; no calibration motion is safe.";
                    return false;
                }
                if (!WithinSafeArea(current, workArea))
                {
                    reason = $"Returning from the previous head would approach an XY limit: " +
                             $"({current.X:F2}, {current.Y:F2}) mm.";
                    return false;
                }
            }
            if (!WithinSafeArea(cameraTarget, workArea))
            {
                reason = $"Configured bottom-camera XY ({cameraTarget.X:F2}, " +
                         $"{cameraTarget.Y:F2}) is on/near a machine travel limit. " +
                         "Verify and correct its AbsolutePosition before calibration.";
                return false;
            }
            // After navigating to camera XY in camera mode, select the
            // requested head. The shift is applied at the camera, NOT at
            // the gantry's prior XY (which may be near the top-right edge).
            var predicted = requiresHeadSelection
                ? new Point2D<double>(
                    cameraTarget.X + headRelativeOffset.X,
                    cameraTarget.Y + headRelativeOffset.Y)
                : cameraTarget;
            if (!WithinSafeArea(predicted, workArea))
            {
                reason = $"Selecting the tool head at the bottom camera would " +
                         $"approach or exceed an XY travel limit: target " +
                         $"({predicted.X:F2}, {predicted.Y:F2}), work area " +
                         $"({workArea.X:F2}, {workArea.Y:F2}) mm.";
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
