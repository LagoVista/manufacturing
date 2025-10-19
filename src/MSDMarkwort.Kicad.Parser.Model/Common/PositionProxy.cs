// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 563f24d14851ec3c9349f38c04f75d317767c553fd554c3d0c9dbeda7a0645db
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class PositionProxy
    {
        [KicadParserComplexSymbol("xy")]
        public Position Position { get; set; } = new Position();
    }
}
