using System;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Deterministic two-reference-hole guard. Uses the production staging-grid
    /// coordinate convention; never guesses plate geometry from the camera.
    /// </summary>
    public static class StagingPlateReferenceGate
    {
        public static bool ValidateConfiguration(MachineStagingPlate plate, out string reason)
        {
            if (plate == null || !FinitePositive(plate.HoleSpacing))
            {
                reason = "Select a staging plate with valid hole spacing.";
                return false;
            }
            if (!ValidRow(plate.FiducialRow?.Id) ||
                !ValidColumn(plate.FiducialColumn?.Id) ||
                !ValidRow(plate.ReferenceHoleRow1?.Id) ||
                !ValidColumn(plate.ReferenceHoleColumn1?.Id) ||
                !ValidRow(plate.ReferenceHoleRow2?.Id) ||
                !ValidColumn(plate.ReferenceHoleColumn2?.Id))
            {
                reason = "Plate needs a fiducial grid location and two valid reference row/columns.";
                return false;
            }
            if (plate.ReferenceHoleColumn1.Id == plate.ReferenceHoleColumn2.Id &&
                plate.ReferenceHoleRow1.Id == plate.ReferenceHoleRow2.Id)
            {
                reason = "Reference holes must be two distinct grid positions.";
                return false;
            }
            reason = null;
            return true;
        }

        public static bool ValidateTwoHoleMeasurements(MachineStagingPlate plate,
            Point2D<double> measuredFirst, Point2D<double> measuredSecond,
            out string reason)
        {
            if (!ValidateConfiguration(plate, out reason)) return false;
            if (measuredFirst == null || measuredSecond == null ||
                !Finite(measuredFirst.X) || !Finite(measuredFirst.Y) ||
                !Finite(measuredSecond.X) || !Finite(measuredSecond.Y))
            {
                reason = "Measured hole coordinates are missing or nonfinite.";
                return false;
            }

            var firstLocal = StagingPlateUtils.ResolveStagePlateLocation(plate,
                plate.ReferenceHoleColumn1.Id, plate.ReferenceHoleRow1.Id);
            var secondLocal = StagingPlateUtils.ResolveStagePlateLocation(plate,
                plate.ReferenceHoleColumn2.Id, plate.ReferenceHoleRow2.Id);
            var nominalDx = secondLocal.X - firstLocal.X;
            var nominalDy = secondLocal.Y - firstLocal.Y;
            var actualDx = measuredSecond.X - measuredFirst.X;
            var actualDy = measuredSecond.Y - measuredFirst.Y;
            var nominalDistance = Math.Sqrt(nominalDx * nominalDx + nominalDy * nominalDy);
            var measuredDistance = Math.Sqrt(actualDx * actualDx + actualDy * actualDy);
            if (!FinitePositive(nominalDistance) || !FinitePositive(measuredDistance))
            {
                reason = "Reference-hole span is zero or invalid.";
                return false;
            }

            var allowedLengthError = Math.Max(1.0, nominalDistance * 0.02);
            if (Math.Abs(measuredDistance - nominalDistance) > allowedLengthError)
            {
                reason = "Measured reference-hole separation differs by " +
                    Math.Abs(measuredDistance - nominalDistance).ToString("F2") +
                    " mm from configured plate geometry.";
                return false;
            }

            // Orientation may vary a little during plate installation; an
            // effectively reversed or unrelated pair is not a valid alignment.
            var angle = Math.Atan2(nominalDx * actualDy - nominalDy * actualDx,
                nominalDx * actualDx + nominalDy * actualDy);
            if (Math.Abs(angle) > Math.PI / 12)
            {
                reason = "Measured reference-hole orientation differs by more than 15 degrees.";
                return false;
            }

            reason = null;
            return true;
        }

        public static bool WithinWorkArea(Point2D<double> location,
            Point2D<double> workArea)
        {
            return location != null && workArea != null &&
                FinitePositive(workArea.X) && FinitePositive(workArea.Y) &&
                Finite(location.X) && Finite(location.Y) &&
                location.X >= 0 && location.Y >= 0 &&
                location.X <= workArea.X && location.Y <= workArea.Y;
        }

        private static bool ValidRow(string row) =>
            row != null && row.Length == 1 && row[0] >= 'A' && row[0] <= 'Z';
        private static bool ValidColumn(string column) =>
            int.TryParse(column, out var number) && number > 0 && number <= 500;
        private static bool FinitePositive(double value) => Finite(value) && value > 0;
        private static bool Finite(double value) =>
            !double.IsNaN(value) && !double.IsInfinity(value);
    }
}
