// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 32a163563fffdce5bd1e69405e9926401cde798890d6db9656d125236a2552f9
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartPad
{
    public class Teardrops
    {
        [KicadParserSymbol("best_length_ratio")]
        public double BestLengthRatio { get; set; }

        [KicadParserSymbol("max_length")]
        public double MaxLength { get; set; }

        [KicadParserSymbol("best_width_ratio")]
        public double BestWidthRatio { get; set; }

        [KicadParserSymbol("max_width")]
        public double MaxWidth { get; set; }

        [KicadParserSymbol("curve_points")]
        public double CurvePoints { get; set; }

        [KicadParserSymbol("filter_ratio")]
        public double FilterRatio { get; set; }

        [KicadParserSymbol("enabled")]
        public bool Enabled { get; set; }

        [KicadParserSymbol("allow_two_segments")]
        public bool AllowTwoSegments { get; set; }

        [KicadParserSymbol("prefer_zone_connections")]
        public bool PreferZoneConnections { get; set; }
    }
}
