// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a78f8497c61d10a630fa0addab0ac4eaaa1bdec1a5f7daa7b766736fc8820821
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartGr
{
    public class Polygon
    {
        [KicadParserComplexSymbol("pts")]
        public MultiPointPositionXY Pts { get; set; } = new MultiPointPositionXY();
    }
}
