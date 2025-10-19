// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5f222699be21ec284458b12b37452d7effae0d812344761d5cad16d14abcbe0c
// IndexVersion: 0
// --- END CODE INDEX META ---
namespace MSDMarkwort.Kicad.Parser.Base.Attributes
{
    public class KicadParserSymbolAttribute : KicadParserBaseAttribute
    {
        public KicadParserSymbolAttribute(string symbolName, KicadParserSymbolSetType symbolSetType = KicadParserSymbolSetType.SetParameter, params string[] parameterMappings)
        {
            SymbolName = symbolName;
            SymbolSetType = symbolSetType;
            IsComplex = false;
            ParameterMappings = parameterMappings;
        }
    }
}
