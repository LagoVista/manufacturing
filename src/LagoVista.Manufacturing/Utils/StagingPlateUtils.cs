using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Util
{
    public class StagingPlateUtils
    {
        private readonly MachineStagingPlate _plate;
        public StagingPlateUtils(MachineStagingPlate plate)
        {
            _plate = plate;
        }

        public double RowToY(string rowId)
        {
            return (Convert.ToByte(rowId.ToCharArray()[0]) - 64) * _plate.HoleSpacing / 2;            
        }

        public double ColToX(string colId)
        {
            var idx = Convert.ToByte(colId);
            return idx * (_plate.HoleSpacing / 2);
        }

        public Point2D<double> ResolveStagePlateLocation(EntityHeader col, EntityHeader row)
        {
            return new Point2D<double>(ColToX(col.Id), RowToY(row.Id));
        }

        public Point2D<double> ResolveStagePlateWorkSpakeLocation(EntityHeader col, EntityHeader row)
        {
            return ResolveStagePlateWorkSpaceLocation(col.Id, row.Id);
        }

        public Point2D<double> ResolveStagePlateWorkSpaceLocation(string colId, string rowId)
        {
            if (_plate.ReferenceHoleColumn1 == null)
                return null;

            var holeLocation = new Point2D<double>(ColToX(colId), RowToY(rowId));
            var referenceOffset = new Point2D<double>(ColToX(_plate.ReferenceHoleColumn1.Id), RowToY(_plate.ReferenceHoleRow1.Id));

            return new Point2D<double>()
            {
                X = (holeLocation.X - referenceOffset.X) + _plate.ReferenceHoleLocation1.X,
                Y = (holeLocation.Y - referenceOffset.Y) + _plate.ReferenceHoleLocation1.Y
            };
        }
    }
}
