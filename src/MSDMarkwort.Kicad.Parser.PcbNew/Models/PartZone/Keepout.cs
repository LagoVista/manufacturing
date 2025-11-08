// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b860fc8db84c6592106a563a779a9a3f18be1f58ec1ad728b8e9a58846e6b608
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartZone
{
    public class Keepout
    {
        [KicadParserSymbol("tracks")]
        public string Tracks { get; set; }

        [KicadParserSymbol("vias")]
        public string Vias { get; set; }

        [KicadParserSymbol("pads")]
        public string Pads { get; set; }

        [KicadParserSymbol("copperpour")]
        public string CopperPour { get; set; }

        [KicadParserSymbol("footprints")]
        public string Footprints { get; set; }
    }
}
