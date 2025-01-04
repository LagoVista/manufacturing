using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Models
{
    public class MVLocatedCorner
    {
        public CameraTypes Camera { get; set; }
        public Point2D<double> Position { get; set; }
        public int FoundCount { get; set; }
        bool Centered { get; set; }       
    }
}
