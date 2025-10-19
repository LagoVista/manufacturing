// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7d8579fe226ce179b4261c6c8a6ed988afde104015cbf6de957ac38f5d6c63b5
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Position
    {
        [KicadParameter(0)]
        public double X { get; set; }

        [KicadParameter(1)]
        public double Y { get; set; }

        public override string ToString()
        {
            return $"{X}/{Y}";
        }
    }
}
