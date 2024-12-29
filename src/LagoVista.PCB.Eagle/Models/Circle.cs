using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp;
using System;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Circle
    {
        public PCBLayers L { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double R { get; set; }
        public double W { get; set; }
        public double H { get; set; }

        public string S { get; set; }
        public string F { get; set; }


        public static Circle Create(XElement element)
        {
            var attr = element.Attributes();

            return new Circle()
            {
                L = element.GetInt32("layer").FromEagleLayer(),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                R = element.GetDouble("radius"),
                W = element.GetDouble("width"),
                S = element.GetInt32("layer").FromEagleColor(),
                F = "none"
            };
        }

        public static Circle Create(FpCircle circle)
        {
            // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
            var cir = new Circle()
            {
                L = circle.Layer.FromKiCadLayer(),
                X = circle.Center.X,
                Y = -circle.Center.Y,
                R = circle.Width / 2,
                W = circle.Width,
                H = circle.Width,
                F = circle.Fill,
                S = circle.Stroke.Color.ToString(circle.Layer)
            };



            if(cir.R == 0)
            {
                cir.R = Math.Max(circle.EndPosition.X - cir.X, circle.EndPosition.Y - cir.Y);
            }

            return cir;
         }
    }
}
