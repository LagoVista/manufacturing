// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4c427cd8637e2392869b230e4f3413b4dec3e2dadb5d01d1729e43c4f03ab978
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class PcbLine
    {
        public double W { get; set; }
        public double? Crv { get; set; }

        public string N { get; set; }
        public PCBLayers L { get; set; } 
        public string S { get; set; }

        [JsonIgnore]
        // NOTE: The following are used to generate a full board layout
        public List<PcbLine> StartJunctions { get; set; }
        [JsonIgnore]
        public List<PcbLine> EndJunctions { get; set; }

        [JsonIgnore]
        public ContactRef StartContactRef { get; set; }
        [JsonIgnore]
        public ContactRef EndContactRef { get; set; }


        [JsonIgnore]
        public double Length
        {
            get
            {
                return Math.Sqrt(Math.Pow((X2 - X1), 2) + Math.Pow((Y2 - Y1), 2));
            }
        }

        [JsonIgnore]
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

        public static PcbLine Create(XElement element, string name = null)
        {
            var attrs = element.Attributes();

            var line = new PcbLine()
            {
                L = element.GetInt32("layer").FromEagleLayer(),
                W = element.GetDouble("width"),
                Crv = element.GetDoubleNullable("curve"),
                S = element.GetInt32("layer").FromEagleColor(),
                N = name,
            };

            var rect = Rect.Create(element);
            line.X1 = rect.X1;
            line.X2 = rect.X2;
            line.Y1 = rect.Y1;
            line.Y2 = rect.Y2;

            return line;
        }

        public static PcbLine Create(FpLine line)
        {
            // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
            return new PcbLine()
            {
                L = line.Layer.FromKiCadLayer(),
                X1 = line.StartPosition.X,
                Y1 = -line.StartPosition.Y,
                X2 = line.EndPosition.X,
                Y2 = -line.EndPosition.Y,                
                S = line.Stroke.Color.ToString(line.Layer),
                W = line.Stroke.Width,                
            };
        }

        public override string ToString()
        {
            return $"Wire => X1={X1}, Y1={Y1}, X2={X2}, Y2={Y2}, Width={W}, Curve={Crv}";
        }
    }
}
