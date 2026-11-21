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
