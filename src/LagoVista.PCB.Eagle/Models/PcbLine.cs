using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class PcbLine
    {
        public string Name { get; set; }
        public double Width { get; set; }
        public double? Curve { get; set; }

        
        public EntityHeader<PCBLayers> Layer { get; set; } 
        public string Stroke { get; set; }

        // NOTE: The following are used to generate a full board layout
        public List<PcbLine> StartJunctions { get; set; }
        public List<PcbLine> EndJunctions { get; set; }

        public ContactRef StartContactRef { get; set; }
        public ContactRef EndContactRef { get; set; }


        public double Length
        {
            get
            {
                return Math.Sqrt(Math.Pow((X2 - X1), 2) + Math.Pow((Y2 - Y1), 2));
            }
        }

        public double Angle
        {
            get
            {
                var angle = (Math.Atan2(Y2 - Y1, X2 - X1) * (180 / Math.PI));
                angle -= 90;
                if (angle < 0)
                    angle += 360;

                return angle;
            }
        }

        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }

        public static PcbLine Create(XElement element)
        {
            var line = new PcbLine()
            {
                Layer = element.GetInt32("layer").FromEagleLayer(),
                Name = element.GetString("name"),
                Width = element.GetDouble("width"),                
                Curve = element.GetDoubleNullable("curve"),
            };

            var rect = Rect.Create(element);
            line.X1 = rect.X1;
            line.X2 = rect.X2;
            line.Y1 = rect.Y1;
            line.Y2 = rect.Y2;

            line.Stroke = "#FFFFFF";

            return line;
        }

        public static PcbLine Create(FpLine line)
        {
            // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
            return new PcbLine()
            {
                Layer = line.Layer.FromKiCadLayer(),
                X1 = line.StartPosition.X,
                Y1 = -line.StartPosition.Y,
                X2 = line.EndPosition.X,
                Y2 = -line.EndPosition.Y,                
                Stroke = line.Stroke.Color.ToString(line.Layer),
                Width = line.Stroke.Width,                
            };
        }

        public override string ToString()
        {
            return $"Wire => X1={X1}, Y1={Y1}, X2={X2}, Y2={Y2}, Width={Width}, Curve={Curve}";
        }
    }
}
