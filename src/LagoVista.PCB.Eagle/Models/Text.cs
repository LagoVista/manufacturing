using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp;
using System;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Text
    {
        public PCBLayers L { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string V { get; set; }
        public double Sz { get; set; }
        public double A { get; set; }
        public string S { get; set; }


        public static Text Create(XElement element)
        {
            var sttrs = element.Attributes();

            var text = new Text()
            {
                L = element.GetInt32("layer").FromEagleLayer(),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                V = element.Value,
                Sz = element.GetDouble("size"),
                A = element.GetString("rot")?.ToAngle() ?? 0,
                S = element.GetInt32("layer").FromEagleColor()
            };            

            return text;
        }

        public static Text Create(FpText text)
        {
            // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
            return new Text()
            {
                L = text.Layer.FromKiCadLayer(),
                X = text.PositionAt.X,
                Y = -text.PositionAt.Y,
                V = text.Text,
                S = text.Stroke.Color.ToString(text.Layer)
            };
        }
    }
}
