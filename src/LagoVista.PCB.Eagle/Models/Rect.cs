// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f731ae88a4ae1f0f4c0da1a288a7a5e3806d9da03b60adc493db68659dd8eeed
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using Newtonsoft.Json;

namespace LagoVista.PCB.Eagle.Models
{
    public class Rect
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }
        public double W { get; set; }
        public string S { get; set; }
        public PCBLayers L { get; set; }

        [JsonIgnore]
        public double Length
        {
            get
            {
                return Math.Sqrt(Math.Pow((X2 - X1),2) + Math.Pow((Y2 - Y1),2));
            }
        }

        [JsonIgnore]
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
                L = element.GetInt32("layer").FromEagleLayer(),
                X1 = element.GetDouble("x1"),
                X2 = element.GetDouble("x2"),
                Y1 = element.GetDouble("y1"),
                Y2 = element.GetDouble("y2"),
                W = element.GetDouble("width"),
                S = element.GetInt32("layer").FromEagleColor()
            };

            return rect;
        }

        public static Rect Create(FpRect rect)
        {
            // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
            return new Rect()
            {
                L = rect.Layer.FromKiCadLayer(),
                X1 = rect.StartPosition.X,
                Y1 = -rect.StartPosition.Y,
                X2 = rect.EndPosition.X,
                Y2 = -rect.EndPosition.Y,
            };
        }
    }
}
