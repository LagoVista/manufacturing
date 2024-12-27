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

        public double RowToY(EntityHeader row)
        {
            return (Convert.ToByte(row.Id.ToCharArray()[0]) - 64) * _plate.HoleSpacing / 2;            
        }

        public double ColToX(EntityHeader col)
        {
            var idx = Convert.ToByte(col.Id);
            return idx * (_plate.HoleSpacing / 2);
        }

        public Point2D<double> ResolveStagePlateLocation(EntityHeader col, EntityHeader row)
        {
            return new Point2D<double>(ColToX(col), RowToY(row));
        }

        public Point2D<double> ResolveStagePlateWorkSpakeLocation(EntityHeader col, EntityHeader row)
        {
            var holeLocation = new Point2D<double>(ColToX(col), RowToY(row));
            var referenceOffset = new Point2D<double>(ColToX(_plate.ReferenceHoleColumn1), RowToY(_plate.ReferenceHoleRow1));

            return new Point2D<double>()
            {
                X = (holeLocation.X - referenceOffset.X) + _plate.ReferenceHoleLocation1.X,
                Y = (holeLocation.Y - referenceOffset.Y) + _plate.ReferenceHoleLocation1.Y
            };
        }
    }
}
