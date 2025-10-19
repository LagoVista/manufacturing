// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 6aa281498f485bf5e9ecf8c43984e828dbf11fa527502470042ddb136c3e944f
// IndexVersion: 0
// --- END CODE INDEX META ---
namespace MSDMarkwort.Kicad.Parser.Base.Parser.Result
{
    public class ParserWarning
    {
        public int LineNo { get; set; }

        public ParserWarnings Warning { get; set; }

        public string Information { get; set; }

        public override string ToString()
        {
            return $"In line {LineNo}: {Warning}: {Information}";
        }
    }
}
