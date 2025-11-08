// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 41806fcfb74eefc31afc3231f5cb10950e356cd9f5edc5296c7b2333e952be40
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartSetup
{
    public class Tenting
    {
        [KicadParameter(0)]
        public bool TentViaFront { get; set; }

        [KicadParameter(1)]
        public bool TentViaBack { get; set; }
    }
}
