using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Color
    {
        [KicadParameter(0)]
        public int Red { get; set; }

        [KicadParameter(1)]
        public int Green { get; set; }

        [KicadParameter(2)]
        public int Blue { get; set; }

        [KicadParameter(3)]
        public double Alpha { get; set; }

        public override string ToString()
        {
            return $"#{Alpha:0x00}{Red:0x00}{Green:0x00}{Blue:0x00}";
        }
    }
}
