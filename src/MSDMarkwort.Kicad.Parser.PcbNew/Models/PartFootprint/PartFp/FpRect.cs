// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9a8db802c1896515ad8977fbbcfa7b02c2b497b04f90c69fcd148e552eeacccb
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp
{
    public class FpRect : FpBase
    {
        [KicadParserComplexSymbol("start")]
        public Position StartPosition { get; set; } = new Position();

        [KicadParserComplexSymbol("end")]
        public Position EndPosition { get; set; } = new Position();

        [KicadParserSymbol("fill")]
        public string Fill { get; set; }

        [KicadParserSymbol("width")]
        public double Width { get; set; }
        public override string ToString()
        {
            return $"{StartPosition.X}/{StartPosition.Y}-{EndPosition.X}/{EndPosition.Y} ({Layer})";
        }
    }
}
