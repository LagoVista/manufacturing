// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f600c31165b6831918773c4eb43daed74f8f4c78061e1af449acc77214e47e7b
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartZone
{
    public class Placement
    {
        [KicadParserSymbol("enabled")]
        public bool Enabled { get; set; }

        [KicadParserSymbol("sheetname")]
        public string SheetName { get; set; }
    }
}
