// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 08f1ce8d7404fb86fed67634294489ae981bd5f914085fc81a0007d746218fe6
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;

namespace MSDMarkwort.Kicad.Parser.Base.Attributes
{
    public class KicadParserListAttribute : KicadParserSymbolAttribute
    {
        public KicadParserListAttribute(string symbolName, KicadParserListAddType listAddType) : base(symbolName)
        {
            ListAddType = listAddType;

            switch (listAddType)
            {
                case KicadParserListAddType.Complex:
                    IsComplex = true;
                    break;
                case KicadParserListAddType.NotSet:
                case KicadParserListAddType.FromParameters:
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(listAddType), listAddType, null);
            }
        }
    }
}
