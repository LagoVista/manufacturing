// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4cfe37cc6975365a90d89f2dd03b1a857cd0b48873efd124e5fa2fdfc70db156
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.Common;

namespace MSDMarkwort.Kicad.Parser.PcbNew.PartTarget
{
    public class Target
    {
        [KicadParameter(0)]
        public string Type { get; set; }

        [KicadParserComplexSymbol("at")]
        public PositionAt At { get; set; } = new PositionAt();

        [KicadParserSymbol("size")]
        public double Size { get; set; }

        [KicadParserSymbol("width")]
        public double Width { get; set; }

        [KicadParserSymbol("layer")]
        public string Layer { get; set; }

        [KicadParserSymbol("tstamp")]
        public Guid TStamp { get; set; }

    }
}
//(target plus (at 96.52 144.78) (size 5) (width 0.15) (layer "Edge.Cuts") (tstamp 0656fbe8-d593-4158-9bf3-80d6a1b2e6a0))