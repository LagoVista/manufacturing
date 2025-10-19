// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9fb39b49ba8e4ed7e51850433ebdae54c89aadb6d0b8a11d699dd8dcc0cec087
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartLibFootprint
{
    public class LibFootprint : Footprint
    {
        [KicadParserSymbol("version")]
        public int Version { get; set; }

        [KicadParserSymbol("generator")]
        public string Generator { get; set; }

        [KicadParserSymbol("generator_version")]
        public string GeneratorVersion { get; set; }
    }
}
