// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c210eae45cef4b6f469f02623db5796d46996f22aa5fa474840c1f8f2b7ee585
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class Format
    {
        [KicadParameter(0)]
        public string SuppressZeroes { get; set; }

        [KicadParserSymbol("prefix")]
        public string Prefix { get; set; }

        [KicadParserSymbol("suffix")]
        public string Suffix { get; set; }

        [KicadParserSymbol("units")]
        public int Units { get; set; }

        [KicadParserSymbol("units_format")]
        public int UnitsFormat { get; set; }

        [KicadParserSymbol("precision")]
        public int Precision { get; set; }

        [KicadParserSymbol("override_value")]
        public string OverrideValue { get; set; }
    }
}
