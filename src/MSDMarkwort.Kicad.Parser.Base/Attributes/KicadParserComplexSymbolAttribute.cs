// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d060a218b51e39ecb4736fbf9cf878c177fbb00400e05ed42e97dd9099c58eec
// IndexVersion: 2
// --- END CODE INDEX META ---
namespace MSDMarkwort.Kicad.Parser.Base.Attributes
{
    public class KicadParserComplexSymbolAttribute : KicadParserSymbolAttribute
    {
        public KicadParserComplexSymbolAttribute(string symbolName) : base(symbolName)
        {
            IsComplex = true;
        }
    }
}
