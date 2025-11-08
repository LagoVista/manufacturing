// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 00d5ccd57ffbd07b5189da6c265dc82864f0432c09dc8d3eeac98558725ccb0e
// IndexVersion: 2
// --- END CODE INDEX META ---
using System.Collections.Generic;
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class MultiPointPositionXY
    {
        [KicadParserList("xy", KicadParserListAddType.Complex)]
        public List<Position> Positions { get; set; } = new List<Position>();
    }
}
