using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using LagoVista.Core;
using System.Diagnostics;
using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp;

namespace LagoVista.PCB.Eagle.Models
{
    public class Rect
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }
        public double Width { get; set; }
        public EntityHeader<PCBLayers> Layer { get; set; }

        public double Length
        {
            get
            {
                return Math.Sqrt(Math.Pow((X2 - X1),2) + Math.Pow((Y2 - Y1),2));
            }
        }

        public double Angle
        {
            get
            {
                var angle = (Math.Atan2(Y2 - Y1, X2 - X1) * (180 / Math.PI));
                angle-= 90;
                if (angle < 0)
                    angle += 360;

                return angle;
            }
        }

        public static Rect Create(XElement element)
        {
            var attr = element.Attributes();

            var rect = new Rect()
            {
                Layer = element.GetInt32("layer").FromEagleLayer(),
                X1 = element.GetDouble("x1"),
                X2 = element.GetDouble("x2"),
                Y1 = element.GetDouble("y1"),
                Y2 = element.GetDouble("y2"),
                Width = element.GetDouble("width")
            };


            return rect;
        }

        public static Rect Create(FpRect rect)
        {
            return new Rect()
            {
                Layer = rect.Layer.FromKiCadLayer(),
                X1 = rect.StartPosition.X,
                Y1 = rect.StartPosition.Y,
                X2 = rect.EndPosition.X,
                Y2 = rect.EndPosition.Y,
            };
        }
    }
}
