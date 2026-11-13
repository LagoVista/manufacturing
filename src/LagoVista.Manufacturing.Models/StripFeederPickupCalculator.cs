using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.Manufacturing.Models
{
    /// <summary>
    /// Finds a part in a strip feeder using the measured first and last tape
    /// holes. Calculations stay in feeder-local coordinates (X along the tape,
    /// Y across it); only the final pickup point is mapped to machine X/Y.
    /// </summary>
    public static class StripFeederPickupCalculator
    {
        // Sprocket holes are 4 mm apart even on tapes with 2 mm part pitch.
        private const double TapeHolePitchMm = 4.0;

        public static Point2D<double> Calculate(
            Point2D<double> feederOrigin,
            Point2D<double> firstTapeHoleOffset,
            Point2D<double> lastTapeHoleOffset,
            bool isVertical, bool holesOnTop, bool feedForward,
            int partIndex, double tapePitchMm, double tapeWidthMm,
            double? packageXOffsetMm, double? packageYOffsetMm, bool holesOnBothSides)
        {
            if (feederOrigin == null || firstTapeHoleOffset == null || lastTapeHoleOffset == null)
                throw new ArgumentNullException("Feeder origin and both measured tape holes are required.");
            if (partIndex < 1 || tapePitchMm <= 0 || tapeWidthMm <= 0)
                throw new ArgumentOutOfRangeException(nameof(partIndex),
                    "Part index, tape pitch and width must all be positive.");

            // The measured spacing can differ slightly from the nominal 4 mm
            // hole pitch. Recover the integer number of intervals and use that
            // to interpolate the measured tape axis at any component index.
            var measuredSpan = lastTapeHoleOffset.X - firstTapeHoleOffset.X;
            var intervals = Math.Round(measuredSpan / TapeHolePitchMm,
                MidpointRounding.AwayFromZero);
            if (intervals < 1)
                throw new ArgumentException("The last tape hole must follow the first tape hole.");

            var nominalSpan = intervals * TapeHolePitchMm;
            var distanceFromFeedEnd = (partIndex - 1) * tapePitchMm +
                                      (packageXOffsetMm ?? 2.0);
            var fraction = distanceFromFeedEnd / nominalSpan;
            var positionOnTape = feedForward ? fraction : 1.0 - fraction;

            var along = firstTapeHoleOffset.X +
                        measuredSpan * positionOnTape;
            var across = firstTapeHoleOffset.Y +
                         (lastTapeHoleOffset.Y - firstTapeHoleOffset.Y) * positionOnTape;

            var componentOffset = packageYOffsetMm ??
                (holesOnBothSides ? tapeWidthMm / 2 - 1.8 : tapeWidthMm / 2 - 0.5);
            across += holesOnTop ? -componentOffset : componentOffset;

            // The measured tape axis already includes both longitudinal
            // stretch and lateral drift. Never apply an additional correction
            // in machine axes: that previously swapped the two errors.
            return isVertical
                ? new Point2D<double>(feederOrigin.X + across, feederOrigin.Y - along)
                : new Point2D<double>(feederOrigin.X + along, feederOrigin.Y + across);
        }
    }
}
