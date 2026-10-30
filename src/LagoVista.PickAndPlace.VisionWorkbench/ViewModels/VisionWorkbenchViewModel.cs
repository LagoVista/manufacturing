using LagoVista.PickAndPlace.Models;
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
            bool white = !Profile.ApplyThreshold || value >= threshold;
            if (Profile.Invert) white = !white;
            pixels[i] = (byte)(white ? 255 : 0);
            if (white) whiteCount++;
        }
        var output = BitmapSource.Create(width, height, gray.DpiX, gray.DpiY, PixelFormats.Gray8, null, pixels, width);
        output.Freeze();
        ProcessedImage = output;
        double count = pixels.Length;
        Status = $"{Path.GetFileName(_imagePath)} • {width}×{height} • Threshold {threshold} • White {100 * whiteCount / count:F1}% • Near-black input {100 * dark / count:F1}% • Near-white input {100 * bright / count:F1}%";
    }
}
