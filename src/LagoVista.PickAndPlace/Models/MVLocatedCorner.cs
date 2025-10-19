// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e18de934bc816a62651a4e0baa0622c659396af9e669bf1a5c3b09b50c6971d5
// IndexVersion: 0
// --- END CODE INDEX META ---
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

        public int Iteration { get; set;}
    }
}
