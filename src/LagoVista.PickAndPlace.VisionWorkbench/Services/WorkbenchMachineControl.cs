using LagoVista.Manufacturing.Models;
using System.ComponentModel;
using System.IO.Ports;

namespace LagoVista.PickAndPlace.VisionWorkbench.Services;

public sealed class WorkbenchMachineControl : IDisposable
{
    private bool _settingsLoadedFromPlatform;
    private const int InitialFastFeedRate = 2500;
    public LagoVista.PickAndPlace.Machine Machine { get; } = new();

    public event EventHandler? StateChanged;

    public WorkbenchMachineControl()
    {
        Machine.PropertyChanged += Machine_PropertyChanged;
    }

    private void Machine_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Machine.MachinePosition) ||
            e.PropertyName == nameof(Machine.Connected) ||
            e.PropertyName == nameof(Machine.Mode) ||
            e.PropertyName == nameof(Machine.Status))
            StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void LoadSettings(LagoVista.Manufacturing.Models.Machine settings)
    {
        if (Machine.Connected)
            throw new InvalidOperationException("Disconnect before loading different machine settings.");

        Machine.Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        // Workbench-only conservative motion limit; do not persist to platform.
        Machine.Settings.FastFeedRate = Math.Min(
            Machine.Settings.FastFeedRate > 0 ? Machine.Settings.FastFeedRate : InitialFastFeedRate,
            InitialFastFeedRate);
        _settingsLoadedFromPlatform = true;
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public IReadOnlyList<string> EnumeratePorts()
        => SerialPort.GetPortNames().OrderBy(name => name, StringComparer.OrdinalIgnoreCase).ToArray();

    public async Task ConnectAsync(string portName, int baudRate, FirmwareTypes firmwareType)
    {
        if (Machine.Connected) return;

        if (!_settingsLoadedFromPlatform)
            throw new InvalidOperationException("Sign in and load the machine configuration from the platform before connecting.");

        await Machine.InitAsync();
        await Machine.ConnectAsync(new SerialPort(portName, baudRate));
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public async Task DisconnectAsync()
    {
        if (Machine.Connected)
            await Machine.DisconnectAsync();
        StateChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Jog(double xMm, double yMm)
    {
        if (!Machine.Connected || Machine.Mode != OperatingMode.Manual)
            throw new InvalidOperationException("Machine must be connected and in Manual mode before jogging.");

        Machine.GotoPoint(xMm, yMm, rapidMove: true, relativeMove: true);
    }

    public void EmergencyStop() => Machine.EmergencyStop();

    public void Dispose()
    {
        Machine.PropertyChanged -= Machine_PropertyChanged;
        if (Machine.Connected)
            Machine.DisconnectAsync().GetAwaiter().GetResult();
    }
}
