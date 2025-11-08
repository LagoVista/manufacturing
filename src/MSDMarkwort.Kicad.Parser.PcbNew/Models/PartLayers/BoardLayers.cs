// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7b66e685b40295d3a0a5a2bb2f37c1d948631c208febd6c388d2f40c54744351
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartLayers
{
    public class BoardLayers
    {
        [KicadParserList("layers", KicadParserListAddType.Complex)]
        public BoardLayerCollection Layers { get; set; } = new BoardLayerCollection();
    }
}
