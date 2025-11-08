// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e41214fd0a4526f507e199999fa8044371fdb9453fb4c100bb2e214271f0029f
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Justify
    {
        [KicadParameter(0)]
        public string HorizontalAlignment { get; set; }

        [KicadParameter(1)]
        public string VerticalAlignment { get; set; }

        [KicadParameter(2)]
        public string Mirror { get; set; }
    }
}
