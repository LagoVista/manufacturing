// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b6593d74fddf5c70200ee8de1cb1e7f966c69404bb088ae677e39eaa6e8fe428
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartKicadPcb
{
    public class General
    {
        [KicadParserSymbol("thickness")]
        public double Thickness { get; set; }

        [KicadParserSymbol("legacy_teardrops")]
        public bool LegacyTearDrops { get; set; }
    }
}
