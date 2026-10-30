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
    private string _cameraStatus = "Camera idle";
    public ObservableCollection<CameraDeviceInfo> CameraDevices { get; } = new();
    private CameraDeviceInfo? _selectedCamera;
    public CameraDeviceInfo? SelectedCamera { get => _selectedCamera; set { _selectedCamera = value; Notify(); } }
    public string CameraStatus { get => _cameraStatus; private set { _cameraStatus = value; Notify(); } }
    public void SetCameraDevices(IEnumerable<CameraDeviceInfo> devices)
    {
        CameraDevices.Clear();
        foreach (var device in devices) CameraDevices.Add(device);
        SelectedCamera ??= CameraDevices.FirstOrDefault();
        CameraStatus = CameraDevices.Count == 0 ? "No cameras found" : $"{CameraDevices.Count} camera(s) found";
    }
    public void ApplyCameraFrame(BitmapSource frame)
    {
        _inspectionProfile = null;
        _selectedTest = null;
        _imagePath = SelectedCamera?.Name ?? "Live camera";
        OriginalImage = frame;
        ProcessImage();
        CameraStatus = $"Live: {_imagePath} • {frame.PixelWidth}×{frame.PixelHeight}";
    }
    public void SetCameraError(string message) => CameraStatus = "Camera error: " + message;
    public void SetCameraStopped() => CameraStatus = "Camera stopped";

    private string? _imagePath;
    private string? _profilePath;
    private string _status = "Ready — open an image or drag one into this window.";
    private string _profileStatus = "New local profile";
    private bool _changed;
    public ObservableCollection<ImageTestCase> ImageTests { get; } = new(
        FeederVisionSuite.Scenarios.Select(scenario => new ImageTestCase
        {
            Name = scenario.Name, FileName = scenario.FileName,
            IsCanonical = scenario.ExpectedCircles.HasValue
        }));
    private VisionProfile? _inspectionProfile;
    private readonly Dictionary<string, List<TapeGeometry.Point>> _annotations = new();
    private ImageTestCase? _selectedTest;
    private string _annotationsStatus = "Select a test, then click three hole centers in the ORIGINAL image to annotate ground truth.";
    public string AnnotationsStatus { get => _annotationsStatus; private set { _annotationsStatus = value; Notify(); } }
    public string AllTestResults => BuildTestReport();
    public string BuildTestReport()
    {
        var profile = FeederVisionSuite.CreateProfile();
        var lines = new List<string>
        {
            $"Suite: {FeederVisionSuite.Version}",
            $"Profile: {profile.Name} ({profile.Id})",
            $"Hough: DP={profile.HoughCirclesDP}, P1={profile.HoughCirclesParam1}, P2={profile.HoughCirclesParam2}, radii={profile.HoughCirclesMinRadius}-{profile.HoughCirclesMaxRadius}mm, minDist={profile.HoughCirclesMinDistance}mm, ppm={profile.PixelsPerMM}, pitch={FeederVisionSuite.HolePitchMm}mm",
            $"Summary: {TestRunStatus}"
        };
        lines.AddRange(ImageTests.Select(t => t.Summary));
        foreach (var item in _annotations.OrderBy(kv => kv.Key))
            lines.Add($"Provisional annotation {item.Key}: " +
                string.Join("; ", item.Value.OrderBy(p => p.Y).Select(p => $"({p.X:F1},{p.Y:F1})")));
        return string.Join(Environment.NewLine, lines);
    }
    public void AnnotateSelectedHole(double x, double y)
    {
        if (_selectedTest is null) return;
        if (!_annotations.TryGetValue(_selectedTest.FileName, out var points))
            _annotations[_selectedTest.FileName] = points = new List<TapeGeometry.Point>();
        if (points.Count >= 3) points.Clear();
        points.Add(new TapeGeometry.Point(x, y));
        AnnotationsStatus = $"{_selectedTest.Name}: {points.Count}/3 expected centers marked (provisional, in-memory)";
        ProcessImage();
    }
    public void ClearAnnotations()
    {
        if (_selectedTest is null) return;
        _annotations.Remove(_selectedTest.FileName);
        AnnotationsStatus = $"{_selectedTest.Name}: annotations cleared";
        ProcessImage();
    }
    public string TestProfileLabel => $"Test suite {FeederVisionSuite.Version} • profile controlled by code, not working profile";
    private string _testRunStatus = "Not run. Press Run image tests.";
    public string TestRunStatus { get => _testRunStatus; private set { _testRunStatus = value; Notify(); } }
    private static readonly string TestImageFolder = Path.Combine(AppContext.BaseDirectory, "TestImages", "Feeder1");
    private double _tapeHolePitchMm = 4.0;
    private double _pickupAlongMm;
    private double _pickupCrossMm = 3.5;
    private double _componentPitchMm = 4.0;
    public double ComponentPitchMm
    {
        get => _componentPitchMm;
        set { if (!double.IsFinite(value) || value <= 0) return; _componentPitchMm = value; Notify(); ProcessImage(); }
    }
    private string _pickupSummary = "Waiting for a validated tape-hole row.";
    public double PickupAlongMm
    {
        get => _pickupAlongMm;
        set { if (!double.IsFinite(value)) return; _pickupAlongMm = value; Notify(); ProcessImage(); }
    }
    public double PickupCrossMm
    {
        get => _pickupCrossMm;
        set { if (!double.IsFinite(value)) return; _pickupCrossMm = value; Notify(); ProcessImage(); }
    }
    public string PickupSummary { get => _pickupSummary; private set { _pickupSummary = value; Notify(); } }
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
        _inspectionProfile = null;
        _selectedTest = null;
        LoadImageCore(path);
    }

    private void LoadImageCore(string path)
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
        _selectedTest = test;
        _inspectionProfile = FeederVisionSuite.CreateProfile();
        LoadImageCore(path);
        AnnotationsStatus = $"{test.Name}: click three hole centers in ORIGINAL image to annotate ground truth.";
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
                var circles = _detector.Detect(OriginalImage, FeederVisionSuite.CreateProfile()).Circles;
                var sequence = TapeHoleSequence.Evaluate(
                    circles.Select(circle => new TapeGeometry.Point(circle.X, circle.Y)).ToArray(),
                    FeederVisionSuite.HolePitchMm, FeederVisionSuite.CreateProfile().PixelsPerMM);
                var errorReport = "";
                if (_annotations.TryGetValue(test.FileName, out var expectedCenters) && expectedCenters.Count == 3 && sequence.Accepted.Count >= 3)
                {
                    var actual = sequence.Accepted.OrderBy(point => point.Y).ToArray();
                    var expected = expectedCenters.OrderBy(point => point.Y).ToArray();
                    var distances = Enumerable.Range(0, 3).Select(i =>
                        Math.Sqrt(Math.Pow(actual[i].X - expected[i].X, 2) + Math.Pow(actual[i].Y - expected[i].Y, 2))).ToArray();
                    errorReport = $" | annotated center error avg {distances.Average():F1}px max {distances.Max():F1}px (provisional)";
                }
                if (test.IsCanonical)
                {
                    bool okay = circles.Count == 3 && sequence.Valid && sequence.Accepted.Count == 3 &&
                        sequence.Geometry is not null &&
                        Math.Abs(sequence.Geometry.PitchPixels - FeederVisionSuite.CanonicalExpectedSpacingPx) <= FeederVisionSuite.CanonicalSpacingTolerancePx;
                    test.Result = (okay ? "PASS" : "FAIL") +
                        $" | {imageSize} | circles {circles.Count}/3 | accepted {sequence.Accepted.Count}/3" +
                        (sequence.Geometry is null ? " | pitch unavailable" :
                            $" | pitch {sequence.Geometry.PitchPixels:F1}px (target ~202px)");
                    test.Result += errorReport;
                    if (okay) passed++; else failed++;
                }
                else
                {
                    test.Result = $"OBSERVED | {imageSize} | circles {circles.Count} | accepted {sequence.Accepted.Count} | exploratory";
                    test.Result += errorReport;
                    explored++;
                }
            }
            catch (Exception ex) { test.Result = $"ERROR ({ex.Message})"; failed++; }
        }
        Notify(nameof(AllTestResults));
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

    private static BitmapSource DrawOverlay(int width, int height, VisionDetectionResult detection, IReadOnlyList<TapeGeometry.Point>? expected, FeederPickupGeometry.Result? pickup = null, IReadOnlyList<TapeGeometry.Point>? pickupPoints = null, TapeGeometry.Point? lastHole = null)
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
            if (expected is not null)
            {
                var markerPen = new Pen(Brushes.DeepSkyBlue, 2);
                foreach (var p in expected)
                {
                    dc.DrawLine(markerPen, new Point(p.X - 9, p.Y - 9), new Point(p.X + 9, p.Y + 9));
                    dc.DrawLine(markerPen, new Point(p.X - 9, p.Y + 9), new Point(p.X + 9, p.Y - 9));
                }
            }
            if (pickup is not null)
            {
                var axisPen = new Pen(Brushes.Cyan, 1.5) { DashStyle = DashStyles.Dash };
                var end = lastHole ?? pickup.NextHole;
                dc.DrawLine(axisPen, new Point(pickup.ReferenceHole.X, pickup.ReferenceHole.Y),
                    new Point(end.X, end.Y));
                var targetPen = new Pen(Brushes.Magenta, 2);
                foreach (var target in pickupPoints ?? new[] { pickup.Pickup })
                {
                    var center = new Point(target.X, target.Y);
                    dc.DrawEllipse(null, targetPen, center, 7, 7);
                    dc.DrawLine(targetPen, new Point(center.X - 12, center.Y), new Point(center.X + 12, center.Y));
                    dc.DrawLine(targetPen, new Point(center.X, center.Y - 12), new Point(center.X, center.Y + 12));
                }
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
        var activeProfile = _inspectionProfile ?? Profile;
        int threshold = (int)(activeProfile.PrimaryThreshold / 100.0 * 255.0);
        for (int i = 0; i < pixels.Length; i++)
        {
            var value = pixels[i];
            if (value <= 2) dark++;
            if (value >= 253) bright++;
            byte outputPixel;
            if (activeProfile.ApplyThreshold)
            {
                bool white = value >= threshold;
                if (activeProfile.Invert) white = !white;
                outputPixel = (byte)(white ? 255 : 0);
            }
            else
            {
                outputPixel = activeProfile.Invert ? (byte)(255 - value) : value;
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
            var detection = _detector.Detect(OriginalImage, activeProfile);
            DetectionOverlay = DrawOverlay(width, height, detection,
                _selectedTest is not null && _annotations.TryGetValue(_selectedTest.FileName, out var expected)
                    ? expected : null);
            CircleMeasurements.Clear();
            for (int i = 0; i < detection.Circles.Count; i++)
            {
                var c = detection.Circles[i];
                CircleMeasurements.Add($"#{i + 1}: X={c.X:F1}  Y={c.Y:F1}  R={c.Radius:F1}");
            }
            CircleSummary = $"{detection.Circles.Count} circle(s) found";
            var sequence = TapeHoleSequence.Evaluate(
                detection.Circles.Select(c => new TapeGeometry.Point(c.X, c.Y)).ToArray(),
                _inspectionProfile is null ? TapeHolePitchMm : FeederVisionSuite.HolePitchMm,
                activeProfile.PixelsPerMM);
            var geometry = sequence.Valid ? sequence.Geometry : null;
            var prediction = FeederPickupGeometry.Predict(geometry, PickupAlongMm, PickupCrossMm,
                _inspectionProfile is null ? TapeHolePitchMm : FeederVisionSuite.HolePitchMm);
            PickupSummary = "No reliable row; pickup prediction withheld.";
            if (prediction is not null && geometry is not null)
            {
                var points = new List<TapeGeometry.Point>();
                // Preview only the measured span; don't extrapolate without feeder configuration.
                var lengthMm = (geometry.Count - 1) * geometry.PitchPixels / geometry.PixelsPerMm;
                for (double along = PickupAlongMm; along <= lengthMm + 0.001 && points.Count < 24; along += ComponentPitchMm)
                {
                    if (along < -0.001) continue;
                    var candidate = FeederPickupGeometry.Predict(geometry, along, PickupCrossMm,
                        _inspectionProfile is null ? TapeHolePitchMm : FeederVisionSuite.HolePitchMm);
                    if (candidate is not null) points.Add(candidate.Pickup);
                }
                PickupSummary = $"Hole axis: ({geometry.Start.X:F1}, {geometry.Start.Y:F1}) to ({geometry.End.X:F1}, {geometry.End.Y:F1}) px • {points.Count} preview pickup(s) at {ComponentPitchMm:F2} mm spacing • image coordinates only";
                DetectionOverlay = DrawOverlay(width, height, detection,
                    _selectedTest is not null && _annotations.TryGetValue(_selectedTest.FileName, out var marks) ? marks : null,
                    prediction, points, geometry.End);
            }
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
            PickupSummary = "Detection failed; pickup prediction unavailable.";
            measurementStatus = $"Detection unavailable: {ex.GetType().Name}: {ex.Message}";
        }
        Status = $"{Path.GetFileName(_imagePath)} • {width}×{height} • Threshold {threshold} • White {100 * whiteCount / count:F1}% • Near-black input {100 * dark / count:F1}% • Near-white input {100 * bright / count:F1}% • {measurementStatus}";
    }
}
