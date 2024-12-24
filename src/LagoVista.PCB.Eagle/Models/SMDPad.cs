﻿using LagoVista.Core.Models;
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
        public EntityHeader<PCBLayers> Layer { get; set; }
        public string Name { get; set; }
        public double OriginX { get; set; }
        public double OriginY { get; set; }

        public double X1 { get; set; }
        public double Y1 { get; set; }
        public double X2 { get; set; }
        public double Y2 { get; set; }
        public double DX { get; set; }
        public double DY { get; set; }
        public double? Roundness { get; set; }
        public double Rotation { get; set; }
        public string Shape { get; set; }
        public string Fill { get; set; }

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
                Name = element.GetString("name"),
                OriginX = element.GetDouble("x"),
                OriginY = element.GetDouble("y"),
                DX = element.GetDouble("dx"),
                DY = element.GetDouble("dy"),
                Roundness = element.GetDoubleNullable("roundness"),
                Rotation = element.GetString("rot").ToAngle(),
                Fill = element.GetInt32("layer").FromEagleColor(),
            };

            smd.Shape = smd.Roundness > 0 ? "roundrect" : "rect";

            smd.X1 = Math.Round(smd.OriginX - (smd.DX / 2), 3);
            smd.Y1 = Math.Round(smd.OriginY - (smd.DY / 2), 3);
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
                    Name = pad.PadNumber,
                    OriginX = pad.PositionAt.X,
                    OriginY = -pad.PositionAt.Y,
                    DX = pad.Size.Width,
                    DY = pad.Size.Height,
                    Rotation = (pad.PositionAt.Angle - fpAngle),                    
                };

                smd.X1 = Math.Round(smd.OriginX - (smd.DX / 2), 3);
                smd.Y1 = Math.Round(smd.OriginY - (smd.DY / 2), 2);
                smd.X2 = Math.Round(smd.X1 + smd.DX, 3);
                smd.Y2 = Math.Round(smd.Y1 + smd.DY, 3);
                smd.Shape = pad.Shape;
                smd.Fill = MSDMarkwort.Kicad.Parser.Model.Common.Color.LayerToColor(layer);
                pads.Add(smd);
            }

            return pads;
        }
    }
}
