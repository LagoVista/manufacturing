// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fdca22446a38bd138f30dd5cfc05f19b2eefa8dc845386b6d4279c8f033ccc9d
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Windows.Markup;

namespace FontAwesome.WPF.Converters
{
    /// <summary>
    /// Converts a FontAwesomIcon to its description.
    /// </summary>
    public class DescriptionConverter
        : MarkupExtension, IValueConverter
    {
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is FontAwesomeIcon)) return null;

            var icon = (FontAwesomeIcon) value;

            var memInfo = typeof(FontAwesomeIcon).GetMember(icon.ToString());
            var attributes = memInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes.Length == 0) return null; // alias

            return ((DescriptionAttribute)attributes[0]).Description;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
