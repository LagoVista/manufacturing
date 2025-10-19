// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 01f2b2f069f75cd8eb6fded6cc7221d503e0e05d64cff82cef7da20e26758b8d
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Reflection;

namespace MSDMarkwort.Kicad.Parser.Base.Parser.Reflection.PropertySetter
{
    internal class ByteArrayPropertySetter : IPropertySetter
    {
        public Type TargetType => typeof(byte[]);

        public void Set(PropertyInfo targetProperty, object target, string value)
        {
            targetProperty.SetValue(target, Convert.FromBase64String(value));
        }
    }
}
