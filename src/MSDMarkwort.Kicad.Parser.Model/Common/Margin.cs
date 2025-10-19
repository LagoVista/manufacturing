// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 85fe606c6e0d11feb29a662f7c345cf4e5c2f09e85c1de6a7b886a1dd44efaf5
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Margin
    {
        [KicadParameter(0)]
        public double Left { get; set; }

        [KicadParameter(1)]
        public double Top { get; set; }

        [KicadParameter(2)]
        public double Right { get; set; }

        [KicadParameter(3)]
        public double Bottom { get; set; }
    }
}
