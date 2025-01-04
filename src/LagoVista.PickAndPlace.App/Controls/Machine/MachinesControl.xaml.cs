using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Repos;
using LagoVista.XPlat;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for MachinesControl.xaml
    /// </summary>
    public partial class MachinesControl : VMBoundUserControl<IMachineRepo>
    {
        public MachinesControl()
        {
            InitializeComponent();
        }

        private void Settings_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // Clone in case we cancel.
            var clonedSettings = ViewModel.CurrentMachine.Settings.Clone();
            var dlg = new SettingsWindow(ViewModel, clonedSettings, false);
            dlg.Owner = System.Windows.Application.Current.MainWindow;
            dlg.ShowDialog();
            if (dlg.DialogResult.HasValue && dlg.DialogResult.Value)
            {
                ViewModel.CurrentMachine.Settings = clonedSettings;
            }
        }
    }
}
