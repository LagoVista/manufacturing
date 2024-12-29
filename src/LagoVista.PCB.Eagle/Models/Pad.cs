using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.PCB.Eagle.Extensions;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Pad
    {
        public string N { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double D { get; set; }
        public double A { get; set; }
        public string Shp { get; set; }
        public double W { get; set; }
        public double H { get; set; }
        public string F { get; set; }
        public PCBLayers L { get; set; }

        public static List<Pad> Create(XElement element)
        {
            var attr = element.Attributes();

            var pads = new List<Pad>();

            pads.Add(new Pad()
            {
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                D = element.GetDouble("drill"),
                N = element.GetString("name"),
                W = element.GetDouble("drill") * 2,
                H = element.GetDouble("drill") * 2,
                Shp = element.GetString("shape", "Circle"),
                A = element.GetString("rot").ToAngle(),
                L = 1.FromEagleLayer(),
                F =1.FromEagleColor(),
            });

            pads.Add(new Pad()
            {
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                D = element.GetDouble("drill"),
                W = element.GetDouble("drill") * 2,
                H = element.GetDouble("drill") * 2,
                N = element.GetString("name"),
                Shp = element.GetString("shape", "Circle"),
                A = element.GetString("rot").ToAngle(),
                L = 16.FromEagleLayer(),
                F = 16.FromEagleColor()
            });

            return pads;
        }

        public static List<Pad> Create(MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartPad.Pad pad, double fpAngle)
        {
            var pads = new List<Pad>();
            foreach (var layer in pad.Layers)
            {
                if (layer == "*.Cu")
                {
                    // TODO: A little bit of laziness going on here, sorta hack-ish, but I need to get on to what's next KDW - 2024/12/24
                    // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
                    var pd = new Pad()
                    {
                        N = pad.PadNumber,
                        D = double.Parse(pad.Drill.DrillHole),
                        X = pad.PositionAt.X,
                        Y = -pad.PositionAt.Y,
                        A = (pad.PositionAt.Angle - fpAngle),
                        W = pad.Size.Width,
                        H = pad.Size.Height,
                        Shp = pad.Shape,
                        F = MSDMarkwort.Kicad.Parser.Model.Common.Color.LayerToColor("F.Cu"),
                        L = "F.Cu".FromKiCadLayer(),
                    };

                    pads.Add(pd);

                    // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
                    pd = new Pad()
                    {
                        N = pad.PadNumber,
                        D = double.Parse(pad.Drill.DrillHole),
                        X = pad.PositionAt.X,
                        Y = -pad.PositionAt.Y,
                        A = (pad.PositionAt.Angle - fpAngle),
                        W = pad.Size.Width,
                        H = pad.Size.Height,
                        Shp = pad.Shape,
                        F = MSDMarkwort.Kicad.Parser.Model.Common.Color.LayerToColor("B.Cu"),
                        L = "B.Cu".FromKiCadLayer(),
                    };

                    pads.Add(pd);
                }
                else if(layer == "*.Mask")
                {
                    // TODO: A little bit of laziness going on here, sorta hack-ish, but I need to get on to what's next KDW - 2024/12/24
                    // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
                    var pd = new Pad()
                    {
                        N = pad.PadNumber,
                        D = double.Parse(pad.Drill.DrillHole),
                        X = pad.PositionAt.X,
                        Y = -pad.PositionAt.Y,
                        A = (pad.PositionAt.Angle - fpAngle),
                        W = pad.Size.Width,
                        H = pad.Size.Height,
                        Shp = pad.Shape,
                        F = MSDMarkwort.Kicad.Parser.Model.Common.Color.LayerToColor("F.Mask"),
                        L = "F.Mask".FromKiCadLayer(),
                    };

                    pads.Add(pd);

                    // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
                    pd = new Pad()
                    {
                        D = double.Parse(pad.Drill.DrillHole),
                        X = pad.PositionAt.X,
                        Y = -pad.PositionAt.Y,
                        N = pad.PadNumber,
                        A = (pad.PositionAt.Angle - fpAngle),
                        W = pad.Size.Width,
                        H = pad.Size.Height,
                        Shp = pad.Shape,
                        F = MSDMarkwort.Kicad.Parser.Model.Common.Color.LayerToColor("B.Mask"),
                        L = "B.Mask".FromKiCadLayer(),
                    };

                    pads.Add(pd);
                }
                else 
                {
                    // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
                    var pd = new Pad()
                    {
                        D = double.Parse(pad.Drill.DrillHole),
                        X = pad.PositionAt.X,
                        Y = -pad.PositionAt.Y,
                        N = pad.PadNumber,
                        A = (pad.PositionAt.Angle - fpAngle),
                        W = pad.Size.Width,
                        H = pad.Size.Height,
                        Shp = pad.Shape,
                        F = MSDMarkwort.Kicad.Parser.Model.Common.Color.LayerToColor(layer),
                        L = layer.FromKiCadLayer(),
                    };

                    pads.Add(pd);
                }
            }

            return pads;
        }

        public Pad ApplyRotation(double angle)
        {
            var pad = this.MemberwiseClone() as Pad;
            if (angle == 0)
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
