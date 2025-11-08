// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: ebb6e4396b6dce8b78d2d4160b4880c27b7be71223300b056c8cbfd657ba68b8
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartSetup
{
    public class Stackup
    {
        [KicadParserList("layer", KicadParserListAddType.Complex)]
        public LayerCollection Layers { get; set; } = new LayerCollection();

        [KicadParserSymbol("copper_finish")]
        public string CopperFinish { get; set; }

        [KicadParserSymbol("dielectric_constraints")]
        public bool DielectricConstraints { get; set; }
    }
}
