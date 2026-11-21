using LagoVista.Core.IOC;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.ViewModels.Machine;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using System.Windows.Media;

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

        private static bool HasInvalidInput(DependencyObject root)
        {
            if (root == null) return false;
            if (Validation.GetHasError(root)) return true;
            for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
                if (HasInvalidInput(VisualTreeHelper.GetChild(root, index)))
                    return true;
            return false;
        }

        private async void ApplyVisionPreview_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanEditVision != true) return;
            if (HasInvalidInput(VisionSettingTabs))
            {
                Procedure.ReportVisionInputError();
                return;
            }
            await Procedure.ApplyVisionPreviewAsync();
        }

        private async void ObserveVisionPreview_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanEditVision == true)
                await Procedure.ObserveVisionPreviewAsync();
        }

        private async void SaveVisionProfile_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanEditVision == true)
                await Procedure.SaveVisionProfileAsync();
        }

        private void RevertVisionProfile_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanEditVision == true)
                Procedure.RevertVisionPreview();
        }

        private void SearchLeft_Click(object sender, RoutedEventArgs e) =>
            Procedure?.NudgeSearchRegion(-0.1, 0);

        private void SearchRight_Click(object sender, RoutedEventArgs e) =>
            Procedure?.NudgeSearchRegion(0.1, 0);

        private void SearchUp_Click(object sender, RoutedEventArgs e) =>
            Procedure?.NudgeSearchRegion(0, -0.1);

        private void SearchDown_Click(object sender, RoutedEventArgs e) =>
            Procedure?.NudgeSearchRegion(0, 0.1);

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
            {
                e.Cancel = true;
                return;
            }

            // Closing never persists changes. Any applied-but-unsaved preview
            // is rolled back to the prior stagingplatehole profile values.
            Procedure?.RevertVisionPreview();
        }
    }
}
