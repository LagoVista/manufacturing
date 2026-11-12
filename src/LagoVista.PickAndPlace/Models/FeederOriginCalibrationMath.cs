using LagoVista.Core.Models.Drawing;
using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// The feeder origin is measured relative to the staging-plate-derived
    /// approach, not relative to a previous OriginOffset. This function is
    /// side-effect-free and can be used without connecting to the machine.
    /// </summary>
    public static class FeederOriginCalibrationMath
    {
        public static bool TryCalculate(Point2D<double> calculatedOrigin,
            Point2D<double> measuredMachinePosition, double pixelsPerMm,
            bool positionStable, bool centered, out Point2D<double> originOffset)
        {
            originOffset = null;
            if (calculatedOrigin == null || measuredMachinePosition == null ||
                !positionStable || !centered || !Finite(pixelsPerMm) || pixelsPerMm <= 0 ||
                !Finite(calculatedOrigin.X) || !Finite(calculatedOrigin.Y) ||
                !Finite(measuredMachinePosition.X) || !Finite(measuredMachinePosition.Y))
                return false;

            originOffset = new Point2D<double>(
                Math.Round(measuredMachinePosition.X - calculatedOrigin.X, 2),
                Math.Round(measuredMachinePosition.Y - calculatedOrigin.Y, 2));
            return true;
        }

        public static bool SamePosition(Point2D<double> a,
            Point2D<double> b, double toleranceMm)
        {
            return a != null && b != null && Finite(toleranceMm) && toleranceMm >= 0 &&
                   Finite(a.X) && Finite(a.Y) && Finite(b.X) && Finite(b.Y) &&
                   Math.Abs(a.X - b.X) <= toleranceMm &&
                   Math.Abs(a.Y - b.Y) <= toleranceMm;
        }

        private static bool Finite(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);
    }
}
