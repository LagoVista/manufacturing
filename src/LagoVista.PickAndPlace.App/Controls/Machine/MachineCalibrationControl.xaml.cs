// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fda4631678668997e135f92a6fa9732e1bf84799bbc98e9dbbc66cd9e33b3c50
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.Core.IOC;
using LagoVista.XPlat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LagoVista.PickAndPlace.App.Controls.Machine
{
    /// <summary>
    /// Interaction logic for MachineCalibrationControl.xaml
    /// </summary>
    public partial class MachineCalibrationControl : VMBoundUserControl<IMachineCalibrationViewModel>
    {
        public MachineCalibrationControl()
        {
            InitializeComponent();
        }

        private void OpenExposureCalibration_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new CameraExposureCalibrationWindow(ViewModel)
            {
                Owner = Window.GetWindow(this)
            };
            dialog.ShowDialog();
        }

        private void OpenFiducialCalibration_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new MachineFiducialCalibrationWindow
            {
                Owner = Window.GetWindow(this)
            };
            dialog.ShowDialog();
        }

        private void OpenStagingPlateSettings_Click(object sender, RoutedEventArgs e)
        {
            // Configuration is available now; do not accidentally run the
            // legacy staging runner, which chains to machine-origin calibration.
            CalibrationTabs.SelectedItem = StagingPlatesTab;
        }

        private void OpenCameraSettings_Click(object sender, RoutedEventArgs e)
        {
            CalibrationTabs.SelectedItem = CamerasTab;
        }

        private VacuumCalibrationWindow _vacuumWindow;

        private void OpenVacuumCalibration_Click(object sender, RoutedEventArgs e)
        {
            if (_vacuumWindow != null)
            {
                _vacuumWindow.Activate();
                return;
            }
            var viewModel = new LagoVista.PickAndPlace.ViewModels.Machine.VacuumCalibrationViewModel(
                SLWIOC.Get<IMachineRepo>());
            _vacuumWindow = new VacuumCalibrationWindow(viewModel)
            {
                Owner = Window.GetWindow(this)
            };
            _vacuumWindow.Closed += (s, args) => _vacuumWindow = null;
            // Nonmodal is intentional: nozzle Z contact must be manually
            // jogged via the existing machine control panel.
            _vacuumWindow.Show();
        }

        private void OpenNozzleRunout_Click(object sender, RoutedEventArgs e)
        {
            var viewModel = SLWIOC.Get<INozzleTipCalibrationViewModel>();
            var dialog = new NozzleRunoutCalibrationWindow
            {
                DataContext = viewModel,
                Owner = Window.GetWindow(this)
            };
            dialog.ShowDialog();
        }
    }
}
