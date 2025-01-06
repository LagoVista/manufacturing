using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces;
using LagoVista.Manufacturing.Models;
using RingCentral;
using System;
using System.Linq;

namespace LagoVista.Manufacturing.Util
{
    public enum LocationType
    {
        FeederOrigin,
        FeederReferenceHole,
        FirstFeederRowReferenceHole,
        LastFeederRowReferenceHole,
        FirstPart,
        LastPart,
        NextPart,
        PreviousPart,
        CurrentPart,
    }

    public enum SetTypes
    {
        CalculatedFeederOrigin,
        FeederOrigin,
        FirstFeederRowReferenceHole,
        LastFeederRowReferenceHole,
        FeederFiducial,
        FeederPickLocation,
    }

    public class StripFeederService
    {
        private readonly IMachineCore _machine;
        public StripFeederService(IMachineCore machine)
        {
            _machine = machine;
        }

        public InvokeResult<Point2D<double>> GetStripFeederOrigin(StripFeeder stripFeeder)
        {
            var plate = _machine.Settings.StagingPlates.SingleOrDefault(sp => sp.Id == stripFeeder.StagingPlate.Id);
            if (plate == null)
            {
                return InvokeResult<Point2D<double>>.FromError($"Could not find staging plate for {stripFeeder.Name}");
            }

            var spUtils = new StagingPlateUtils(plate);

            var location = spUtils.ResolveStagePlateLocation(stripFeeder.ReferenceHoleColumn, stripFeeder.ReferenceHoleRow);
            if (stripFeeder.Orientation.Value == FeederOrientations.Horizontal)
            {
                location.X -= stripFeeder.ReferenceHoleOffset.X;
                location.Y -= stripFeeder.ReferenceHoleOffset.Y;
            }
            else if (stripFeeder.Orientation.Value == FeederOrientations.Vertical)
            {
                location.X -= stripFeeder.ReferenceHoleOffset.Y;
                location.Y -= stripFeeder.ReferenceHoleOffset.X;
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
                location.X -= stripFeeder.ReferenceHoleOffset.X + (component.ComponentPackage.Value.SpacingX.Value * row.CurrentPartIndex) + component.ComponentPackage.Value.CenterX.Value;
                location.Y -= (stripFeeder.ReferenceHoleOffset.Y + component.ComponentPackage.Value.CenterY.Value);
            }
            else if (stripFeeder.Orientation.Value == FeederOrientations.Vertical)
            {
                location.X -= stripFeeder.ReferenceHoleOffset.Y - (component.ComponentPackage.Value.SpacingX.Value * row.CurrentPartIndex) + component.ComponentPackage.Value.CenterX.Value;
                location.Y -= stripFeeder.ReferenceHoleOffset.X + component.ComponentPackage.Value.CenterY.Value;
            }
            else
            {
                return InvokeResult<Point3D<double>>.FromError($"Unsupported strip feeder orientation {stripFeeder.Orientation.Value}");
            }
            

            return InvokeResult<Point3D<double>>.Create(new Point3D<double>(location.X, location.Y,stripFeeder.PickHeight));
        }

        public Point2D<double> CalculateFeederOrigin(StripFeeder stripFeeder)
        {
            throw new NotImplementedException();
        }

        public Point2D<double> CalculateFirstTapeReferenceHOle(StripFeeder stripFeeder, StripFeederRow row)
        {
            throw new NotImplementedException();
        }

        public Point2D<double> CalculateLastTapeReferenceHOle(StripFeeder stripFeeder, StripFeederRow row)
        {
            throw new NotImplementedException();
        }

        public InvokeResult SetLocation(StripFeeder feeder, StripFeederRow row, SetTypes setType)
        {
            switch (setType)
            {
                case SetTypes.FirstFeederRowReferenceHole:
                    row.FirstTapeHoleOffset = feeder.Origin - _machine.MachinePosition.ToPoint2D();
                    break;
                case SetTypes.LastFeederRowReferenceHole:
                    row.LastTapeHoleOffset = feeder.Origin - _machine.MachinePosition.ToPoint2D();
                    break;
            }

            return InvokeResult.Success;
        }

        public InvokeResult<Point2D<double>> GetLocation(StripFeeder stripFeeder, LocationType locatorType)
        {
            switch(locatorType) 
            {

            }

            return InvokeResult<Point2D<double>>.FromError($"Could not resolve location for: {locatorType}");
        }

        public InvokeResult<Point2D<double>> GetLocation(StripFeeder stripFeeder, StripFeederRow row, LocationType locatorType)
        {
            return InvokeResult<Point2D<double>>.FromError($"Could not resolve location for: {locatorType}");
        }
    }
}
