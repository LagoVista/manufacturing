// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 345af2f8eb33fe1fcedd8ecd5766310ffd5ad8a82e4ee12f2d46dfc021d2eb46
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Comment
    {
        [KicadParameter(0)]
        public int Number { get; set; }

        [KicadParameter(1)]
        public string Text { get; set; }
    }
}
