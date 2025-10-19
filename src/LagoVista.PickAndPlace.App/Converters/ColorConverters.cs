// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3e15e3b3acdc71f3d56fa246264fb5bba51de1b88e9176281bddabb7ef0529fd
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LagoVista.PickAndPlace.App.Converters
{
    public class ColorEnabledConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if((bool)value)
            {
                return parameter;
            }
            else
            {
                return "Gray";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
