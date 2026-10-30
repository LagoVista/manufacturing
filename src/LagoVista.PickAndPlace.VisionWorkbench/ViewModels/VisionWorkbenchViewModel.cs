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

public sealed record RecentImageEntry(string Path)
{
    public string DisplayName => System.IO.Path.GetFileName(Path);
}

public sealed class ImageTestCase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public string Name { get; init; } = "";
    public string FileName { get; init; } = "";
    public bool IsCanonical { get; init; }
    private string _result = "Not run";
    public string Result { get => _result; set { _result = value; PropertyChanged?.Invoke(this, new(nameof(Summary))); } }
    public string Summary => $"{Name}: {Result}";
}

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
    public ObservableCollection<ImageTestCase> ImageTests { get; } = new()
    {
        new() { Name = "000 Canonical", FileName = "Feeder1-000-Canonical.png", IsCanonical = true },
        new() { Name = "010 Low light", FileName = "Feeder1-010-LowLight.png" },
        new() { Name = "020 High exposure", FileName = "Feeder1-020-HighExposure.png" },
        new() { Name = "030 Rotated", FileName = "Feeder1-030-Rotated.png" },
        new() { Name = "040 Extra circle", FileName = "Feeder1-040-ExtraCircle.png" }
    };
    private string _testRunStatus = "Not run. Press Run image tests.";
    public string TestRunStatus { get => _testRunStatus; private set { _testRunStatus = value; Notify(); } }
    private static readonly string TestImageFolder = Path.Combine(AppContext.BaseDirectory, "TestImages", "Feeder1");
    private double _tapeHolePitchMm = 4.0;
    private string _tapeGeometrySummary = "Detect at least two consecutive tape holes.";
    public double TapeHolePitchMm
    {
        get => _tapeHolePitchMm;
        set
        {
            if (!double.IsFinite(value) || value <= 0) return;
            _tapeHolePitchMm = value;
            Notify();
            ProcessImage();
            SaveSession();
        }
    }
    public string TapeGeometrySummary
    {
        get => _tapeGeometrySummary;
        private set { _tapeGeometrySummary = value; Notify(); }
    }
    public ObservableCollection<RecentImageEntry> RecentImages { get; } = new();
    private static readonly string RecentImagesFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LagoVista", "VisionWorkbench", "recent-images.json");
    private const int MaxRecentImages = 12;
    private static readonly string SessionFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LagoVista", "VisionWorkbench", "last-session.json");
    private sealed record WorkbenchSession(VisionProfile Profile, double TapeHolePitchMm, string? ProfilePath, bool WasEdited);

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
            SaveSession();
        }
    }

    public VisionWorkbenchViewModel()
    {
        _profile.PropertyChanged += ProfileChanged;
        LoadRecentImages();
        RestoreSession();
    }

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
        SaveSession();
    }

    public void CreateProfile()
    {
        _profilePath = null;
        Profile = NewProfile();
        _changed = false;
        ProfileStatus = "New local profile";
        SaveSession();
    }

    public void LoadProfile(string path)
    {
        var value = JsonSerializer.Deserialize<VisionProfile>(File.ReadAllText(path), JsonOptions)
            ?? throw new InvalidDataException("File did not contain a VisionProfile.");
        _profilePath = path;
        Profile = value;
        _changed = false;
        ProfileStatus = Path.GetFileName(path);
        SaveSession();
    }

    public string? ProfilePath => _profilePath;
    public bool HasUnsavedChanges => _changed;

    public void SaveProfile(string path)
    {
        File.WriteAllText(path, JsonSerializer.Serialize(Profile, JsonOptions));
        _profilePath = path;
        _changed = false;
        ProfileStatus = "Saved: " + path;
        SaveSession();
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
        RememberImage(path);
    }

    private void RestoreSession()
    {
        try
        {
            if (!File.Exists(SessionFile)) return;
            var session = JsonSerializer.Deserialize<WorkbenchSession>(File.ReadAllText(SessionFile), JsonOptions);
            if (session?.Profile is null) return;
            if (double.IsFinite(session.TapeHolePitchMm) && session.TapeHolePitchMm > 0)
            {
                _tapeHolePitchMm = session.TapeHolePitchMm;
                Notify(nameof(TapeHolePitchMm));
            }
            _profilePath = session.ProfilePath;
            Profile = session.Profile;
            _changed = session.WasEdited;
            ProfileStatus = _changed
                ? "Restored working profile • Edited (not saved)"
                : _profilePath is null ? "Restored local working profile" : "Restored: " + Path.GetFileName(_profilePath);
        }
        catch (Exception)
        {
            // Invalid user-local session must never prevent launching the workbench.
        }
    }

    private void SaveSession()
    {
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(SessionFile)!);
            var snapshot = new WorkbenchSession(Profile, TapeHolePitchMm, _profilePath, _changed);
            var temp = SessionFile + ".tmp";
            File.WriteAllText(temp, JsonSerializer.Serialize(snapshot, JsonOptions));
            File.Move(temp, SessionFile, true);
        }
        catch (Exception)
        {
            // Persistence of experimental UI state is best-effort; never block detection.
        }
    }

    private void LoadRecentImages()
    {
        try
        {
            if (!File.Exists(RecentImagesFile)) return;
            var saved = JsonSerializer.Deserialize<List<string>>(File.ReadAllText(RecentImagesFile)) ?? new();
            foreach (var path in saved.Where(File.Exists).Distinct(StringComparer.OrdinalIgnoreCase).Take(MaxRecentImages))
                RecentImages.Add(new RecentImageEntry(path));
        }
        catch (Exception) { /* Corrupt local history must not prevent launching the workbench. */ }
    }

    public void RestoreLastImage()
    {
        if (RecentImages.Count > 0)
            LoadImage(RecentImages[0].Path);
    }

    private void RememberImage(string imagePath)
    {
        var fullPath = Path.GetFullPath(imagePath);
        var old = RecentImages.FirstOrDefault(item => string.Equals(item.Path, fullPath, StringComparison.OrdinalIgnoreCase));
        if (old is not null) RecentImages.Remove(old);
        RecentImages.Insert(0, new RecentImageEntry(fullPath));
        while (RecentImages.Count > MaxRecentImages) RecentImages.RemoveAt(RecentImages.Count - 1);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(RecentImagesFile)!);
            File.WriteAllText(RecentImagesFile, JsonSerializer.Serialize(RecentImages.Select(item => item.Path).ToArray(), JsonOptions));
        }
        catch (IOException) { /* Image loading works even if the MRU cannot be saved. */ }
        catch (UnauthorizedAccessException) { /* Settings storage is optional. */ }
    }

    public void ShowImageTest(ImageTestCase test)
    {
        var path = Path.Combine(TestImageFolder, test.FileName);
        if (!File.Exists(path)) throw new FileNotFoundException("Test image missing from output folder.", path);
        LoadImage(path);
    }

    public async Task RunImageTestsAsync()
    {
        int passed = 0, failed = 0, explored = 0;
        foreach (var test in ImageTests)
        {
            test.Result = "Running...";
            TestRunStatus = $"Testing {test.Name}...";
            await Task.Yield();
            try
            {
                ShowImageTest(test);
                var imageSize = $"{OriginalImage!.PixelWidth}×{OriginalImage.PixelHeight}";
                var circles = _detector.Detect(OriginalImage, Profile).Circles;
                var sequence = TapeHoleSequence.Evaluate(
                    circles.Select(circle => new TapeGeometry.Point(circle.X, circle.Y)).ToArray(),
                    TapeHolePitchMm, Profile.PixelsPerMM);
                if (test.IsCanonical)
                {
                    bool okay = circles.Count == 3 && sequence.Valid && sequence.Accepted.Count == 3 &&
                        sequence.Geometry is not null &&
                        Math.Abs(sequence.Geometry.PitchPixels - 202.03) <= 8.0;
                    test.Result = (okay ? "PASS" : "FAIL") +
                        $" | {imageSize} | circles {circles.Count}/3 | accepted {sequence.Accepted.Count}/3" +
                        (sequence.Geometry is null ? " | pitch unavailable" :
                            $" | pitch {sequence.Geometry.PitchPixels:F1}px (target ~202px)");
                    if (okay) passed++; else failed++;
                }
                else
                {
                    test.Result = $"OBSERVED | {imageSize} | circles {circles.Count} | accepted {sequence.Accepted.Count} | exploratory";
                    explored++;
                }
            }
            catch (Exception ex) { test.Result = $"ERROR ({ex.Message})"; failed++; }
        }
        TestRunStatus = $"Canonical: {passed} passed / {failed} failed • Exploratory: {explored} observed. Select a row to inspect.";
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
            var sequence = TapeHoleSequence.Evaluate(
                detection.Circles.Select(c => new TapeGeometry.Point(c.X, c.Y)).ToArray(),
                TapeHolePitchMm, Profile.PixelsPerMM);
            var geometry = sequence.Geometry;
            TapeGeometrySummary = sequence.Reason +
                $" • Accepted {sequence.Accepted.Count} / {detection.Circles.Count} • Rejected {sequence.Rejected}" +
                (sequence.Missing > 0 ? $" • Possible missing: {sequence.Missing}" : "") +
                (geometry is null ? " • Calibration withheld" :
                    $" • {geometry.PitchPixels:F2} px/hole • {geometry.PixelsPerMm:F2} px/mm (estimated)" +
                    $" • Tilt {geometry.AngleFromVerticalDegrees:+0.00;-0.00;0.00}°" +
                    $" • Spacing variation {geometry.PitchVariationPixels:F2} px • Max line error {geometry.MaxLineErrorPixels:F2} px");
            measurementStatus = $"{detection.Circles.Count} circles • {detection.Rectangles.Count} rectangles";
        }
        catch (Exception ex)
        {
            DetectionOverlay = null;
            CircleMeasurements.Clear();
            CircleSummary = "Detection failed";
            TapeGeometrySummary = "Detection failed; no tape geometry available.";
            measurementStatus = $"Detection unavailable: {ex.GetType().Name}: {ex.Message}";
        }
        Status = $"{Path.GetFileName(_imagePath)} • {width}×{height} • Threshold {threshold} • White {100 * whiteCount / count:F1}% • Near-black input {100 * dark / count:F1}% • Near-white input {100 * bright / count:F1}% • {measurementStatus}";
    }
}
