// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8a4b7cd9d2f57295c536892e0ced7685eb6c69d567f3006863df4179a6260808
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace LagoVista.PickAndPlace.App.Converters
{
    public class BoolColorConverter : IValueConverter
    {
        private static SolidColorBrush _redBrush = new SolidColorBrush(Colors.Red);
        private static SolidColorBrush _greenBrush = new SolidColorBrush(Colors.Green);

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value is Boolean && (bool)value) ? _redBrush : _greenBrush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
