// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5726fd419db8f5055f7639978b142a6a86a30bf4fbaaa41151affdda0dc28098
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartGr
{
    public class GrPoly : GrBase
    {
        [KicadParserComplexSymbol("pts")]
        public MultiPointPositionXY Pts { get; set; } = new MultiPointPositionXY();

        [KicadParserSymbol("width")]
        public double Width { get; set; }
    }
}
