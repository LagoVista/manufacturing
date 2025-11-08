// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3fe238115c5bf7f051fa0339e450b2b42dbdb90396f662cce3290297486885eb
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Stroke
    {
        [KicadParserSymbol("width")]
        public double Width { get; set; }

        [KicadParserSymbol("type")]
        public string Type { get; set; }

        [KicadParserComplexSymbol("color")]
        public Color Color { get; set; } = new Color();
    }
}
