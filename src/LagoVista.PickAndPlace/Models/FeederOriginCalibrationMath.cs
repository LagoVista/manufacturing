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

        /// <summary>
        /// A freshly captured camera baseline is valid on the FIRST approach.
        /// Later observations must retain the same camera, active profile,
        /// and effective physical scale. Uninitialized/NaN values never pass.
        /// </summary>
        public static bool CameraProfileUnchanged(
            string expectedCameraId, double expectedPixelsPerMm,
            string actualCameraId, double actualPixelsPerMm,
            string actualProfileKey, string requiredProfileKey)
        {
            return !String.IsNullOrWhiteSpace(expectedCameraId) &&
                   !String.IsNullOrWhiteSpace(requiredProfileKey) &&
                   String.Equals(expectedCameraId, actualCameraId, StringComparison.Ordinal) &&
                   String.Equals(actualProfileKey, requiredProfileKey, StringComparison.Ordinal) &&
                   Finite(expectedPixelsPerMm) && expectedPixelsPerMm > 0 &&
                   Finite(actualPixelsPerMm) && actualPixelsPerMm > 0 &&
                   Math.Abs(expectedPixelsPerMm - actualPixelsPerMm) <= 0.0001;
        }

        private static bool Finite(double value) =>
            !Double.IsNaN(value) && !Double.IsInfinity(value);
    }
}
