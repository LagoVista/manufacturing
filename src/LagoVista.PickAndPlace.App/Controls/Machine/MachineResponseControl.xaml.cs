using LagoVista.PickAndPlace.App.ViewModels;
using LagoVista.PickAndPlace.ViewModels;
using System.Windows.Controls;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for MachineResponse.xaml
    /// </summary>
    public partial class MachineResponseControl : UserControl
    {
        MessageWindow _messagesWindow;
        public MachineResponseControl()
        {
            InitializeComponent();
        }

        private void ShowLogWindow_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (_messagesWindow == null)
            {
                var vm = DataContext as HomeViewModel;
                _messagesWindow = new MessageWindow(vm.MachineRepo.CurrentMachine);
                //_messagesWindow.Owner = Home.This;
                _messagesWindow.Closed += _messagesWindow_Closed;
                _messagesWindow.Show();
            }

        }

        private void _messagesWindow_Closed(object sender, System.EventArgs e)
        {
            _messagesWindow = null;
        }
    }
}
