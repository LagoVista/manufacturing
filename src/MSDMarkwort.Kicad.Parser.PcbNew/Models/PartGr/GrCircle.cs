// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 03fd18fa58557bfe83d7fa1d12771643f20027a976526ad4b60fcc5853fedbdf
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartGr
{
    public class GrCircle : GrBase
    {
        [KicadParameter(0)]
        public string Locked { get; set; }

        [KicadParserComplexSymbol("center")]
        public Position CenterPosition { get; set; } = new Position();

        [KicadParserComplexSymbol("end")]
        public Position EndPosition { get; set; } = new Position();

        [KicadParserSymbol("width")]
        public double Width { get; set; }

        public override string ToString()
        {
            return $"{CenterPosition.X}/{CenterPosition.Y}-{EndPosition.X}/{EndPosition.Y} ({Layer})";
        }
    }
}
