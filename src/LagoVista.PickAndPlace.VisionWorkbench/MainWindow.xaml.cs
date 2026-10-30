using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.VisionWorkbench;

public partial class MainWindow : Window
{
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
