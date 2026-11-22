using LagoVista.PickAndPlace.ViewModels.Machine;
using System.ComponentModel;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>
    /// The guided runner owns camera travel, tool selection, and a
    /// separately operator-triggered move to configured pick height.
    /// Hardware actions and validation remain in the testable view model.
    /// </summary>
    public partial class VacuumCalibrationWindow : Window
    {
        public VacuumCalibrationWindow(VacuumCalibrationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }

        private VacuumCalibrationViewModel Procedure => DataContext as VacuumCalibrationViewModel;

        private void Header_MouseLeftButtonDown(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 &&
                e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                try { DragMove(); }
                catch (System.InvalidOperationException) { }
            }
        }

        private async void Continue_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure != null) await Procedure.ContinueAsync();
        }

        private async void RunAll_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure != null) await Procedure.RunAllAsync();
        }

        private async void ApplyCalibration_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure != null) await Procedure.ApplyCalibrationAsync();
        }

        private async void Abort_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure != null) await Procedure.AbortAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanClose == true) Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Closing the UI does not cancel queued machine motion, therefore
            // require a settled, pump-off state before abandoning the wizard.
            if (Procedure?.CanClose != true)
                e.Cancel = true;
        }
    }
}
