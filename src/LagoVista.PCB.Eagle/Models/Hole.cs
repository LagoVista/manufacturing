// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 89d7c8e56cf85198ce26e2c05bff90a1e28e06ad1dff12cede63a5a6834dea51
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using System;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Hole
    {
        public PCBLayers Layer { get; set; }

        public double X { get; set; }
        public double Y { get; set; }
        public double D { get; set; }

        public static Hole Create(XElement element)
        {
            return new Models.Hole()
            {
                Layer = PCBLayers.Holes,
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                D = element.GetDouble("drill"),
            };
        }

        public Hole ApplyRotation(double angle)
        {
            var hole = this.MemberwiseClone() as Hole;
            if (angle == 0)
            {
                return hole;
            }

            var rotated = new Point2D<double>(hole.X, hole.Y).Rotate(angle);

            hole.X = Math.Round(rotated.X, 6);
            hole.Y = Math.Round(rotated.Y, 6);

            return hole;
        }


        public override string ToString()
        {
            return $"Hole => X={X}, Y={Y}, Diameter={D};";
        }
    }
}
