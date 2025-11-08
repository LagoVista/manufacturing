// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0e1141dc568c62547d1fcb90878cc714592a489bc1eb0416d6e09919f5636fed
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Reflection;

namespace MSDMarkwort.Kicad.Parser.Base.Parser.Reflection.PropertySetter
{
    internal class GuidPropertySetter : IPropertySetter
    {
        public Type TargetType => typeof(Guid);

        public void Set(PropertyInfo targetProperty, object target, string value)
        {
            targetProperty.SetValue(target, Guid.Parse(value));
        }
    }
}
