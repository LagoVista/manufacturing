// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 95de54bc0332aa49d55b8167ae94fac9e4cc3d4e7348bb043e510a83cbb6711e
// IndexVersion: 2
// --- END CODE INDEX META ---
using MSDMarkwort.Kicad.Parser.Base.Attributes;
using MSDMarkwort.Kicad.Parser.Model.PartLib;

namespace MSDMarkwort.Kicad.Parser.Model.PartLibraryTable
{
    public class LibraryTable
    {
        [KicadParserSymbol("version")]
        public int Version { get; set; }

        [KicadParserList("lib", KicadParserListAddType.Complex)]
        public LibCollection Libraries { get; set; } = new LibCollection();
    }
}
