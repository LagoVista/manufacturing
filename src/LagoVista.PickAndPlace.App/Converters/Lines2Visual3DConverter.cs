// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c5414707d7006584be2449e32a72a9b8be7348a5e042512f0597f6c0b1d4f098
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.Converters
{
    public class Lines2Visual3DConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            /* No lines, OK, don't render anything */
            if(value == null)
            {
                return null;
            }

            var inputPoints = value as ObservableCollection<Line3D>;
            if (inputPoints == null)
            {
                /* If not a list of points...bug, so throw exception */
                throw new Exception("Invalid Convesion Point, must provide ObservableCollection of Line3D");
            }

            var outputPoints = new Point3DCollection();
            foreach (var inputPoint in inputPoints)
            {
                outputPoints.Add(new Point3D() { X = inputPoint.Start.X, Y = inputPoint.Start.Y, Z = inputPoint.Start.Z });
                outputPoints.Add(new Point3D() { X = inputPoint.End.X, Y = inputPoint.End.Y, Z = inputPoint.End.Z });
            }
            return outputPoints;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
