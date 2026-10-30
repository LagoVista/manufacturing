using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.Manufacturing.Models
{
    /// <summary>
    /// Machine-independent implementation of the existing strip-feeder pick calculation.
    /// The coordinate/sign conventions deliberately match StripFeederViewModel.
    /// </summary>
    public static class StripFeederPickupCalculator
    {
        public static Point2D<double> Calculate(
            Point2D<double> feederOrigin,
            Point2D<double> firstTapeHoleOffset,
            Point2D<double> lastTapeHoleOffset,
            bool isVertical, bool holesOnTop, bool feedForward,
            int partIndex, double tapePitchMm, double tapeWidthMm,
            double? packageXOffsetMm, double? packageYOffsetMm, bool holesOnBothSides)
        {
            var tapeLength = lastTapeHoleOffset.X - firstTapeHoleOffset.X + 2;
            var expectedDeltaX = Convert.ToInt16(Math.Floor(tapeLength / 4)) * 4;
            var expectedDelta = new Point2D<double>(expectedDeltaX, 0);
            var along = (partIndex - 1) * tapePitchMm + (packageXOffsetMm ?? 2.0);
            var across = packageYOffsetMm ??
                (holesOnBothSides ? tapeWidthMm / 2 - 1.8 : tapeWidthMm / 2 - 0.5);

            var firstHole = feederOrigin.AddWithConditionalSwap(isVertical, lastTapeHoleOffset);
            var lastHole = feederOrigin.AddWithConditionalSwap(isVertical, firstTapeHoleOffset);
            var actualDelta = new Point2D<double>(lastHole.Y - firstHole.Y, lastHole.X - firstHole.X);
            var position = feederOrigin.AddWithConditionalSwap(isVertical,
                feedForward ? firstTapeHoleOffset : lastTapeHoleOffset);

            if (isVertical)
            {
                if (holesOnTop) position.SubtractFromX(across);
                else position.AddToX(across);
            }
            else
            {
                if (holesOnTop) position.SubtractFromY(across);
                else position.AddToY(across);
            }

            if (feedForward)
            {
                if (isVertical) position.SubtractFromY(along);
                else position.AddToX(along);
            }
            else
            {
                if (isVertical) position.AddToY(along);
                else position.SubtractFromX(along);
            }

            var ratio = along / expectedDeltaX;
            if (!feedForward) ratio = 1.0 - ratio;
            return FeederPositionCorrection.Apply(actualDelta, expectedDelta, ratio, position);
        }
    }
}
