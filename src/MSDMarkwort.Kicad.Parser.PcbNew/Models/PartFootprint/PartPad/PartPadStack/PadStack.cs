// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 51af9fd485ff7a1dc276c9ec39276166ba5eb1d4794cba105a4cac2224ddc18b
// IndexVersion: 0
// --- END CODE INDEX META ---
using System.Collections.Generic;
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartPad.PartPadStack
{
    public class PadStack
    {
        [KicadParserSymbol("mode")]
        public string Mode { get; set; }

        [KicadParserList("layer", KicadParserListAddType.Complex)]
        public List<PadStackLayer> Layers { get; set; } = new List<PadStackLayer>();
    }
}
