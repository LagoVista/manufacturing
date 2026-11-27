using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System.ComponentModel;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls.PickAndPlace
{
    /// <summary>
    /// Only presentation and lifecycle. All machine movement, evidence and
    /// persistence stay in StripFeederViewModel's typed procedure.
    /// </summary>
    public partial class FeederCalibrationProgressWindow : Window
    {
        private bool _started;

        public FeederCalibrationProgressWindow()
        {
            InitializeComponent();
        }

        private IStripFeederViewModel Procedure => DataContext as IStripFeederViewModel;

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (_started) return;
            _started = true;
            if (Procedure != null)
                await Procedure.StartFeederCalibrationFlowAsync();
        }

        private async void RunAgain_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure == null || Procedure.FeederFlowRunning) return;
            await Procedure.StartFeederCalibrationFlowAsync();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.FeederFlowRunning != true)
                Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // Closing a modal is never an emergency stop or cancellation of
            // already queued physical motion. Keep the view until stopped.
            if (Procedure?.FeederFlowRunning == true)
                e.Cancel = true;
        }
    }
}
