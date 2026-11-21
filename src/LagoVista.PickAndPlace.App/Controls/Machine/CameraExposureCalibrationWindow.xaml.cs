using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System.ComponentModel;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>
    /// A self-contained exposure UI. The existing machine calibration view model
    /// owns motion, camera verification, exposure measurements and durable save.
    /// Closing this window never cancels a queued physical machine command.
    /// </summary>
    public partial class CameraExposureCalibrationWindow : Window
    {
        public CameraExposureCalibrationWindow(IMachineCalibrationViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            if (viewModel?.SelectedMachineCamera?.CameraType?.Value != CameraTypes.Position)
            {
                var active = viewModel?.Machine?.PositionImageCaptureService?.Camera;
                if (active?.CameraType?.Value == CameraTypes.Position)
                    viewModel.SelectedMachineCamera = active;
            }
        }

        private IMachineCalibrationViewModel Procedure => DataContext as IMachineCalibrationViewModel;

        private async void RunAgain_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure != null && !Procedure.ExposureCalibrationRunning)
                await Procedure.RunExposureCalibrationAsync();
        }

        // Borderless dialog: the in-app header is the drag surface.
        // Keep the explicit Close action and existing active-motion guard.
        private void Header_MouseLeftButtonDown(object sender,
            System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 &&
                e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                try { DragMove(); }
                catch (InvalidOperationException) { /* Pointer released before drag. */ }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.ExposureCalibrationRunning != true) Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Procedure?.ExposureCalibrationRunning == true)
                e.Cancel = true;
        }
    }
}
