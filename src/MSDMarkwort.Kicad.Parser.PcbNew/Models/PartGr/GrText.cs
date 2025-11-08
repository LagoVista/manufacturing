// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f41fb80786f943ddcc2a1d73a2f7ca8e8f6b3151743aa2a7c9992dc7701f61a8
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartGr
{
    public class GrText : GrBase
    {
        [KicadParameter(0)]
        public string Text { get; set; }

        [KicadParserComplexSymbol("at")]
        public PositionAt Position { get; set; } = new PositionAt();

        [KicadParserComplexSymbol("effects")]
        public Effects Effects { get; set; } = new Effects();

        [KicadParserComplexSymbol("render_cache")]
        public RenderCache RenderCache { get; set; } = new RenderCache();

        public override string ToString()
        {
            return $"{Text} - {Position.X}/{Position.Y} ({Layer})";
        }
    }
}
