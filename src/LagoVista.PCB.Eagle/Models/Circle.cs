using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Circle
    {
        public EntityHeader<PCBLayers> Layer { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Radius { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public string Stroke { get; set; }
        public string Fill { get; set; }

        public PcbPackage Package { get; set; }

        public static Circle Create(XElement element)
        {
            return new Circle()
            {
                Layer = element.GetInt32("layer").FromEagleLayer(),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                Radius = element.GetDouble("radius"),
                Width = element.GetDouble("width"),
                Stroke = "#FFFFFF",
                Fill = "$00FFFFFF"
            };
        }

        public static Circle Create(FpCircle circle)
        {
            // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
            var cir = new Circle()
            {
                Layer = circle.Layer.FromKiCadLayer(),
                X = circle.Center.X,
                Y = -circle.Center.Y,
                Radius = circle.Width / 2,
                Width = circle.Width,
                Height = circle.Width,
                Fill = circle.Fill,
                Stroke = circle.Stroke.Color.ToString(circle.Layer)
            };



            if(cir.Radius == 0)
            {
                cir.Radius = Math.Max(circle.EndPosition.X - cir.X, circle.EndPosition.Y - cir.Y);
            }

            return cir;
         }
    }
}
