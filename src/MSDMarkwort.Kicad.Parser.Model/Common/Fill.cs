// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b6943f1dd0bc430b0ef26fb5314f7a95be9b10b819de3a35cfd674f2d1a8eb77
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Fill
    {
        [KicadParserSymbol("type")]
        public string Type { get; set; }

        [KicadParserComplexSymbol("color")]
        public Color Color { get; set; } = new Color();
    }
}
