using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System.ComponentModel;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>Matches the nozzle-runout modal's procedure-first pattern.</summary>
    public partial class CameraToolPickupCalibrationWindow : Window
    {
        public CameraToolPickupCalibrationWindow()
        {
            InitializeComponent();
        }

        private ICameraToolPickupCalibrationViewModel Procedure =>
            DataContext as ICameraToolPickupCalibrationViewModel;

        private async void RunAgain_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure == null || Procedure.CalibrationRunning) return;
            await Procedure.StartAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CalibrationRunning != true)
                Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Procedure?.CalibrationRunning == true)
                e.Cancel = true;
        }
    }
}
