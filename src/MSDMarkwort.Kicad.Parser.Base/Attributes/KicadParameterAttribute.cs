// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c7a597dff9a7653bc0932cd103c1f5cb4c7d95ea9f184ea432cd14aef9147720
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;

namespace MSDMarkwort.Kicad.Parser.Base.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class KicadParameterAttribute : Attribute
    {
        public int Number { get; set; }

        public KicadParameterAttribute(int number)
        {
            Number = number;
        }
    }
}
