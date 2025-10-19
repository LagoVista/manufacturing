// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c57aaa92a67de203b5fed757f081894493ef1b39f7cabdd27f5d269e0977ee91
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartZone
{
    public class Hatch
    {
        [KicadParameter(0)]
        public string Type { get; set; }

        [KicadParameter(1)]
        public double Width { get; set; }
    }
}
