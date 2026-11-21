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

        private async void ApplyTuning_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanManualAdjust == true)
                await Procedure.ApplyTuningPreviewAsync();
        }

        private async void ObserveTuning_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanManualAdjust == true)
                await Procedure.ObserveTuningAsync();
        }

        private async void SaveTuning_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanManualAdjust == true)
                await Procedure.SaveTuningAsync();
        }

        private void RevertTuning_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanManualAdjust == true)
                Procedure.RevertTuningPreview();
        }

        private double JogIncrement => JogStepSelect.SelectedIndex == 1 ? 0.5 : 0.1;

        private async void JogLeft_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanManualAdjust == true)
                await Procedure.JogAsync(-JogIncrement, 0);
        }

        private async void JogRight_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanManualAdjust == true)
                await Procedure.JogAsync(JogIncrement, 0);
        }

        private async void JogUp_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanManualAdjust == true)
                await Procedure.JogAsync(0, JogIncrement);
        }

        private async void JogDown_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanManualAdjust == true)
                await Procedure.JogAsync(0, -JogIncrement);
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanClose != false) Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Procedure?.CanClose == false)
            {
                e.Cancel = true;
                return;
            }

            // Modal dismiss never persists a preview. An explicit successful
            // Save Profile is the only authorization to retain Hough changes.
            Procedure?.RevertTuningPreview();
        }
    }
}
