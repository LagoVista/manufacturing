// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a7e2d29ff15447f797bec43d14565a11860f5368b996fa6d58a0355f42c4f67c
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.ViewModels.PickAndPlace;
using LagoVista.XPlat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LagoVista.PickAndPlace.App.Controls.PickAndPlace
{
    public sealed class FeederDiscoveryStatusConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (values.Length < 5 || !(values[0] is LagoVista.Manufacturing.Models.AutoFeeder feeder) ||
                !(values[1] is IPhotonFeederViewModel vm) || !(values[4] is bool connected) || !connected)
                return "Not scanned";
            var atSlot = vm.DiscoveredFeeders.FirstOrDefault(f => f.Slot == feeder.Slot);
            if (atSlot != null) return string.Equals(atSlot.Address, feeder.FeederId, StringComparison.OrdinalIgnoreCase)
                ? "Discovered" : "ID mismatch";
            if (vm.DiscoveredFeeders.Any(f => string.Equals(f.Address, feeder.FeederId, StringComparison.OrdinalIgnoreCase)))
                return "Discovered in another slot";
            if (feeder.Slot < 1 || feeder.Slot > 254) return "Invalid slot";
            var status = vm.GetDiscoveryStatus((byte)feeder.Slot);
            if (status.StartsWith("Missing", StringComparison.OrdinalIgnoreCase)) return "No response";
            if (status.Contains("communication error")) return "Communication error";
            return "Not scanned";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
            => throw new NotSupportedException();
    }

    /// <summary>
    /// Interaction logic for AutoFeederControl.xaml
    /// </summary>
    public partial class AutoFeederControl : VMBoundUserControl<IAutoFeederViewModel>
    {
        public AutoFeederControl()
        {
            InitializeComponent();
        }

        private void OpenPlacementCalibration_Click(object sender, RoutedEventArgs e)
        {
            if (!(DataContext is IAutoFeederViewModel vm) || vm.PlacementBusy) return;
            var dialog = new AutoFeederCalibrationWindow(vm) { Owner = Window.GetWindow(this) };
            dialog.ShowDialog();
        }
    }
}
