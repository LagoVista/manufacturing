// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0c159291889856720ce310771019e74192d91e019a9c8dc271866944aa4a161d
// IndexVersion: 2
// --- END CODE INDEX META ---
namespace MSDMarkwort.Kicad.Parser.Base.Parser.SExpression.Models
{
    public class SExprSymbol : SExpr
    {
        public override SExprTypes Type => SExprTypes.Symbol;

        public string Value { get; set; }

        public override string ToString()
        {
            return $"Value: {Value} (Symbol), Line: {LineNumber}";
        }

    }
}
