using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>Matches the nozzle-runout modal's procedure-first pattern.</summary>
    public partial class CameraToolPickupCalibrationWindow : Window
    {
        public CameraToolPickupCalibrationWindow()
        {
            InitializeComponent();
            // Approximately twice the original area, constrained to the display.
            Width = Math.Min(Width, SystemParameters.WorkArea.Width);
            Height = Math.Min(Height, SystemParameters.WorkArea.Height);
            MinWidth = Math.Min(MinWidth, Width);
            MinHeight = Math.Min(MinHeight, Height);
        }

        private ICameraToolPickupCalibrationViewModel Procedure =>
            DataContext as ICameraToolPickupCalibrationViewModel;

        private async void PreviewBottomCamera_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanOperate == true)
                await Procedure.PreviewBottomCameraAsync();
        }

        private async void RunAgain_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanOperate == true)
                await Procedure.StartAsync();
        }

        private async void RetryHistory_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanRetryHistory == true)
                await Procedure.RetryHistoryAsync();
        }

        private async void SaveAndVerify_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanSaveAndVerify != true)
                return;

            var confirmed = MessageBox.Show(this,
                "Approve saving the measured camera-to-tool offset to Manufacturing " +
                "and moving the REAL machine for a THIRD pickup using its normal " +
                "SAVED offset (NO temporary jog)? The disc must be returned, " +
                "the machine must be homed, idle and in Manual/camera mode. " +
                "Keep the emergency stop accessible. A rejected pickup will " +
                "attempt to restore the previous stored offset.",
                "Approve persisted offset and supervised third pickup",
                MessageBoxButton.YesNo, MessageBoxImage.Warning,
                MessageBoxResult.No);
            if (confirmed == MessageBoxResult.Yes)
                await Procedure.SaveAndVerifyAsync();
        }

        private async void ProposeCamera_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanOperate == true && ConfirmMotion("Verify both persisted pickup offsets, then measure Head 1's shared bottom-camera candidate. Nothing will be saved."))
                await Procedure.ProposeSharedCameraAsync();
        }

        private async void ApproveCamera_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanOperate == true && Procedure.SharedCandidateReady && ConfirmMotion(Procedure.SharedCameraStatus + "\n\nApprove saving this shared camera candidate and independently approaching it with both heads?"))
                await Procedure.ApproveSharedCameraAsync();
        }

        private async void ValidateCamera_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanOperate == true && ConfirmMotion("Approach the saved bottom-camera position with both heads without optical correction."))
                await Procedure.ValidateSharedCameraAsync();
        }

        private async void RunDiagnostics_Click(object sender, RoutedEventArgs e)
        {
            if (Procedure?.CanOperate == true && ConfirmMotion("Run supervised repeated saved-offset pickups and uncorrected bottom-camera approaches for both heads. No settings will be written."))
                await Procedure.RunFullDiagnosticsAsync();
        }

        private void StopDiagnostics_Click(object sender, RoutedEventArgs e) => Procedure?.RequestDiagnosticStop();

        private bool ConfirmMotion(string message) => MessageBox.Show(this,
            message + "\n\nConfirm fixture clearance, both nozzles installed, and emergency stop accessible.",
            "Supervised T001 calibration", MessageBoxButton.YesNo, MessageBoxImage.Warning,
            MessageBoxResult.No) == MessageBoxResult.Yes;

        private void CopyRunReport_Click(object sender, RoutedEventArgs e)
        {
            var report = Procedure?.DiagnosticReport;
            if (!String.IsNullOrWhiteSpace(report))
                Clipboard.SetText(report);
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
            if (Procedure?.CanOperate != false)
                Close();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (Procedure?.CanOperate == false)
                e.Cancel = true;
        }
    }
}
