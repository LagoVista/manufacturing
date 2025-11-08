// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ef64dccec22c7ce5559b9dc4b28c412440e06e00bd30ce5f16360a240a2d853f
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class PositionXYZProxy
    {
        [KicadParserComplexSymbol("xyz")]
        public PositionXYZ PositionXYZ { get; set; } = new PositionXYZ();
    }
}
