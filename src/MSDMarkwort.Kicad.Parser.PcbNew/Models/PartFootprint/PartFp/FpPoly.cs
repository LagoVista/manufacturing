// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 31e56518ad8e2e8e975eac30ed1930c7550acc14bab32cbef91e89666a24cb9f
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp
{
    public class FpPoly : FpBase
    {
        [KicadParserComplexSymbol("pts")]
        public MultiPointPositionXY Pts { get; set; } = new MultiPointPositionXY();

        [KicadParserSymbol("fill")]
        public string Fill { get; set; }

        [KicadParserSymbol("width")]
        public double Width { get; set; }
    }
}
