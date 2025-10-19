// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a826795cd2d822ee7bc543b4b86888ae17d4d5f573705bcf4150e2481c129163
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Data;

namespace LagoVista.PickAndPlace.App.Converters
{
    public class MessageTypeBackgroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messageType = (StatusMessageTypes)value;
            if (messageType == StatusMessageTypes.Warning)
                return "Blue";
            
            return "White";

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class MessageTypeForegroundColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var messageType = (StatusMessageTypes)value;

            switch(messageType)
            {
                case StatusMessageTypes.FatalError: return "Red";
                case StatusMessageTypes.ReceivedLine:
                case StatusMessageTypes.SentLine: return "Green";
                case StatusMessageTypes.SentLinePriority: return "Red";
                case StatusMessageTypes.Warning: return "Yellow";
            }

            return Brushes.Black;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
