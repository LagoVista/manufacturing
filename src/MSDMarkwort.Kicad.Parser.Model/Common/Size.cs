// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4e17e44b4bac4bd6f205c571d7e269e9657644f10115587a40fba0deb6dff856
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Size
    {
        [KicadParameter(0)]
        public double Width { get; set; }

        [KicadParameter(1)]
        public double Height { get; set; }

        public override string ToString()
        {
            return $"{Width}/{Height}";
        }
    }
}
