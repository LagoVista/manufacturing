// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b4502b0db030c76457b1dae7030f7dc00401bf0636b63740a0960335dc20b7a1
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp
{
    public class FpBase
    {
        [KicadParserSymbol("layer")]
        public string Layer { get; set; }

        [KicadParserSymbol("locked")]
        public bool IsLocked { get; set; }

        [KicadParserComplexSymbol("stroke")]
        public Stroke Stroke { get; set; } = new Stroke();

        [KicadParserSymbol("uuid")]
        public Guid Uuid { get; set; }

        [KicadParserSymbol("tstamp")]
        public Guid TStamp { get; set; }
    }
}
