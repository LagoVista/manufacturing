// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fa28ac0a601e9ebac94ca32ddc20590fe19fb049ba8958e17166ab4df687626a
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartGr
{
    public class GrCurve : GrBase
    {
        [KicadParserComplexSymbol("pts")]
        public MultiPointPositionXY Pts { get; set; } = new MultiPointPositionXY();
    }
}
