using LagoVista.Core.IOC;
using LagoVista.Manufacturing.Models;
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
    /// One-plate independent calibration. Opening does not start motion.
    /// The existing parent plate selection is a convenience, never a mandate.
    /// </summary>
    public partial class StagingPlateCalibrationWindow : Window
    {
        public StagingPlateCalibrationWindow(MachineStagingPlate selected = null)
        {
            InitializeComponent();
            DataContext = new StagingPlateCalibrationViewModel(
                SLWIOC.Get<IMachineRepo>(), SLWIOC.Get<ILocatorViewModel>(), selected);
        }

        private StagingPlateCalibrationViewModel Procedure =>
            DataContext as StagingPlateCalibrationViewModel;

        private async void RunAgain_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanRun == true)
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
            if (Procedure?.CanClose != false)
                Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Procedure?.CanClose == false)
                e.Cancel = true;
        }
    }
}
