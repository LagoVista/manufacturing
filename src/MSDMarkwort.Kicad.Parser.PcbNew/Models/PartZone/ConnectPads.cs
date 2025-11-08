// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9a96862887948267146601af811642f082c2cc3e2c782e06b1543114493f2cb4
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartZone
{
    public class ConnectPads
    {
        [KicadParameter(0)]
        public bool Connect { get; set; }

        [KicadParserSymbol("clearance")]
        public double Clearance { get; set; }
    }
}
