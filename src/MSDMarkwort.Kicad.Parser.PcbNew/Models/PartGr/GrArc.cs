// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5ab6499cfb8c72ea093ca4f216da31a76b3286d126c059a7dcf1f54a90830ac3
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartGr
{
    public class GrArc : GrBase
    {
        [KicadParserComplexSymbol("start")]
        public Position StartPosition { get; set; } = new Position();

        [KicadParserComplexSymbol("mid")]
        public Position MidPosition { get; set; } = new Position();

        [KicadParserComplexSymbol("end")]
        public Position EndPosition { get; set; } = new Position();

        [KicadParserSymbol("width")]
        public double Width { get; set; }

        [KicadParserSymbol("angle")]
        public double Angle { get; set; }

        public override string ToString()
        {
            return $"{StartPosition.X}/{StartPosition.Y}-{MidPosition.X / MidPosition.Y}-{EndPosition.X}/{EndPosition.Y} ({Layer})";
        }
    }
}
