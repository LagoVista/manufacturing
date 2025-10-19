// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b5a24dd82ebaec75978aaf01aa14e98151ea9e5c109e2b5badd8e9102b81aa22
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.Interfaces;
using System.Windows;

namespace LagoVista.PickAndPlace.App
{
    /// <summary>
    /// Interaction logic for MessageWindow.xaml
    /// </summary>
    public partial class MessageWindow : Window
    {
        readonly IMachine _machine;

        public MessageWindow(IMachine machine)
        {
            _machine = machine;
            InitializeComponent();
            DataContext = machine;
        }

        private void ClearMessages_Click(object sender, RoutedEventArgs e)
        {
            _machine.Messages.Clear();
        }

        private void ClearSentMessages_Click(object sender, RoutedEventArgs e)
        {
            _machine.SentMessages.Clear();
        }
    }
}
