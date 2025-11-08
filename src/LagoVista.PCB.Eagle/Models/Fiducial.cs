// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 74dc1000265536fed2a344975d21e2e3f075f16ce2143740ae5cda6e19e9d0f3
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PCB.Eagle.Models
{
    public enum FidicualTypes
    {
        Circle,
        BoardEdge,
    }

    public class Fiducial
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double Diameter { get; set; }

        public FidicualTypes FiducialType { get; set; }
    }
}
