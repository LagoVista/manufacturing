// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 81026d98d01b3a7765e98f651391ed7d0caa8a323caddf385af037421c7f0418
// IndexVersion: 0
// --- END CODE INDEX META ---
namespace MSDMarkwort.Kicad.Parser.Base.Parser.SExpression.Models
{
    public enum SExprTypes
    {
        List,
        Symbol,
        String
    }

    public abstract class SExpr
    {
        public abstract SExprTypes Type { get; }

        public int LineNumber { get; set; }
    }
}
