// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8073572ccbf6c77f1b673fc9e1cf1d3e0f65afe583dbdf26d645d63aed116106
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartPad
{
    public class Options
    {
        [KicadParserSymbol("clearance")]
        public string Clearance { get; set; }

        [KicadParserSymbol("anchor")]
        public string Anchor { get; set; }
    }
}
