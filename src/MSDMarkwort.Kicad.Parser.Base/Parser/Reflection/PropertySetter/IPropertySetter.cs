// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f39622d8ca068109414c2d1c7cb9e37065a1b14e4d2c4d9a87a185f6704ff9cb
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Reflection;

namespace MSDMarkwort.Kicad.Parser.Base.Parser.Reflection.PropertySetter
{
    internal interface IPropertySetter
    {
        Type TargetType { get; }

        void Set(PropertyInfo targetProperty, object target, string value);
    }
}
