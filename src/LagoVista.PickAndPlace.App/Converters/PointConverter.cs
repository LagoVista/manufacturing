// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8668af7a53a3b22bf1884cc659528fe0004cec49af929ca5ee2cb5e0a22aa659
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace LagoVista.PickAndPlace.App.Converters
{
    public class PointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Vector3 point3d = (Vector3)value;
            return point3d.ToPoint3D().ToMedia3D();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class ToolTopPointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Vector3 point3d = (Vector3)value;
            return (point3d + new Vector3(0,0,10)).ToPoint3D().ToMedia3D();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
