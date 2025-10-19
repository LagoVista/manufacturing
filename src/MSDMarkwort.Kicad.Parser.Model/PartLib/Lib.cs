// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 350171b5e0d48982839f2a18a54c5286e90cfd1d00ee01471b25fc8eb657e9b3
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.PartLib
{
    public class Lib
    {
        [KicadParserSymbol("name")]
        public string Name { get; set; }

        [KicadParserSymbol("type")]
        public string Type { get; set; }

        [KicadParserSymbol("uri")]
        public string Uri { get; set; }

        [KicadParserSymbol("options")]
        public string Options { get; set; }

        [KicadParserSymbol("descr")]
        public string Description { get; set; }
    }
}
