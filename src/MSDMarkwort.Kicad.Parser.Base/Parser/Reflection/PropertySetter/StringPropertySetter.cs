// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 10b0bb3a7bc74df0ff3fd3a77ed72bfa6fc49df9f84e17543c154f3d02dc7764
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Reflection;

namespace MSDMarkwort.Kicad.Parser.Base.Parser.Reflection.PropertySetter
{
    internal class StringPropertySetter : IPropertySetter
    {
        public Type TargetType => typeof(string);

        public void Set(PropertyInfo targetProperty, object target, string value)
        {
            targetProperty.SetValue(target, value);
        }
    }
}
