// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a0f50c39c9382a2c6e58405fea49a1568fcc0331656581a64c15ddd20ffa5dd9
// IndexVersion: 0
// --- END CODE INDEX META ---
using System.Collections.Generic;
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.Model.Common
{
    public class TitleBlock
    {
        [KicadParserSymbol("title")]
        public string Title { get; set; }

        [KicadParserSymbol("date")]
        public string Date { get; set; }

        [KicadParserSymbol("rev")]
        public string Revision { get; set; }

        [KicadParserSymbol("company")]
        public string Company { get; set; }

        [KicadParserList("comment", KicadParserListAddType.Complex)]
        public List<Comment> Comments { get; set; } = new List<Comment>();
    }
}
