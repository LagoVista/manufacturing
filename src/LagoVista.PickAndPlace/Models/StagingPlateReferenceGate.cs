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

        /// <summary>
        /// Explain a rejected target using the same exact numerical work-area
        /// envelope that guards motion. This intentionally does not widen the
        /// software limit, guess hardware travel, or silently clamp a target.
        /// </summary>
        public static string DescribeTravelViolation(Point2D<double> target,
            Point2D<double> workArea, string referenceName)
        {
            var name = string.IsNullOrWhiteSpace(referenceName)
                ? "Reference target" : referenceName;
            if (workArea == null || !FinitePositive(workArea.X) ||
                !FinitePositive(workArea.Y))
                return name + ": configured machine XY travel is invalid.";
            if (target == null || !Finite(target.X) || !Finite(target.Y))
                return name + ": computed XY target is missing or invalid.";

            var message = name + " target X=" + target.X.ToString("F3") +
                ", Y=" + target.Y.ToString("F3") + " mm. Configured range: X=0.." +
                workArea.X.ToString("F3") + ", Y=0.." +
                workArea.Y.ToString("F3") + " mm.";

            if (WithinWorkArea(target, workArea))
                return message + " Target is inside configured travel.";

            var xShortfall = target.X < 0 ? -target.X :
                Math.Max(0, target.X - workArea.X);
            var yShortfall = target.Y < 0 ? -target.Y :
                Math.Max(0, target.Y - workArea.Y);

            return message +
                " OUTSIDE configured travel by X=" + xShortfall.ToString("F3") +
                ", Y=" + yShortfall.ToString("F3") + " mm" +
                (target.X < 0 || target.Y < 0
                    ? " (negative coordinate: expanding the upper limit will not fix this)."
                    : ".") +
                " Verify plate grid/spacing and machine origin. Only change " +
                "Work Area Size after confirming actual physical travel. No move issued.";
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
