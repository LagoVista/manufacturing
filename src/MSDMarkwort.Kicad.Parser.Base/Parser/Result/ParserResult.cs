// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c6c9727b13462cb56d267d0a4c290808fc6b33c681c51c96e06e77351ca6207f
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;

namespace MSDMarkwort.Kicad.Parser.Base.Parser.Result
{
    public class ParserResult<T> where T : class
    {
        public bool Success { get; set; }

        public T Result { get; set; }

        public ParserWarning[] Warnings { get; set; } = Array.Empty<ParserWarning>();

        public ParserError Error { get; set; }
    }
}
