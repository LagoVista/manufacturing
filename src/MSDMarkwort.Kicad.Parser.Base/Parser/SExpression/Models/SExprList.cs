// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cabbe77d7024ab8c91977e549d1fffa57ca82113145e5c3b40e0c089dc33cd34
// IndexVersion: 2
// --- END CODE INDEX META ---
using System.Collections.Generic;

namespace MSDMarkwort.Kicad.Parser.Base.Parser.SExpression.Models
{
    public class SExprList : SExpr
    {
        public override SExprTypes Type => SExprTypes.List;

        public List<SExpr> Children { get; set; } = new List<SExpr>();

        public override string ToString()
        {
            return $"Children: {Children.Count}, Line: {LineNumber}";
        }
    }
}
