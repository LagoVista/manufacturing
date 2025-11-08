// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 473ed0d9058dcf3c72bcc74cf6594bd71071bc918bbbbaf794580a83f5925287
// IndexVersion: 2
// --- END CODE INDEX META ---
namespace MSDMarkwort.Kicad.Parser.Base.Parser.SExpression.Models
{
    public class SExprString : SExpr
    {
        public override SExprTypes Type => SExprTypes.String;

        public string Value { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"Value: {Value} (String), Line: {LineNumber}";
        }
    }
}
