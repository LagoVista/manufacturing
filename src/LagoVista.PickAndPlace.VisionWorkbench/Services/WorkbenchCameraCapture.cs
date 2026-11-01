using DirectShowLib;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System.IO;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.VisionWorkbench.Services;

public sealed record CameraDeviceInfo(string Name, string DevicePath, int Index)
{
    public override string ToString() => Name;
}

/// <summary>Small workbench-only wrapper over the same DirectShow/Emgu capture path used by the PnP app.</summary>
public sealed class WorkbenchCameraCapture : IDisposable
{
    private VideoCapture? _capture;
    private CancellationTokenSource? _cts;
    private Task? _loop;

    public IReadOnlyList<CameraDeviceInfo> Enumerate()
        => DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice)
            .Select((device, index) => new CameraDeviceInfo(device.Name, device.DevicePath, index))
            .ToArray();

    public void Start(CameraDeviceInfo device, VisionProfile? profile, Action<BitmapSource> onFrame, Action<string>? onError = null)
    {
        Stop();
        _capture = new VideoCapture(device.Index);
        if (!_capture.IsOpened)
        {
            _capture.Dispose();
            _capture = null;
            throw new InvalidOperationException($"Could not open camera '{device.Name}'.");
        }

        _capture.Set(CapProp.FrameWidth, 1920);
        _capture.Set(CapProp.FrameHeight, 1920);
        _capture.Set(CapProp.AutoExposure, 0);
        if (profile is not null)
        {
            _capture.Set(CapProp.Brightness, profile.Brightness);
            _capture.Set(CapProp.Focus, profile.Focus);
            _capture.Set(CapProp.Contrast, profile.Contrast);
            _capture.Set(CapProp.Exposure, profile.Exposure);
        }

        _cts = new CancellationTokenSource();
        var token = _cts.Token;
        _loop = Task.Run(async () =>
        {
            try
            {
                while (!token.IsCancellationRequested)
                {
                    using var frame = _capture?.QueryFrame();
                    if (frame is not null && !frame.IsEmpty)
                    {
                        using var buffer = new VectorOfByte();
                        CvInvoke.Imencode(".png", frame, buffer);
                        using var stream = new MemoryStream(buffer.ToArray());
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                        image.Freeze();
                        onFrame(image);
                    }
                    await Task.Delay(120, token);
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { onError?.Invoke(ex.Message); }
        }, token);
    }

    public void Stop()
    {
        _cts?.Cancel();
        try { _loop?.Wait(500); } catch { }
        _loop = null;
        _cts?.Dispose();
        _cts = null;
        _capture?.Stop();
        _capture?.Dispose();
        _capture = null;
    }

    public void Dispose() => Stop();
}
