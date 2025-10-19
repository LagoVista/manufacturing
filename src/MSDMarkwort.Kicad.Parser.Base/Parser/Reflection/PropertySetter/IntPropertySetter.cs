// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 39501320280fc8a60870959f9b7d1384b5065920263940e1d0994a4681f239a1
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Globalization;
using System.Reflection;

namespace MSDMarkwort.Kicad.Parser.Base.Parser.Reflection.PropertySetter
{
    internal class IntPropertySetter : IPropertySetter
    {
        public Type TargetType => typeof(int);

        public void Set(PropertyInfo targetProperty, object target, string value)
        {
            var intValue = int.Parse(value, CultureInfo.GetCultureInfo("en-US"));

            targetProperty.SetValue(target, intValue);
        }
    }
}
