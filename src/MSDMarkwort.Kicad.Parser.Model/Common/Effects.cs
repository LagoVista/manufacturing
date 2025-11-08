// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3998db58ffcd4a97e677cefe91c97847a9da3ee6f1d44c82df13f9c9c7f45a91
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Effects
    {
        [KicadParserComplexSymbol("font")]
        public Font Font { get; set; } = new Font();

        [KicadParserComplexSymbol("justify")]
        public Justify Justify { get; set; } = new Justify();

        [KicadParserSymbol("hide", parameterMappings: "hide")]
        public bool Hide { get; set; }

        [KicadParserSymbol("href")]
        public string Href { get; set; }
    }
}
