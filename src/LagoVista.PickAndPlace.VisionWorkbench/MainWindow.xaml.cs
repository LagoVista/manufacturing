using Microsoft.Win32;
using LagoVista.PickAndPlace.Models;
using System.Text.Json;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.VisionWorkbench;

public partial class MainWindow : Window
{
    private VisionProfile _profile = new() { Name = "New experiment", Key = VisionProfile.VisionProfile_Custom, Id = Guid.NewGuid().ToString("N") };
    private string? _profilePath;
    private bool _loadingProfile;
    private BitmapSource? _source;
    private BitmapSource? _processed;
    private string? _filename;
    private bool _invertThreshold;

    public bool InvertThreshold
    {
        get => _invertThreshold;
        set
        {
            _invertThreshold = value;
            ProcessImage();
        }
    }

    public MainWindow()
    {
        InitializeComponent();
        ShowProfile();
    }


    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true, PropertyNameCaseInsensitive = true };

    private void ShowProfile()
    {
        _loadingProfile = true;
        try
        {
            ProfileName.Text = _profile.Name ?? "";
            ProfileKey.Text = _profile.Key ?? "";
            ApplyThresholdCheck.IsChecked = _profile.ApplyThreshold;
            InvertCheck.IsChecked = _profile.Invert;
            ApplyMaskCheck.IsChecked = _profile.ApplyMask;
            BlurCheck.IsChecked = _profile.UseBlurredImage;
            PrimaryThresholdSlider.Value = Math.Min((byte)100, _profile.PrimaryThreshold);
            KernelSlider.Value = Math.Clamp(_profile.GausianKernellSize | 1, 1, 19);
            FindCirclesCheck.IsChecked = _profile.FindCircles;
            FindRectanglesCheck.IsChecked = _profile.FindRectangles;
            FindLinesCheck.IsChecked = _profile.FindLines;
            FindCornersCheck.IsChecked = _profile.FindCorners;
            CannyCheck.IsChecked = _profile.UseCannyEdgeDetection;
            CannySlider.Value = Math.Clamp(_profile.CannyLowThreshold, 0, 255);
            ExposureBox.Text = _profile.Exposure.ToString(CultureInfo.CurrentCulture);
            BrightnessBox.Text = _profile.Brightness.ToString(CultureInfo.CurrentCulture);
            ContrastBox.Text = _profile.Contrast.ToString(CultureInfo.CurrentCulture);
            LightOnCheck.IsChecked = _profile.LightOn;
            ProfileStatus.Text = _profilePath is null ? "New local profile" : Path.GetFileName(_profilePath);
        }
        finally { _loadingProfile = false; }
        SyncProfileToPreview();
    }

    private void ProfileText_Changed(object sender, System.Windows.Controls.TextChangedEventArgs e) => UpdateProfile();

    private void ProfileSetting_Changed(object sender, RoutedEventArgs e) => UpdateProfile();

    private void ProfileSlider_Changed(object sender, RoutedPropertyChangedEventArgs<double> e) => UpdateProfile();

    private void CameraValue_LostFocus(object sender, RoutedEventArgs e) => UpdateProfile();

    private void UpdateProfile()
    {
        if (_loadingProfile || ProfileName is null || PrimaryThresholdSlider is null || LightOnCheck is null) return;
        _profile.Name = ProfileName.Text;
        _profile.Key = ProfileKey.Text;
        _profile.ApplyThreshold = ApplyThresholdCheck.IsChecked == true;
        _profile.Invert = InvertCheck.IsChecked == true;
        _profile.ApplyMask = ApplyMaskCheck.IsChecked == true;
        _profile.UseBlurredImage = BlurCheck.IsChecked == true;
        _profile.PrimaryThreshold = (byte)PrimaryThresholdSlider.Value;
        _profile.GausianKernellSize = (int)KernelSlider.Value | 1;
        _profile.FindCircles = FindCirclesCheck.IsChecked == true;
        _profile.FindRectangles = FindRectanglesCheck.IsChecked == true;
        _profile.FindLines = FindLinesCheck.IsChecked == true;
        _profile.FindCorners = FindCornersCheck.IsChecked == true;
        _profile.UseCannyEdgeDetection = CannyCheck.IsChecked == true;
        _profile.CannyLowThreshold = CannySlider.Value;
        _profile.LightOn = LightOnCheck.IsChecked == true;
        if (double.TryParse(ExposureBox.Text, CultureInfo.CurrentCulture, out var exposure)) _profile.Exposure = exposure;
        if (double.TryParse(BrightnessBox.Text, CultureInfo.CurrentCulture, out var brightness)) _profile.Brightness = brightness;
        if (double.TryParse(ContrastBox.Text, CultureInfo.CurrentCulture, out var contrast)) _profile.Contrast = contrast;
        SyncProfileToPreview();
        ProfileStatus.Text = (_profilePath is null ? "New local profile" : Path.GetFileName(_profilePath)) + " • Edited (not saved)";
    }

    private void SyncProfileToPreview()
    {
        if (PrimaryThresholdValue is null || ThresholdSlider is null) return;
        PrimaryThresholdValue.Text = $"{_profile.PrimaryThreshold} / 100";
        KernelValue.Text = $"{_profile.GausianKernellSize}";
        CannyValue.Text = $"{_profile.CannyLowThreshold:F0}";
        // Production preprocessing maps 0-100 profile threshold to 0-255 image intensity.
        ThresholdSlider.Value = _profile.PrimaryThreshold / 100.0 * 255.0;
        InvertThreshold = _profile.Invert;
        ProcessImage();
    }

    private void NewProfile_Click(object sender, RoutedEventArgs e)
    {
        _profile = new VisionProfile { Name = "New experiment", Key = VisionProfile.VisionProfile_Custom, Id = Guid.NewGuid().ToString("N") };
        _profilePath = null;
        ShowProfile();
    }

    private void OpenProfile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Open VisionProfile JSON", Filter = "JSON profiles (*.json)|*.json" };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            var profile = JsonSerializer.Deserialize<VisionProfile>(File.ReadAllText(dialog.FileName), JsonOptions);
            if (profile is null) throw new InvalidDataException("File did not contain a VisionProfile.");
            _profile = profile;
            _profilePath = dialog.FileName;
            ShowProfile();
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Unable to load profile", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void SaveProfile_Click(object sender, RoutedEventArgs e)
    {
        UpdateProfile();
        var dialog = new SaveFileDialog
        {
            Title = "Save VisionProfile JSON",
            Filter = "JSON profiles (*.json)|*.json",
            FileName = _profilePath is null ? "VisionProfile.json" : Path.GetFileName(_profilePath),
            InitialDirectory = _profilePath is null ? null : Path.GetDirectoryName(_profilePath)
        };
        if (dialog.ShowDialog(this) != true) return;
        try
        {
            File.WriteAllText(dialog.FileName, JsonSerializer.Serialize(_profile, JsonOptions));
            _profilePath = dialog.FileName;
            ProfileStatus.Text = "Saved: " + _profilePath;
        }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Unable to save profile", MessageBoxButton.OK, MessageBoxImage.Error); }
    }

    private void OpenImage_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Title = "Select camera image",
            Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff)|*.png;*.jpg;*.jpeg;*.bmp;*.tif;*.tiff|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) == true) LoadImage(dialog.FileName);
    }

    private void LoadImage(string path)
    {
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(Path.GetFullPath(path));
            image.EndInit();
            image.Freeze();
            _source = image;
            _filename = path;
            OriginalImage.Source = image;
            ProcessImage();
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Unable to open image", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void Threshold_Changed(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (ThresholdLabel is not null)
            ThresholdLabel.Text = ((int)e.NewValue).ToString();
        ProcessImage();
    }

    private void ProcessImage()
    {
        if (_source is null || ProcessedImage is null || ThresholdSlider is null) return;

        var gray = new FormatConvertedBitmap(_source, PixelFormats.Gray8, null, 0);
        var width = gray.PixelWidth;
        var height = gray.PixelHeight;
        var stride = width;
        var pixels = new byte[stride * height];
        gray.CopyPixels(pixels, stride, 0);

        int clippedDark = 0, clippedBright = 0, foreground = 0;
        var threshold = (int)ThresholdSlider.Value;
        for (int i = 0; i < pixels.Length; i++)
        {
            var intensity = pixels[i];
            if (intensity <= 2) clippedDark++;
            if (intensity >= 253) clippedBright++;

            bool white = intensity >= threshold;
            if (_invertThreshold) white = !white;
            pixels[i] = (byte)(white ? 255 : 0);
            if (white) foreground++;
        }

        var result = BitmapSource.Create(width, height, gray.DpiX, gray.DpiY,
                                         PixelFormats.Gray8, null, pixels, stride);
        result.Freeze();
        _processed = result;
        ProcessedImage.Source = result;

        double total = pixels.Length;
        StatusText.Text = $"{Path.GetFileName(_filename)}  •  {width}×{height}  •  Threshold {threshold}  •  " +
                          $"White {100 * foreground / total:F1}%  •  " +
                          $"Near-black input {100 * clippedDark / total:F1}%  •  " +
                          $"Near-white input {100 * clippedBright / total:F1}%";
    }

    private void SaveProcessed_Click(object sender, RoutedEventArgs e)
    {
        if (_processed is null) return;
        var dialog = new SaveFileDialog { Title = "Save processed image", Filter = "PNG (*.png)|*.png", FileName = "processed.png" };
        if (dialog.ShowDialog(this) != true) return;
        using var stream = File.Create(dialog.FileName);
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(_processed));
        encoder.Save(stream);
    }

    private void FitView_Click(object sender, RoutedEventArgs e)
    {
        OriginalScroll.ScrollToHome();
        ProcessedScroll.ScrollToHome();
    }

    private void Window_DragOver(object sender, DragEventArgs e)
    {
        e.Effects = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
        e.Handled = true;
    }

    private void Window_Drop(object sender, DragEventArgs e)
    {
        if (e.Data.GetData(DataFormats.FileDrop) is string[] files && files.Length > 0)
            LoadImage(files[0]);
    }
}
