using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.VisionWorkbench.Services;
using System.Windows;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.VisionWorkbench.ViewModels;

/// <summary>Workbench state and offline image processing. Does not access machine hardware.</summary>
public sealed class VisionWorkbenchViewModel : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    private void Notify([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private VisionProfile _profile = NewProfile();
    private BitmapSource? _originalImage;
    private BitmapSource? _processedImage;
    private BitmapSource? _detectionOverlay;
    private readonly VisionDetectionService _detector = new();
    private string? _imagePath;
    private string? _profilePath;
    private string _status = "Ready — open an image or drag one into this window.";
    private string _profileStatus = "New local profile";
    private bool _changed;

    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };
    private static VisionProfile NewProfile() => new() { Name = "New experiment", Key = VisionProfile.VisionProfile_Custom, Id = Guid.NewGuid().ToString("N") };

    public VisionProfile Profile
    {
        get => _profile;
        private set
        {
            _profile.PropertyChanged -= ProfileChanged;
            _profile = value;
            _profile.PropertyChanged += ProfileChanged;
            Notify();
            Notify(nameof(ThresholdValue));
            Notify(nameof(ThresholdText));
            Notify(nameof(KernelText));
            Notify(nameof(CannyText));
            ProcessImage();
        }
    }

    public VisionWorkbenchViewModel() => _profile.PropertyChanged += ProfileChanged;

    public BitmapSource? OriginalImage { get => _originalImage; private set { _originalImage = value; Notify(); } }
    public BitmapSource? ProcessedImage { get => _processedImage; private set { _processedImage = value; Notify(); } }
    public BitmapSource? DetectionOverlay { get => _detectionOverlay; private set { _detectionOverlay = value; Notify(); } }
    public ObservableCollection<string> CircleMeasurements { get; } = new();
    private string _circleSummary = "No circle measurements yet";
    public string CircleSummary { get => _circleSummary; private set { _circleSummary = value; Notify(); } }
    public string Status { get => _status; private set { _status = value; Notify(); } }
    public string ProfileStatus { get => _profileStatus; private set { _profileStatus = value; Notify(); } }
    public string ThresholdText => $"{Profile.PrimaryThreshold} / 100";
    public string KernelText => Profile.GausianKernellSize.ToString();
    public string CannyText => Profile.CannyLowThreshold.ToString("F0");
    public double ThresholdValue
    {
        get => Profile.PrimaryThreshold / 100.0 * 255.0;
        set => Profile.PrimaryThreshold = (byte)Math.Clamp((int)Math.Round(value / 255.0 * 100.0), 0, 100);
    }

    private void ProfileChanged(object? sender, PropertyChangedEventArgs e)
    {
        _changed = true;
        ProfileStatus = $"{(_profilePath is null ? "New local profile" : Path.GetFileName(_profilePath))} • Edited (not saved)";
        Notify(nameof(ThresholdValue));
        Notify(nameof(ThresholdText));
        Notify(nameof(KernelText));
        Notify(nameof(CannyText));
        ProcessImage();
    }

    public void CreateProfile()
    {
        _profilePath = null;
        Profile = NewProfile();
        _changed = false;
        ProfileStatus = "New local profile";
    }

    public void LoadProfile(string path)
    {
        var value = JsonSerializer.Deserialize<VisionProfile>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException("File did not contain a VisionProfile.");
        _profilePath = path;
        Profile = value;
        _changed = false;
        ProfileStatus = Path.GetFileName(path);
    }

    public string? ProfilePath => _profilePath;
    public bool HasUnsavedChanges => _changed;

    public void SaveProfile(string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(Profile, JsonOptions));
        _profilePath = path;
        _changed = false;
        ProfileStatus = "Saved: " + path;
    }

    public void LoadImage(string path)
    {
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.UriSource = new Uri(Path.GetFullPath(path));
        image.EndInit();
        image.Freeze();
        _imagePath = path;
        OriginalImage = image;
        ProcessImage();
    }

    public void SaveProcessedImage(string path)
    {
        if (ProcessedImage is null) return;
        using var stream = File.Create(path);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(ProcessedImage));
        encoder.Save(stream);
    }

    private static BitmapSource DrawOverlay(int width, int height, VisionDetectionResult detection)
    {
        var visual = new DrawingVisual();
        using (var dc = visual.RenderOpen())
        {
            var circlePen = new Pen(Brushes.LimeGreen, 2);
            var rectanglePen = new Pen(Brushes.Orange, 2);
            foreach (var c in detection.Circles)
            {
                dc.DrawEllipse(null, circlePen, new Point(c.X, c.Y), c.Radius, c.Radius);
                dc.DrawLine(circlePen, new Point(c.X - 5, c.Y), new Point(c.X + 5, c.Y));
                dc.DrawLine(circlePen, new Point(c.X, c.Y - 5), new Point(c.X, c.Y + 5));
            }
            foreach (var r in detection.Rectangles)
            {
                dc.PushTransform(new RotateTransform(r.Angle, r.X, r.Y));
                dc.DrawRectangle(null, rectanglePen, new Rect(r.X - r.Width / 2, r.Y - r.Height / 2, r.Width, r.Height));
                dc.Pop();
            }
        }
        var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        bitmap.Render(visual);
        bitmap.Freeze();
        return bitmap;
    }

    private void ProcessImage()
    {
        if (OriginalImage is null) return;
        var gray = new FormatConvertedBitmap(OriginalImage, PixelFormats.Gray8, null, 0);
        var width = gray.PixelWidth;
        var height = gray.PixelHeight;
        var pixels = new byte[width * height];
        gray.CopyPixels(pixels, width, 0);
        int dark = 0, bright = 0, whiteCount = 0;
        int threshold = (int)ThresholdValue;
        for (int i = 0; i < pixels.Length; i++)
        {
            var value = pixels[i];
            if (value <= 2) dark++;
            if (value >= 253) bright++;
            byte outputPixel;
            if (Profile.ApplyThreshold)
            {
                bool white = value >= threshold;
                if (Profile.Invert) white = !white;
                outputPixel = (byte)(white ? 255 : 0);
            }
            else
            {
                outputPixel = Profile.Invert ? (byte)(255 - value) : value;
            }
            pixels[i] = outputPixel;
            if (outputPixel == 255) whiteCount++;
        }
        var output = BitmapSource.Create(width, height, gray.DpiX, gray.DpiY, PixelFormats.Gray8, null, pixels, width);
        output.Freeze();
        ProcessedImage = output;
        double count = pixels.Length;
        var measurementStatus = "No detectors selected";
        try
        {
            var detection = _detector.Detect(OriginalImage, Profile);
            DetectionOverlay = DrawOverlay(width, height, detection);
            CircleMeasurements.Clear();
            for (int i = 0; i < detection.Circles.Count; i++)
            {
                var c = detection.Circles[i];
                CircleMeasurements.Add($"#{i + 1}: X={c.X:F1}  Y={c.Y:F1}  R={c.Radius:F1}");
            }
            CircleSummary = $"{detection.Circles.Count} circle(s) found";
            measurementStatus = $"{detection.Circles.Count} circles • {detection.Rectangles.Count} rectangles";
        }
        catch (Exception ex)
        {
            DetectionOverlay = null;
            CircleMeasurements.Clear();
            CircleSummary = "Detection failed";
            measurementStatus = $"Detection unavailable: {ex.GetType().Name}: {ex.Message}";
        }
        Status = $"{Path.GetFileName(_imagePath)} • {width}×{height} • Threshold {threshold} • White {100 * whiteCount / count:F1}% • Near-black input {100 * dark / count:F1}% • Near-white input {100 * bright / count:F1}% • {measurementStatus}";
    }
}
