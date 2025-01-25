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
            var delta = expected - actual;
            var x = (holeLocation.X - referenceOffset.X);
            var y = (holeLocation.Y - referenceOffset.Y);

            var ratioX = x / actual.X;
            var ratioY = y / actual.Y;

            x -= ratioX * delta.X;
            y -= ratioY * delta.Y;

            return new Point2D<double>()
            {
                X = x + plate.ReferenceHoleLocation1.X,
                Y = y + plate.ReferenceHoleLocation1.Y
            };
        }
    }
}
