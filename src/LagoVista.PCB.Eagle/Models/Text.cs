using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp;
using System;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Text
    {
        public EntityHeader<PCBLayers> Layer { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public string Value { get; set; }
        public double Size { get; set; }
        public double Rotation { get; set; }
        public string Storke { get; set; }


        public static Text Create(XElement element)
        {
            var sttrs = element.Attributes();

            var text = new Text()
            {
                Layer = element.GetInt32("layer").FromEagleLayer(),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                Value = element.Value,
                Size = element.GetDouble("size"),                
                Rotation = element.GetString("rot")?.ToAngle() ?? 0
            };            

            return text;
        }

        public static Text Create(FpText text)
        {
            // Note KiCad has origin at top of PCB, we normalize everything to be at bottom left, therefore just negate the Y values since they are relative to origin.
            return new Text()
            {
                Layer = text.Layer.FromKiCadLayer(),
                X = text.PositionAt.X,
                Y = -text.PositionAt.Y,
                Value = text.Text,
                Storke = text.Stroke.Color.ToString(text.Layer)
            };
        }
    }
}
