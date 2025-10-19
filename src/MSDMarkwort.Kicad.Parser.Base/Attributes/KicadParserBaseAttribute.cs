// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 08b9c522147ffa13465cc1643772528527b853a9ef1c2e3688dc108b17a83dca
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;

namespace MSDMarkwort.Kicad.Parser.Base.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class KicadParserBaseAttribute : Attribute
    {
        public string SymbolName { get; protected set; }

        public bool IsComplex { get; protected set; }

        public KicadParserListAddType ListAddType { get; protected set; }

        public KicadParserSymbolSetType SymbolSetType { get; protected set; }

        public string[] ParameterMappings { get; protected set; }
    }
}
