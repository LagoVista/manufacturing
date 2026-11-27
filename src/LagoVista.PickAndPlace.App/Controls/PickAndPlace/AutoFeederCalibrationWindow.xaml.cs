using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.ComponentModel;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls.PickAndPlace
{
    public partial class AutoFeederCalibrationWindow : Window
    {
        private readonly IAutoFeederViewModel _vm;

        public AutoFeederCalibrationWindow(IAutoFeederViewModel vm)
        {
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
            InitializeComponent();
            _vm.ResetPlacementDialog();
            DataContext = _vm;
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_vm.PlacementBusy)
            {
                e.Cancel = true;
                return;
            }
            if (_vm.HasPlacementCandidate &&
                MessageBox.Show(this, "Discard the unsaved feeder placement measurement?", "Unsaved calibration",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            {
                e.Cancel = true;
                return;
            }
            _vm.ResetPlacementDialog();
        }
    }
}
