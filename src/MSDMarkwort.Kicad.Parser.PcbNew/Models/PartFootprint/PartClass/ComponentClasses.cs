// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 01b2e74cf43be9f41795c88c557d044ee4e4384b635746e7b2490b2ebb3ff3ba
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartClass
{
    public class ComponentClasses
    {
        [KicadParserList("class", KicadParserListAddType.Complex)]
        public ClassCollection ClassCollection { get; set; } = new ClassCollection();
    }
}
