using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>
    /// Visual-only independent runout dialog. Opening never commands machine
    /// motion; only the explicit Start Runout button invokes StartAsync.
    /// </summary>
    public partial class NozzleRunoutCalibrationWindow : Window
    {
        public NozzleRunoutCalibrationWindow() { InitializeComponent(); }

        private INozzleTipCalibrationViewModel Procedure =>
            DataContext as INozzleTipCalibrationViewModel;

        private static bool HasInvalidInput(DependencyObject root)
        {
            if (root == null) return false;
            if (Validation.GetHasError(root)) return true;
            for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
                if (HasInvalidInput(VisualTreeHelper.GetChild(root, i))) return true;
            return false;
        }

        private async void PreviewCamera_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanTuneRunoutVision == true)
                await Procedure.PreviewBottomCameraAsync();
        }

        private async void ApplyNozzlePreview_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanTuneRunoutVision != true) return;
            if (HasInvalidInput(NozzleVisionTabs)) return;
            await Procedure.ApplyRunoutVisionPreviewAsync();
        }

        private async void SaveNozzleProfile_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanTuneRunoutVision != true) return;
            if (HasInvalidInput(NozzleVisionTabs)) return;
            await Procedure.SaveRunoutVisionAsync();
        }

        private void RevertNozzleProfile_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanTuneRunoutVision == true)
                Procedure.RevertRunoutVision();
        }

        private async void RunAgain_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanStartRunout == true)
                await Procedure.StartAsync();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 1 && e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); }
                catch (InvalidOperationException) { }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.RunoutRunning != true &&
                Procedure?.RunoutPreviewBusy != true)
                Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Procedure?.RunoutRunning == true ||
                Procedure?.RunoutPreviewBusy == true)
            {
                e.Cancel = true;
                return;
            }
            Procedure?.RevertRunoutVision();
        }
    }
}
