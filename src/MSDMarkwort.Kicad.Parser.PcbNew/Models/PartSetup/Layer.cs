// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1871e12539fd94a63a656710e9011c0ec42e74c3c1e5324bdf27764ad3eda3c6
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartSetup
{
    public class Layer
    {
        [KicadParameter(0)]
        public string Name { get; set; }

        [KicadParserSymbol("type")]
        public string Type { get; set; }

        [KicadParserSymbol("thickness")]
        public double Thickness { get; set; }

        [KicadParserSymbol("color")]
        public string Color { get; set; }

        [KicadParserSymbol("material")]
        public string Material { get; set; }

        [KicadParserSymbol("epsilon_r")]
        public double EpsilonR { get; set; }

        [KicadParserSymbol("loss_tangent")]
        public double LossTangent { get; set; }
    }
}
