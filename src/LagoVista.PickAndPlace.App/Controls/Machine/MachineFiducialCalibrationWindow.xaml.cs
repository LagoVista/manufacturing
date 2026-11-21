using LagoVista.Core.IOC;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.ViewModels.Machine;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>
    /// T009 independent modal. Opening never moves hardware; only Run does.
    /// Closing during an active physical calibration is blocked.
    /// </summary>
    public partial class MachineFiducialCalibrationWindow : Window
    {
        public MachineFiducialCalibrationWindow()
        {
            InitializeComponent();
            DataContext = new MachineFiducialCalibrationViewModel(
                SLWIOC.Get<IMachineRepo>(), SLWIOC.Get<ILocatorViewModel>());
        }

        private MachineFiducialCalibrationViewModel Procedure =>
            DataContext as MachineFiducialCalibrationViewModel;

        private async void RunAgain_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure != null && !Procedure.Running)
                await Procedure.RunAsync();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); }
                catch (InvalidOperationException) { /* Pointer released before drag. */ }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.Running != true) Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Procedure?.Running == true)
                e.Cancel = true;
        }
    }
}
