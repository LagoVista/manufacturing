// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 812343b46941f36a18e470a8436c70bb9eb3b38eabd379b2835534ba95f19483
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartPad
{
    public class Drill
    {
        [KicadParameter(0)]
        public string DrillHole { get; set; }

        [KicadParameter(1)]
        public double InnerDiameter { get; set; }

        [KicadParameter(2)]
        public double OuterDiameter { get; set; }

        [KicadParserComplexSymbol("offset")]
        public Position Offset { get; set; } = new Position();
    }
}
