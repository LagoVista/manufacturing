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
