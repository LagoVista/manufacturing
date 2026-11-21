using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace LagoVista.PickAndPlace.App.Controls.Machine.Calibration
{
    /// <summary>Reusable live-image host for independent calibration steps.</summary>
    public partial class CalibrationVideoSurface : UserControl
    {
        public CalibrationVideoSurface() { InitializeComponent(); }

        public static readonly DependencyProperty FrameSourceProperty =
            DependencyProperty.Register(nameof(FrameSource), typeof(ImageSource),
                typeof(CalibrationVideoSurface), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowExposureReferenceProperty =
            DependencyProperty.Register(nameof(ShowExposureReference), typeof(bool),
                typeof(CalibrationVideoSurface), new PropertyMetadata(false));
        public static readonly DependencyProperty MarkersProperty =
            DependencyProperty.Register(nameof(Markers), typeof(IEnumerable<CalibrationVideoMarker>),
                typeof(CalibrationVideoSurface), new PropertyMetadata(null));
        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register(nameof(Caption), typeof(string),
                typeof(CalibrationVideoSurface), new PropertyMetadata("LIVE CAMERA"));

        public ImageSource FrameSource
        {
            get => (ImageSource)GetValue(FrameSourceProperty);
            set => SetValue(FrameSourceProperty, value);
        }
        public bool ShowExposureReference
        {
            get => (bool)GetValue(ShowExposureReferenceProperty);
            set => SetValue(ShowExposureReferenceProperty, value);
        }
        public IEnumerable<CalibrationVideoMarker> Markers
        {
            get => (IEnumerable<CalibrationVideoMarker>)GetValue(MarkersProperty);
            set => SetValue(MarkersProperty, value);
        }
        public string Caption
        {
            get => (string)GetValue(CaptionProperty);
            set => SetValue(CaptionProperty, value);
        }
    }
}
