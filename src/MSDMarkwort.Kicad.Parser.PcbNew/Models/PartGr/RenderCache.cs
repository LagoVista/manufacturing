// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c3b6eb8525585da260e66ebdfe099961238e162ab1bef3593106abbb788a1317
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartGr
{
    public class RenderCache
    {
        [KicadParameter(0)]
        public string Text { get; set; }

        [KicadParameter(1)]
        public string Angle { get; set; }

        [KicadParserList("polygon", KicadParserListAddType.Complex)]
        public PolygonCollection Polygons { get; set; } = new PolygonCollection();
    }
}
