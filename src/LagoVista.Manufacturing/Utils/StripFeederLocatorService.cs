using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using RingCentral;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LagoVista.Manufacturing.Util
{
    public class StripFeederLocatorService
    {
        private readonly LagoVista.Manufacturing.Models.Machine _machine;
        public StripFeederLocatorService(LagoVista.Manufacturing.Models.Machine machine)
        {
            _machine = machine;
        }


        public InvokeResult<Point2D<double>> GetStripFeederOrigin(StripFeeder stripFeeder)
        {
            var plate = _machine.StagingPlates.SingleOrDefault(sp => sp.Id == stripFeeder.StagingPlate.Id);
            if (plate == null)
            {
                return InvokeResult<Point2D<double>>.FromError($"Could not find staging plate for {stripFeeder.Name}");
            }

            var spUtils = new StagingPlateUtils(plate);

            var location = spUtils.ResolveStagePlateLocation(stripFeeder.ReferenceHoleColumn, stripFeeder.ReferenceHoleRow);
            if (stripFeeder.Orientation.Value == FeederOrientations.Horizontal)
            {
                location.X -= stripFeeder.ReferenceHoleLocation.X;
                location.Y -= stripFeeder.ReferenceHoleLocation.Y;
            }
            else if (stripFeeder.Orientation.Value == FeederOrientations.Vertical)
            {
                location.X -= stripFeeder.ReferenceHoleLocation.Y;
                location.Y -= stripFeeder.ReferenceHoleLocation.X;
            }
            else
            {
                return InvokeResult<Point2D<double>>.FromError($"Unsupported strip feeder orientation {stripFeeder.Orientation.Value}");
            }

            return InvokeResult<Point2D<double>>.Create(location);
        }

        public InvokeResult<Point3D<double>> GetPartPickLocation(StripFeeder stripFeeder, Component component)
        {
            var feederOririn = GetStripFeederOrigin(stripFeeder);

            var row = stripFeeder.Rows.FirstOrDefault(row => row.Component.Id == component.Id);
            if(row == null)
            {
                return InvokeResult<Point3D<double>>.FromError($"Could not find row for component {component.Name}");
            }    

            var rowIndex = stripFeeder.Rows.IndexOf(row);
            var rowOffset = rowIndex * 5;

            var response = GetStripFeederOrigin(stripFeeder);
            if(!response.Successful)
            {
                return InvokeResult<Point3D<double>>.FromInvokeResult(response.ToInvokeResult());
            }

            var location = response.Result;
            
            if (stripFeeder.Orientation.Value == FeederOrientations.Horizontal)
            {
                location.X -= stripFeeder.ReferenceHoleLocation.X + (component.ComponentPackage.Value.SpacingX.Value * row.CurrentPartIndex) + component.ComponentPackage.Value.CenterX.Value;
                location.Y -= (stripFeeder.ReferenceHoleLocation.Y + component.ComponentPackage.Value.CenterY.Value);
            }
            else if (stripFeeder.Orientation.Value == FeederOrientations.Vertical)
            {
                location.X -= stripFeeder.ReferenceHoleLocation.Y - (component.ComponentPackage.Value.SpacingX.Value * row.CurrentPartIndex) + component.ComponentPackage.Value.CenterX.Value;
                location.Y -= stripFeeder.ReferenceHoleLocation.X + component.ComponentPackage.Value.CenterY.Value;
            }
            else
            {
                return InvokeResult<Point3D<double>>.FromError($"Unsupported strip feeder orientation {stripFeeder.Orientation.Value}");
            }
            

            return InvokeResult<Point3D<double>>.Create(new Point3D<double>(location.X, location.Y,stripFeeder.PickHeight));
        }
    }
}
