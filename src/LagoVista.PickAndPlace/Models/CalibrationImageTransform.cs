using System;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Maps an observation in displayed frame pixels onto a WPF video surface
    /// using the same Uniform/letterbox mapping as the live image renderer.
    /// This is independent of camera hardware, image processing and WPF.
    /// </summary>
    public sealed class CalibrationImageTransform
    {
        private CalibrationImageTransform(double imageWidth, double imageHeight,
            double surfaceWidth, double surfaceHeight)
        {
            ImageWidth = imageWidth;
            ImageHeight = imageHeight;
            Scale = Math.Min(surfaceWidth / imageWidth, surfaceHeight / imageHeight);
            Left = (surfaceWidth - imageWidth * Scale) / 2.0;
            Top = (surfaceHeight - imageHeight * Scale) / 2.0;
        }

        public double ImageWidth { get; }
        public double ImageHeight { get; }
        public double Scale { get; }
        public double Left { get; }
        public double Top { get; }
        public double MapX(double imagePixelX) => Left + imagePixelX * Scale;
        public double MapY(double imagePixelY) => Top + imagePixelY * Scale;
        public double Radius(double imagePixelRadius) => imagePixelRadius * Scale;

        public static bool TryCreate(double imageWidth, double imageHeight,
            double surfaceWidth, double surfaceHeight, out CalibrationImageTransform transform)
        {
            transform = null;
            if (!Valid(imageWidth) || !Valid(imageHeight) ||
                !Valid(surfaceWidth) || !Valid(surfaceHeight))
                return false;
            transform = new CalibrationImageTransform(imageWidth, imageHeight,
                surfaceWidth, surfaceHeight);
            return true;
        }

        private static bool Valid(double value) => value > 0 &&
            !Double.IsNaN(value) && !Double.IsInfinity(value);
    }
}
