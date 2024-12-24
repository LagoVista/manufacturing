using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.PCB.Eagle.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Pad
    {
        public string Name { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double DrillDiameter { get; set; }
        public string Shape { get; set; }
        public string RotateStr { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string Fill { get; set; }
        public EntityHeader<PCBLayers> Layer { get; set; }

        public static Pad Create(XElement element)
        {
            return new Pad()
            {
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                DrillDiameter = element.GetDouble("drill"),
                Name = element.GetString("name"),
                Shape = element.GetString("shape", "Circle"),
                RotateStr = element.GetString("rot")
            };
        }

        public static List<Pad> Create(MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartPad.Pad pad, double fpAngle)
        {
            
            var pads = new List<Pad>();
            foreach (var layer in pad.Layers)
            {
                // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
                var pd = new Pad()
                {
                    DrillDiameter = double.Parse(pad.Drill.DrillHole),
                    X = pad.PositionAt.X,
                    Y = -pad.PositionAt.Y,
                    Name = pad.PadNumber,
                    RotateStr = (pad.PositionAt.Angle - fpAngle).ToString(),
                    Width = pad.Size.Width,
                    Height = pad.Size.Height,
                    Shape = pad.Shape,
                    Fill = MSDMarkwort.Kicad.Parser.Model.Common.Color.LayerToColor(layer),
                    Layer = layer.FromKiCadLayer(),
                };

                pads.Add(pd);
            }

            return pads;
        }

        public Pad ApplyRotation(double angle)
        {
            var pad = this.MemberwiseClone() as Pad;
            if(angle == 0)
            {
                return pad;
            }
            var rotated = new Point2D<double>(pad.X, pad.Y).Rotate(angle);

            pad.X = Math.Round(rotated.X, 6);
            pad.Y = Math.Round(rotated.Y, 6);

            return pad;
        }
    }
}
