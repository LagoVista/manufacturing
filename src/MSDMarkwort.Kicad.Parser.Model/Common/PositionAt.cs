// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 740666d381aa70a752382747f5af28cd84c94f56205cd01105645ab28d7e6349
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class PositionAt : Position
    {
        [KicadParameter(2)]
        public double Angle { get; set; }

        [KicadParameter(3)]
        public string Locked { get; set; }

        public override string ToString()
        {
            return $"{X}/{Y} ({Angle}°)";
        }
    }
}
