// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b98bc3768b47da9dd03d30a1ca0d7a5183ee918ca4006299008a1a8cd41925c5
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Globalization;
using System.Reflection;

namespace MSDMarkwort.Kicad.Parser.Base.Parser.Reflection.PropertySetter
{
    internal class DoublePropertySetter : IPropertySetter
    {
        public Type TargetType => typeof(double);

        public void Set(PropertyInfo targetProperty, object target, string value)
        {
            var doubleValue = double.Parse(value, CultureInfo.GetCultureInfo("en-US"));

            targetProperty.SetValue(target, doubleValue);
        }
    }
}
