using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Tape coordinates are local: +X advances along tape. Horizontal +X is
    /// machine +X; vertical +X is machine -Y. The leading edge of a vertical
    /// feeder is its TOP edge, and a horizontal feeder's LEFT edge.
    /// </summary>
    public static class FeederTapeHoleCalibrationMath
    {
        public static Point2D<double> ToRowOffset(
            Point2D<double> calibratedFeederOrigin,
            Point2D<double> measuredMachinePosition,
            bool vertical)
        {
            if (calibratedFeederOrigin == null || measuredMachinePosition == null)
                throw new ArgumentNullException(calibratedFeederOrigin == null
                    ? nameof(calibratedFeederOrigin) : nameof(measuredMachinePosition));

            var deltaX = measuredMachinePosition.X - calibratedFeederOrigin.X;
            var deltaY = measuredMachinePosition.Y - calibratedFeederOrigin.Y;
            return vertical
                ? new Point2D<double>(Math.Round(-deltaY, 3), Math.Round(deltaX, 3))
                : new Point2D<double>(Math.Round(deltaX, 3), Math.Round(deltaY, 3));
        }

        public static Point2D<double> ToMachinePosition(
            Point2D<double> calibratedFeederOrigin,
            Point2D<double> rowOffset,
            bool vertical)
        {
            if (calibratedFeederOrigin == null || rowOffset == null)
                throw new ArgumentNullException(calibratedFeederOrigin == null
                    ? nameof(calibratedFeederOrigin) : nameof(rowOffset));

            return vertical
                ? new Point2D<double>(
                    calibratedFeederOrigin.X + rowOffset.Y,
                    calibratedFeederOrigin.Y - rowOffset.X)
                : new Point2D<double>(
                    calibratedFeederOrigin.X + rowOffset.X,
                    calibratedFeederOrigin.Y + rowOffset.Y);
        }

        /// <summary>
        /// Select the first WHOLE tape hole past the leading edge, not a
        /// partially cut-off hole whose center happens to be past that edge.
        /// Uses the measured circle radius and the verified feeder origin.
        /// </summary>
        public static bool CompletelyBeyondLeadingEdge(
            Point2D<double> calibratedFeederOrigin,
            Point2D<double> measuredHoleCenter,
            double circleRadiusMm, bool vertical)
        {
            if (calibratedFeederOrigin == null || measuredHoleCenter == null ||
                !Finite(circleRadiusMm) || circleRadiusMm <= 0 ||
                !Finite(measuredHoleCenter.X) || !Finite(measuredHoleCenter.Y) ||
                !Finite(calibratedFeederOrigin.X) || !Finite(calibratedFeederOrigin.Y))
                return false;

            var distanceFromLeadingEdge = vertical
                ? calibratedFeederOrigin.Y - measuredHoleCenter.Y
                : measuredHoleCenter.X - calibratedFeederOrigin.X;
            return distanceFromLeadingEdge > circleRadiusMm;
        }

        /// <summary>
        /// Same first-to-last pitch/count calculation as the existing
        /// FindLastFeederReferenceHole path, applied to the MEASURED first
        /// center rather than an old saved row offset.
        /// </summary>
        public static bool TryApproximateLastHole(
            Point2D<double> measuredFirstHole,
            bool vertical, double feederLengthMm, double pitchMm,
            out Point2D<double> lastHole)
        {
            lastHole = null;
            if (measuredFirstHole == null ||
                !Finite(measuredFirstHole.X) || !Finite(measuredFirstHole.Y) ||
                !Finite(feederLengthMm) || feederLengthMm <= 0 ||
                !Finite(pitchMm) || pitchMm <= 0)
                return false;

            var count = Math.Floor(feederLengthMm / pitchMm);
            if (count < 2 || count > 10000)
                return false;

            var span = (count - 1) * pitchMm;
            lastHole = vertical
                ? new Point2D<double>(measuredFirstHole.X, measuredFirstHole.Y - span)
                : new Point2D<double>(measuredFirstHole.X + span, measuredFirstHole.Y);
            return true;
        }

        private static bool Finite(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);
    }
}
