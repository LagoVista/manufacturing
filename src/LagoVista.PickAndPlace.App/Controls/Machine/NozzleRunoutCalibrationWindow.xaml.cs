using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System.ComponentModel;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>
    /// Live rendering only. The existing nozzle view model owns measurement,
    /// safe movement and the deliberate unsaved candidate result.
    /// </summary>
    public partial class NozzleRunoutCalibrationWindow : Window
    {
        public NozzleRunoutCalibrationWindow()
        {
            InitializeComponent();
        }

        private INozzleTipCalibrationViewModel Procedure =>
            DataContext as INozzleTipCalibrationViewModel;

        private async void RunAgain_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure == null || Procedure.RunoutRunning) return;
            await Procedure.StartAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.RunoutRunning != true)
                Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Procedure?.RunoutRunning == true)
                e.Cancel = true;
        }
    }
}
