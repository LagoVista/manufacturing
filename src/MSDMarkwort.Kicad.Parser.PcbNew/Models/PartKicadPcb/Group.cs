// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d0c330501b4dd2d211ce7a5185cf12d492dc3db90c60d38eebdd3b58ccf810cd
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using MSDMarkwort.Kicad.Parser.Base.Attributes;

namespace MSDMarkwort.Kicad.Parser.PcbNew.Models.PartKicadPcb
{
    public class Group
    {
        [KicadParameter(0)]
        public string Name { get; set; }

        [KicadParserSymbol("uuid")]
        [KicadParserSymbol("id")]
        public Guid Uuid { get; set; }

        [KicadParserSymbol("locked", parameterMappings: "locked")]
        public bool IsLocked { get; set; }

        [KicadParserList("members", KicadParserListAddType.FromParameters)]
        public List<Guid> Members { get; set; } = new List<Guid>();
    }
}
