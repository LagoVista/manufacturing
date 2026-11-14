using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Optical centering of a nozzle seen from a fixed, upward-facing camera.
    /// A positive nozzle-image displacement means the gantry must move in
    /// the opposite direction. This differs from a moving position camera
    /// observing a stationary fiducial.
    /// Pure geometry only; does not send machine commands.
    /// </summary>
    public static class NozzleOpticalJogMath
    {
        public const double MaximumCorrectionPerAxisMm = 5;
        public const double EdgeClearanceMm = NozzleRunoutMotionPreflight.EdgeClearanceMm;

        public static bool TryPlan(
            Point2D<double> controllerXY, Point2D<double> observedOffsetMm,
            Point2D<double> selectedHeadOffset, Point2D<double> workArea,
            out Point2D<double> relativeJog, out string reason)
        {
            relativeJog = null;
            reason = null;
            if (!Valid(controllerXY) || !Valid(observedOffsetMm) ||
                !Valid(selectedHeadOffset) || !Valid(workArea) ||
                workArea.X <= 2 * EdgeClearanceMm ||
                workArea.Y <= 2 * EdgeClearanceMm)
            {
                reason = "Missing or invalid controller position, nozzle observation, selected tool offset, or work area.";
                return false;
            }

            // A stationary bottom camera sees the nozzle translate with the
            // gantry. The pixel offset is already expressed in display/machine
            // XY convention by MVLocatedCircle (including the Y-axis flip).
            relativeJog = new Point2D<double>(
                -observedOffsetMm.X, -observedOffsetMm.Y);

            if (Math.Abs(relativeJog.X) > MaximumCorrectionPerAxisMm ||
                Math.Abs(relativeJog.Y) > MaximumCorrectionPerAxisMm)
            {
                relativeJog = null;
                reason = $"Nozzle is too far from the center for a single optical jog (limit {MaximumCorrectionPerAxisMm:F1} mm per axis).";
                return false;
            }

            // Tool selection applies a physical XY offset followed by G92.
            // Thus the controller's reported position is logical tool XY:
            // physical gantry XY = controller XY + selected-head offset.
            var physicalX = controllerXY.X + selectedHeadOffset.X;
            var physicalY = controllerXY.Y + selectedHeadOffset.Y;
            var targetX = physicalX + relativeJog.X;
            var targetY = physicalY + relativeJog.Y;
            if (!WithinSafeArea(physicalX, physicalY, workArea) ||
                !WithinSafeArea(targetX, targetY, workArea))
            {
                relativeJog = null;
                reason = "Optical jog would approach a physical XY limit after applying the selected tool-head offset.";
                return false;
            }
            return true;
        }

        private static bool Valid(Point2D<double> value) =>
            value != null && Finite(value.X) && Finite(value.Y);
        private static bool Finite(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);
        private static bool WithinSafeArea(double x, double y, Point2D<double> work) =>
            x >= EdgeClearanceMm && y >= EdgeClearanceMm &&
            x <= work.X - EdgeClearanceMm && y <= work.Y - EdgeClearanceMm;
    }
}
