// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d1bfbac4437d974a8627615e0e00e2b7248d63c7df8ce211ae0121c749754323
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Util
{
    public static class StagingPlateUtils
    {
        /// <summary>
        /// When an existing reference is assigned to another row/column, carry its
        /// estimated XY forward using the previous valid two-point alignment.
        /// Both estimates must come from the same old alignment, before either
        /// reference location is changed. Do not overwrite explicitly edited XYs.
        /// </summary>
        public static void RebaseChangedReferenceHoles(MachineStagingPlate previous, MachineStagingPlate updated)
        {
            if (previous == null || updated == null ||
                !ValidReference(previous.ReferenceHoleRow1, previous.ReferenceHoleColumn1) ||
                !ValidReference(previous.ReferenceHoleRow2, previous.ReferenceHoleColumn2) ||
                !ValidReference(updated.ReferenceHoleRow1, updated.ReferenceHoleColumn1) ||
                !ValidReference(updated.ReferenceHoleRow2, updated.ReferenceHoleColumn2) ||
                previous.ReferenceHoleLocation1 == null || previous.ReferenceHoleLocation2 == null ||
                updated.ReferenceHoleLocation1 == null || updated.ReferenceHoleLocation2 == null)
                return;

            if (previous.ReferenceHoleRow1.Id == previous.ReferenceHoleRow2.Id &&
                previous.ReferenceHoleColumn1.Id == previous.ReferenceHoleColumn2.Id)
                return;

            if (previous.ReferenceHoleLocation1.X == 0 && previous.ReferenceHoleLocation1.Y == 0 ||
                previous.ReferenceHoleLocation2.X == 0 && previous.ReferenceHoleLocation2.Y == 0)
                return;

            var firstChanged = previous.ReferenceHoleRow1.Id != updated.ReferenceHoleRow1.Id ||
                               previous.ReferenceHoleColumn1.Id != updated.ReferenceHoleColumn1.Id;
            var secondChanged = previous.ReferenceHoleRow2.Id != updated.ReferenceHoleRow2.Id ||
                                previous.ReferenceHoleColumn2.Id != updated.ReferenceHoleColumn2.Id;

            var rebaseFirst = firstChanged &&
                SamePosition(previous.ReferenceHoleLocation1, updated.ReferenceHoleLocation1);
            var rebaseSecond = secondChanged &&
                SamePosition(previous.ReferenceHoleLocation2, updated.ReferenceHoleLocation2);

            // Compute both with the previous, known-good alignment. Never use an
            // already changed reference as input to compute the other reference.
            var first = rebaseFirst
                ? ResolveStagePlateWorkSpaceLocation(previous,
                    updated.ReferenceHoleColumn1.Id, updated.ReferenceHoleRow1.Id)
                : null;
            var second = rebaseSecond
                ? ResolveStagePlateWorkSpaceLocation(previous,
                    updated.ReferenceHoleColumn2.Id, updated.ReferenceHoleRow2.Id)
                : null;

            if (first != null)
                updated.ReferenceHoleLocation1 = first;
            if (second != null)
                updated.ReferenceHoleLocation2 = second;
        }

        private static bool SamePosition(Point2D<double> left, Point2D<double> right)
        {
            return Math.Abs(left.X - right.X) < 0.000001 &&
                   Math.Abs(left.Y - right.Y) < 0.000001;
        }

        private static bool ValidReference(EntityHeader row, EntityHeader column)
        {
            return row?.Id?.Length == 1 && row.Id[0] >= 'A' && row.Id[0] <= 'Z' &&
                   column != null && int.TryParse(column.Id, out var number) && number > 0;
        }

        public static double RowToY(MachineStagingPlate plate, string rowId)
        {
            return (Convert.ToByte(rowId.ToCharArray()[0]) - 64) * plate.HoleSpacing / 2;            
        }

        public static double ColToX(MachineStagingPlate plate, string colId)
        {
            var idx = Convert.ToByte(colId);
            return idx * (plate.HoleSpacing / 2);
        }

        public static Point2D<double> ResolveStagePlateLocation(MachineStagingPlate plate, EntityHeader col, EntityHeader row)
        {
            return new Point2D<double>(ColToX(plate, col.Id), RowToY(plate, row.Id));
        }

        public static Point2D<double> ResolveStagePlateLocation(MachineStagingPlate plate, string colId, string rowId)
        {
            return new Point2D<double>(ColToX(plate, colId), RowToY(plate, rowId));
        }

        public static Point2D<double> ResolveStagePlateWorkSpakeLocation(MachineStagingPlate plate, EntityHeader col, EntityHeader row)
        {
            return ResolveStagePlateWorkSpaceLocation(plate, col.Id, row.Id);
        }

        public static Point2D<double> ResolveStagePlateWorkSpaceLocation(MachineStagingPlate plate, string colId, string rowId)
        {
            var holeLocation = new Point2D<double>(ColToX(plate, colId), RowToY(plate, rowId));
            
            var referenceOffset = new Point2D<double>(ColToX(plate, plate.ReferenceHoleColumn1.Id), RowToY(plate, plate.ReferenceHoleRow1.Id));
            var secondPointOffset = new Point2D<double>(ColToX(plate, plate.ReferenceHoleColumn2.Id), RowToY(plate, plate.ReferenceHoleRow2.Id));

            var expected = secondPointOffset - referenceOffset;
            var actual = plate.ReferenceHoleLocation2 - plate.ReferenceHoleLocation1;
            var lengthSquared = expected.X * expected.X + expected.Y * expected.Y;
            if (lengthSquared == 0)
                throw new ArgumentException("The staging plate reference holes must be different.");

            // Two measured reference holes determine translation, rotation and uniform scale.
            var cosScaled = (expected.X * actual.X + expected.Y * actual.Y) / lengthSquared;
            var sinScaled = (expected.X * actual.Y - expected.Y * actual.X) / lengthSquared;

            var x = holeLocation.X - referenceOffset.X;
            var y = holeLocation.Y - referenceOffset.Y;

            return new Point2D<double>()
            {
                X = plate.ReferenceHoleLocation1.X + cosScaled * x - sinScaled * y,
                Y = plate.ReferenceHoleLocation1.Y + sinScaled * x + cosScaled * y
            };
        }
    }
}
