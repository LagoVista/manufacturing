using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace LagoVista.PickAndPlace.App.Controls.PickAndPlace
{
    public partial class PhotonFeederControlWindow : Window
    {
        private readonly IPhotonFeederViewModel _vm;
        private readonly Queue<string> _lines = new Queue<string>();
        private const int MaxLines = 500;
        private bool _closed;

        public PhotonFeederControlWindow(IPhotonFeederViewModel vm)
        {
            _vm = vm ?? throw new ArgumentNullException(nameof(vm));
            InitializeComponent();
            _vm.ManualTestArmed = false;
            DataContext = _vm;
            _vm.OperationMessage += OnOperationMessage;
            Append("Console opened. Select Discover to scan the bus.");
            if (!string.IsNullOrWhiteSpace(_vm.Status)) Append("Last status: " + _vm.Status);
        }

        private void OnOperationMessage(object sender, string message)
        {
            if (Dispatcher.CheckAccess()) Append(message);
            else Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => Append(message)));
        }

        private void Append(string message)
        {
            if (_closed) return;
            _lines.Enqueue($"{DateTime.Now:HH:mm:ss.fff}  {message}");
            while (_lines.Count > MaxLines) _lines.Dequeue();
            ConsoleText.Text = string.Join(Environment.NewLine, _lines);
            ConsoleText.ScrollToEnd();
        }

        private void DistancePreset_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string distance)
                _vm.ManualDistanceText = distance;
        }

        private void Copy_Click(object sender, RoutedEventArgs e)
        {
            try { if (ConsoleText.Text.Length > 0) Clipboard.SetText(ConsoleText.Text); }
            catch (Exception ex) { Append("Clipboard unavailable: " + ex.Message); }
        }

        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            _lines.Clear();
            ConsoleText.Clear();
        }

        private void Header_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                try { DragMove(); }
                catch (InvalidOperationException) { }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_vm.IsBusBusy)
            {
                e.Cancel = true;
                Append("Cannot close while the feeder bus is active.");
                return;
            }
            _closed = true;
            _vm.OperationMessage -= OnOperationMessage;
            _vm.ManualTestArmed = false;
        }
    }
}
