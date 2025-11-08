// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 2804a82a7f1584e2da2245f031343749e5824e0de3121b2aed829106c98b90f2
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class PositionXYZ
    {
        [KicadParameter(0)]
        public double X { get; set; }

        [KicadParameter(1)]
        public double Y { get; set; }

        [KicadParameter(2)]
        public double Z { get; set; }

        public override string ToString()
        {
            return $"{X}/{Y}/{Z}";
        }
    }
}
