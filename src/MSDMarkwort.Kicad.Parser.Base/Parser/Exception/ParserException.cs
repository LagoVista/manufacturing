// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5b18197d243f1f1406cd4f1d702d7998a06c9555da57b5f4f1c086a4931c6b84
// IndexVersion: 2
// --- END CODE INDEX META ---
namespace MSDMarkwort.Kicad.Parser.Base.Parser.Exception
{
    public class ParserException : System.Exception
    {
        public ParserException(int lineNumber, System.Exception innerException)
            : base("Fatal parsing error. See inner exceptions for details.", innerException)
        {
            LineNumber = lineNumber;
        }

        public int LineNumber { get; }
    }
}
