// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 17caf4373f5c9f41124c6098db3534fe000a249d6242f44a6dafdf9e2a876b1b
// IndexVersion: 0
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartGr
{
    public class GrTextBox : GrBase
    {
        [KicadParameter(0)]
        public string Text { get; set; }

        [KicadParserComplexSymbol("start")]
        public Position StartPosition { get; set; } = new Position();

        [KicadParserComplexSymbol("end")]
        public Position EndPosition { get; set; } = new Position();

        [KicadParserComplexSymbol("margins")]
        public Margin Margin { get; set; } = new Margin();

        [KicadParserComplexSymbol("effects")]
        public Effects Effects { get; set; } = new Effects();

        [KicadParserSymbol("border")]
        public bool HasBorder { get; set; }

        public override string ToString()
        {
            return $"{StartPosition.X}/{StartPosition.Y}-{EndPosition.X}/{EndPosition.Y} ({Layer})";
        }
    }
}
