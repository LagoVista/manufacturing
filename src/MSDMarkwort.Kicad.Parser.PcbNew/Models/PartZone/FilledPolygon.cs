// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c1200fa0cac993d7a589ffeae600eec89f3438094c345bf52218c965d00fbc62
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartZone
{
    public class FilledPolygon
    {
        [KicadParserSymbol("layer")]
        public string Layer { get; set; }

        [KicadParserSymbol("island", KicadParserSymbolSetType.ImplicitBoolTrue)]
        public bool IsIsland { get; set; }

        [KicadParserComplexSymbol("pts")]
        public MultiPointPositionXY Pts { get; set; } = new MultiPointPositionXY();
    }
}
