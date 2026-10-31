using Microsoft.Win32;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LagoVista.PickAndPlace.VisionWorkbench.ViewModels;
using LagoVista.PickAndPlace.VisionWorkbench.Services;

namespace LagoVista.PickAndPlace.VisionWorkbench;

/// <summary>View-only shell: file pickers, scroll position, and drag/drop.</summary>
public partial class MainWindow : Window
{
    private readonly VisionWorkbenchViewModel _vm = new();
    private readonly WorkbenchCameraCapture _cameraCapture = new();
    private readonly WorkbenchMachineControl _machineControl = new();
    private static readonly string WindowPreferencesPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "LagoVista", "VisionWorkbench", "window.json");

    private sealed record WindowPreferences(double Left, double Top, double Width, double Height, bool Maximized);

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        RestoreWindowPreferences();
        _machineControl.StateChanged += (_, _) => Dispatcher.BeginInvoke(UpdateMachineState);
        Closing += (_, _) => { _cameraCapture.Dispose(); _machineControl.Dispose(); SaveWindowPreferences(); };
        Loaded += (_, _) =>
        {
            TryAction(_vm.RestoreLastImage, "Unable to restore last image");
            RefreshCameras();
            RefreshPorts();
            UpdateMachineState();
        };
    }

    private void RestoreWindowPreferences()
    {
        try
        {
            if (!File.Exists(WindowPreferencesPath)) return;
            var saved = JsonSerializer.Deserialize<WindowPreferences>(File.ReadAllText(WindowPreferencesPath));
            if (saved is null || !double.IsFinite(saved.Width) || !double.IsFinite(saved.Height) ||
                !double.IsFinite(saved.Left) || !double.IsFinite(saved.Top)) return;

            var width = Math.Max(MinWidth, saved.Width);
            var height = Math.Max(MinHeight, saved.Height);
            // Avoid reopening an inaccessible window after monitor changes or DPI changes.
            var desktop = SystemParameters.WorkArea;
            if (desktop.Width <= 0 || desktop.Height <= 0) return;
            Width = Math.Min(width, desktop.Width);
            Height = Math.Min(height, desktop.Height);
            Left = Math.Clamp(saved.Left, desktop.Left, desktop.Right - Width);
            Top = Math.Clamp(saved.Top, desktop.Top, desktop.Bottom - Height);
            if (saved.Maximized) WindowState = WindowState.Maximized;
        }
        catch (Exception) { /* Invalid local preferences must not prevent launching. */ }
    }

    private void SaveWindowPreferences()
    {
        try
        {
            var bounds = RestoreBounds;
            var snapshot = new WindowPreferences(bounds.Left, bounds.Top, bounds.Width, bounds.Height,
                WindowState == WindowState.Maximized);
            Directory.CreateDirectory(Path.GetDirectoryName(WindowPreferencesPath)!);
            var temp = WindowPreferencesPath + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(snapshot));
            File.Move(temp, WindowPreferencesPath, true);
        }
        catch (Exception) { /* Window state persistence is best-effort. */ }
    }

    private void RecentImage_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is RecentImageEntry item)
        {
            TryAction(() => _vm.LoadImage(item.Path), "Unable to open recent image");
            if (sender is ComboBox combo) combo.SelectedIndex = -1;
        }
    }

    private async void RunImageTests_Click(object sender, RoutedEventArgs e)
    {
        var runButton = sender as Button;
        if (runButton != null) runButton.IsEnabled = false;
        try { await _vm.RunImageTestsAsync(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Image tests", MessageBoxButton.OK, MessageBoxImage.Error); }
        finally { if (runButton != null) runButton.IsEnabled = true; }
    }

    private void CopyAllResults_Click(object sender, RoutedEventArgs e) => Clipboard.SetText(_vm.BuildTestReport());

    private void ClearMarks_Click(object sender, RoutedEventArgs e) => _vm.ClearAnnotations();

    private void OriginalPreview_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_vm.OriginalImage is null) return;
        var image = OriginalPreview;
        var source = _vm.OriginalImage;
        var scale = Math.Min(image.ActualWidth / source.PixelWidth, image.ActualHeight / source.PixelHeight);
        if (scale <= 0) return;
        var left = (image.ActualWidth - source.PixelWidth * scale) / 2;
        var top = (image.ActualHeight - source.PixelHeight * scale) / 2;
        var point = e.GetPosition(image);
        var x = (point.X - left) / scale;
        var y = (point.Y - top) / scale;
        if (x >= 0 && x < source.PixelWidth && y >= 0 && y < source.PixelHeight)
            _vm.AnnotateSelectedHole(x, y);
    }

    private void ImageTest_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.AddedItems.Count > 0 && e.AddedItems[0] is ImageTestCase test)
            TryAction(() => _vm.ShowImageTest(test), "Unable to open test image");
    }

    private void OpenImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select camera image",
            Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) == true) TryAction(() => _vm.LoadImage(dialog.FileName), "Unable to open image");
    }

    private void SaveProcessed_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.ProcessedImage is null) return;
        var dialog = new SaveFileDialog { Title = "Save processed image", Filter = "PNG (*.png)|*.png", FileName = "processed.png" };
        if (dialog.ShowDialog(this) == true) TryAction(() => _vm.SaveProcessedImage(dialog.FileName), "Unable to save image");
    }

    private void NewProfile_Click(object sender, RoutedEventArgs e) => _vm.CreateProfile();

    private void OpenProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Open VisionProfile JSON", Filter = "JSON profiles (*.json)|*.json" };
        if (dialog.ShowDialog(this) == true) TryAction(() => _vm.LoadProfile(dialog.FileName), "Unable to load profile");
    }

    private void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new SaveFileDialog
        {
            Title = "Save VisionProfile JSON",
            Filter = "JSON profiles (*.json)|*.json",
            FileName = _vm.ProfilePath is null ? "VisionProfile.json" : Path.GetFileName(_vm.ProfilePath),
            InitialDirectory = _vm.ProfilePath is null ? null : Path.GetDirectoryName(_vm.ProfilePath)
        };
        if (dialog.ShowDialog(this) == true) TryAction(() => _vm.SaveProfile(dialog.FileName), "Unable to save profile");
    }


    private void RefreshPorts_Click(object sender, RoutedEventArgs e) => RefreshPorts();

    private void RefreshPorts()
    {
        TryAction(() => _vm.SetSerialPorts(_machineControl.EnumeratePorts()), "Unable to enumerate serial ports");
    }

    private async void ConnectMachine_Click(object sender, RoutedEventArgs e)
    {
        if (String.IsNullOrWhiteSpace(_vm.SelectedSerialPort))
        {
            MessageBox.Show(this, "Select a serial port first.", "Machine", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        try
        {
            await _machineControl.ConnectAsync(_vm.SelectedSerialPort, _vm.SerialBaudRate, _vm.SelectedFirmwareType);
            UpdateMachineState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Unable to connect machine", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private async void DisconnectMachine_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            await _machineControl.DisconnectAsync();
            UpdateMachineState();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Unable to disconnect machine", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void UpdateMachineState()
    {
        var machine = _machineControl.Machine;
        _vm.UpdateMachineState(machine.Connected, machine.Status, machine.MachinePosition.X, machine.MachinePosition.Y);
    }

    private void JogXMinus_Click(object sender, RoutedEventArgs e) => Jog(-_vm.JogStepMm, 0);
    private void JogXPlus_Click(object sender, RoutedEventArgs e) => Jog(_vm.JogStepMm, 0);
    private void JogYMinus_Click(object sender, RoutedEventArgs e) => Jog(0, -_vm.JogStepMm);
    private void JogYPlus_Click(object sender, RoutedEventArgs e) => Jog(0, _vm.JogStepMm);

    private void Jog(double x, double y)
    {
        TryAction(() => _machineControl.Jog(x, y), "Unable to jog machine");
    }

    private void EmergencyStop_Click(object sender, RoutedEventArgs e)
    {
        _machineControl.EmergencyStop();
        UpdateMachineState();
    }

    private void RefreshCameras_Click(object sender, RoutedEventArgs e) => RefreshCameras();

    private void RefreshCameras()
    {
        TryAction(() => _vm.SetCameraDevices(_cameraCapture.Enumerate()), "Unable to enumerate cameras");
    }

    private void StartCamera_Click(object sender, RoutedEventArgs e)
    {
        if (_vm.SelectedCamera is null)
        {
            MessageBox.Show(this, "Select a camera first.", "Camera", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        TryAction(() => _cameraCapture.Start(_vm.SelectedCamera,
            frame => Dispatcher.BeginInvoke(() => _vm.ApplyCameraFrame(frame)),
            error => Dispatcher.BeginInvoke(() => _vm.SetCameraError(error))), "Unable to start camera");
    }

    private void StopCamera_Click(object sender, RoutedEventArgs e)
    {
        _cameraCapture.Stop();
        _vm.SetCameraStopped();
    }

    private void FitView_Click(object sender, RoutedEventArgs e)
    {
        OriginalImageHost.InvalidateMeasure();
        ProcessedImageHost.InvalidateMeasure();
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            TryAction(() => _vm.LoadImage(files[0]), "Unable to open image");
    }

    private void TryAction(Action action, string title)
    {
        try { action(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, title, MessageBoxButton.OK, MessageBoxImage.Error); }
    }
}
