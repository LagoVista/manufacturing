// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 1f9b3003b487d925e0bc4b15f79aac93f00ea003dd19899e817dbb79e08b71de
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Linq;
using System.Reflection;

namespace MSDMarkwort.Kicad.Parser.Base.Parser.Reflection.PropertySetter
{
    internal class BooleanPropertySetter : IPropertySetter
    {
        private readonly string[] _trueValues = new[] { "true", "locked", "hide" };

        public Type TargetType => typeof(bool);

        public void Set(PropertyInfo targetProperty, object target, string value)
        {
            var boolean = _trueValues.Contains(value);

            targetProperty.SetValue(target, boolean);
        }
    }
}
