using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.App.Controls.Machine.Calibration
{
    public enum CalibrationMarkerKind { Target, Candidate, Accepted, Rejected }

    /// <summary>
    /// A marker in the pixel coordinates of the ACTUAL displayed image frame.
    /// No machine offsets or raw sensor coordinates should be passed here
    /// without first mapping them to the rendered image's pixel space.
    /// </summary>
    public sealed class CalibrationVideoMarker
    {
        public double CenterX { get; set; }
        public double CenterY { get; set; }
        public double RadiusPixels { get; set; }
        public string Label { get; set; }
        public CalibrationMarkerKind Kind { get; set; }
    }

    /// <summary>
    /// Common camera annotation surface. Uses the same Uniform image mapping
    /// as the underlying Image, including letterbox offsets. All annotations
    /// are drawn together from frame-pixel data; callers do not hand-position
    /// XAML elements or duplicate overlay colors across calibration screens.
    /// </summary>
    public sealed class CalibrationVideoOverlay : FrameworkElement
    {
        public static readonly DependencyProperty FrameSourceProperty =
            DependencyProperty.Register(nameof(FrameSource), typeof(ImageSource),
                typeof(CalibrationVideoOverlay), new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty ShowExposureReferenceProperty =
            DependencyProperty.Register(nameof(ShowExposureReference), typeof(bool),
                typeof(CalibrationVideoOverlay), new FrameworkPropertyMetadata(false,
                    FrameworkPropertyMetadataOptions.AffectsRender));
        public static readonly DependencyProperty MarkersProperty =
            DependencyProperty.Register(nameof(Markers), typeof(IEnumerable<CalibrationVideoMarker>),
                typeof(CalibrationVideoOverlay), new FrameworkPropertyMetadata(null,
                    FrameworkPropertyMetadataOptions.AffectsRender));

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

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);
            if (!(FrameSource is BitmapSource frame) || frame.PixelWidth <= 0 || frame.PixelHeight <= 0)
                return;

            // Image.Stretch=Uniform: never assume square frames or stretch marker
            // coordinates to fill an unrelated camera presentation rectangle.
            var frameWidth = (double)frame.PixelWidth;
            var frameHeight = (double)frame.PixelHeight;
            if (!CalibrationImageTransform.TryCreate(frameWidth, frameHeight,
                    RenderSize.Width, RenderSize.Height, out var mapping)) return;
            var scale = mapping.Scale;
            Point Map(double x, double y) => new Point(mapping.MapX(x), mapping.MapY(y));

            var center = Map(frameWidth / 2.0, frameHeight / 2.0);
            var line = new Pen(Brushes.DeepSkyBlue, 1.25);
            drawingContext.DrawLine(line, new Point(center.X - 13, center.Y),
                new Point(center.X + 13, center.Y));
            drawingContext.DrawLine(line, new Point(center.X, center.Y - 13),
                new Point(center.X, center.Y + 13));

            if (ShowExposureReference)
            {
                // These normalized radii match the production reference
                // annulus in ImageCaptureService.MeasureReferenceLevels.
                var smallestSide = Math.Min(frameWidth, frameHeight) * scale;
                var reference = new Pen(Brushes.DeepSkyBlue, 1.75)
                {
                    DashStyle = DashStyles.Dash
                };
                drawingContext.DrawEllipse(null, reference, center,
                    smallestSide * 0.15, smallestSide * 0.15);
                drawingContext.DrawEllipse(null, reference, center,
                    smallestSide * 0.35, smallestSide * 0.35);
                DrawLabel(drawingContext, "EXPOSURE SAMPLE ROI", Map(frameWidth / 2.0, frameHeight / 2.0 - Math.Min(frameWidth, frameHeight) * 0.35), Brushes.DeepSkyBlue);
            }

            if (Markers == null) return;
            foreach (var marker in Markers)
            {
                if (marker == null || marker.RadiusPixels < 0) continue;
                var paint = marker.Kind == CalibrationMarkerKind.Accepted ? Brushes.LimeGreen :
                    marker.Kind == CalibrationMarkerKind.Rejected ? Brushes.OrangeRed :
                    marker.Kind == CalibrationMarkerKind.Candidate ? Brushes.Gold : Brushes.DeepSkyBlue;
                var point = Map(marker.CenterX, marker.CenterY);
                var radius = marker.RadiusPixels * scale;
                drawingContext.DrawEllipse(null, new Pen(paint, 2), point, radius, radius);
                if (!String.IsNullOrWhiteSpace(marker.Label))
                    DrawLabel(drawingContext, marker.Label,
                        new Point(point.X + radius + 7, point.Y - radius), paint);
            }
        }

        private void DrawLabel(DrawingContext dc, string label, Point at, Brush color)
        {
            var text = new FormattedText(label, CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight, new Typeface("Segoe UI"), 11,
                color, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawRectangle(new SolidColorBrush(Color.FromArgb(185, 9, 16, 25)),
                null, new Rect(at.X - 3, at.Y - text.Height - 5,
                    text.Width + 6, text.Height + 5));
            dc.DrawText(text, new Point(at.X, at.Y - text.Height - 2));
        }
    }
}
