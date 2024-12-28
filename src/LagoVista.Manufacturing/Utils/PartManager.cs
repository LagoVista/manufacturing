using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LagoVista.Manufacturing.Util
{
    public class PartManager
    {
        private readonly LagoVista.Manufacturing.Models.Machine _machine;
        public PartManager(LagoVista.Manufacturing.Models.Machine machine) 
        { 
            _machine = machine; 
        }

        public List<StripFeeder> StripFeeders { get; set; }
        public List<AutoFeeder> AutoFeeders { get; set; }

        public InvokeResult<Point3D<double>> GetPickLocation(EntityHeader component)
        {
            var feeder = AutoFeeders.FirstOrDefault(fld=>fld.Component?.Id == component.Id);
            if(feeder != null)
            {
                return InvokeResult<Point3D<double>>.Create(new Point3D<double>(feeder.PickLocation.X, feeder.PickLocation.Y, feeder.PickHeight));
            }

            foreach (var fdr in StripFeeders)
            {
                foreach (var row in fdr.Rows)
                {
                    if (row.Component?.Id == component.Id)
                    {
                        var plate = _machine.StagingPlates.SingleOrDefault(plt => plt.Id == fdr.StagingPlate.Id);
                        //plate.ReferenceHoleLocation1.X + 


                        return InvokeResult<Point3D<double>>.Create(new Point3D<double>(0, 0, 0));
                    }
                }
            }

            return InvokeResult<Point3D<double>>.FromError($"Component {component.Text} not found.");
        }
    }
}
