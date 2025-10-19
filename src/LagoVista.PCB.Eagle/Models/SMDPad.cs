// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4b1aade36dd4455a71ab86d87b600a9d1a305c7f189ed6b41587d2d5caba8444
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.Model.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class SMDPad
    {
        public PCBLayers Layer { get; set; }
        public string N { get; set; }
        public double OrgX { get; set; }
        public double OrgY { get; set; }

        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double? Rnd { get; set; }
        public double A { get; set; }
        public string Shp { get; set; }
        public string F { get; set; }

        public SMDPad ApplyRotation(double angle)
        {
            var smd = this.MemberwiseClone() as SMDPad;
            if (angle == 0)
            {
                return smd;
            }

            //TODO: Why do we ignore the rotation at the package level?  If it's not 90, do we rotate then?
            /*if (RotateStr.StartsWith("R"))
            {
                if (String.IsNullOrEmpty(RotateStr))
                {
                    return pad;
                };

                double angle;
                if (double.TryParse(RotateStr.Substring(1), out angle))
                {*/
            var rotatedStart = new Point2D<double>(X1, Y1).Rotate(angle);
            var rotatedEnd = new Point2D<double>(X2, Y2).Rotate(angle);

            smd.X1 = rotatedStart.X;
            smd.Y1 = rotatedStart.Y;

            smd.X2 = rotatedEnd.X;
            smd.Y2 = rotatedEnd.Y;

            return smd;
        }

        public static SMDPad Create(XElement element)
        {
            var attr = element.Attributes();
            var smd = new SMDPad()
            {
                Layer = element.GetInt32("layer").FromEagleLayer(),
                N = element.GetString("name"),
                OrgX = element.GetDouble("x"),
                OrgY = element.GetDouble("y"),
                DX = element.GetDouble("dx"),
                DY = element.GetDouble("dy"),
                Rnd = element.GetDoubleNullable("roundness"),
                A = element.GetString("rot").ToAngle(),
                F = element.GetInt32("layer").FromEagleColor(),
            };

            smd.Shp = smd.Rnd > 0 ? "roundrect" : "rect";

            smd.X1 = Math.Round(smd.OrgX - (smd.DX / 2), 3);
            smd.Y1 = Math.Round(smd.OrgY - (smd.DY / 2), 3);
            smd.X2 = Math.Round(smd.X1 + smd.DX, 3);
            smd.Y2 = Math.Round(smd.Y1 + smd.DY, 3);

            return smd;
        }

        public static List<SMDPad> Create(MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartPad.Pad pad, double fpAngle)
        {
            

            var pads = new List<SMDPad>();
            foreach (var layer in pad.Layers)
            {
                var smd = new SMDPad()
                {
                    Layer = pad.Layers.FirstOrDefault().FromKiCadLayer(),
                    N = pad.PadNumber,
                    OrgX = pad.PositionAt.X,
                    OrgY = -pad.PositionAt.Y,
                    DX = pad.Size.Width,
                    DY = pad.Size.Height,
                    A = (pad.PositionAt.Angle - fpAngle),                    
                };

                smd.X1 = Math.Round(smd.OrgX - (smd.DX / 2), 3);
                smd.Y1 = Math.Round(smd.OrgY - (smd.DY / 2), 2);
                smd.X2 = Math.Round(smd.X1 + smd.DX, 3);
                smd.Y2 = Math.Round(smd.Y1 + smd.DY, 3);
                smd.Shp = pad.Shape;
                smd.F = MSDMarkwort.Kicad.Parser.Model.Common.Color.LayerToColor(layer);
                pads.Add(smd);
            }

            return pads;
        }
    }
}
