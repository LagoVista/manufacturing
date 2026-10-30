using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Util;
using LagoVista.PickAndPlace.Models;
using System.Drawing;
using System.IO;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.VisionWorkbench.Services;

public sealed record FoundCircle(float X, float Y, float Radius);
public sealed record FoundRectangle(float X, float Y, float Width, float Height, float Angle);

public sealed class VisionDetectionResult
{
    public List<FoundCircle> Circles { get; } = new();
    public List<FoundRectangle> Rectangles { get; } = new();
}

/// <summary>
/// Offline, image-based measurement. Uses the same Hough/contour primitives and
/// VisionProfile settings as the production detectors, without machine or WPF view models.
/// </summary>
public sealed class VisionDetectionService
{
    public VisionDetectionResult Detect(BitmapSource source, VisionProfile profile)
    {
        var result = new VisionDetectionResult();
        if (!profile.FindCircles && !profile.FindRectangles) return result;

        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(source));
        using var stream = new MemoryStream();
        encoder.Save(stream);
        using var color = new Mat();
        CvInvoke.Imdecode(stream.ToArray(), ImreadModes.Color, color);
        using var gray = new Mat();
        CvInvoke.CvtColor(color, gray, ColorConversion.Bgr2Gray);
        using var work = new Mat();

        if (profile.ApplyThreshold)
        {
            var threshold = profile.PrimaryThreshold / 100.0 * 255.0;
            CvInvoke.Threshold(gray, work, threshold, 255, ThresholdType.Binary);
        }
        else gray.CopyTo(work);

        if (profile.Invert) CvInvoke.BitwiseNot(work, work);
        if (profile.UseBlurredImage)
        {
            var kernel = Math.Max(1, profile.GausianKernellSize) | 1;
            using var blurred = new Mat();
            CvInvoke.GaussianBlur(work, blurred, new Size(kernel, kernel), Math.Max(0, profile.GaussianSigma));
            blurred.CopyTo(work);
        }

        if (profile.FindCircles)
        {
            var ppm = Math.Max(0.001, profile.PixelsPerMM);
            var minRadius = Math.Max(1, (int)Math.Round(profile.HoughCirclesMinRadius * ppm));
            var maxRadius = Math.Max(minRadius + 1, (int)Math.Round(profile.HoughCirclesMaxRadius * ppm));
            var circles = CvInvoke.HoughCircles(work, HoughModes.Gradient,
                Math.Max(1, profile.HoughCirclesDP),
                Math.Max(1, profile.HoughCirclesMinDistance * ppm),
                Math.Max(1, profile.HoughCirclesParam1),
                Math.Max(1, profile.HoughCirclesParam2),
                minRadius, maxRadius);
            foreach (var circle in circles)
                result.Circles.Add(new FoundCircle(circle.Center.X, circle.Center.Y, circle.Radius));
        }

        if (profile.FindRectangles)
        {
            using var edges = new Mat();
            if (profile.UseCannyEdgeDetection)
                CvInvoke.Canny(work, edges, profile.CannyLowThreshold, profile.CannyHighThreshold,
                    profile.CannyApetureSize, profile.CannyGradient);
            else
                CvInvoke.Threshold(work, edges, profile.ThresholdEdgeDetection, 255, ThresholdType.Binary);
            using var contours = new VectorOfVectorOfPoint();
            var retrieval = profile.ContourRetrieveMode?.Value switch
            {
                ContourRetrieveModes.External => RetrType.External,
                ContourRetrieveModes.Tree => RetrType.Tree,
                ContourRetrieveModes.TwoLevelHierarchy => RetrType.Ccomp,
                _ => RetrType.List
            };
            CvInvoke.FindContours(edges, contours, null, retrieval, ChainApproxMethod.ChainApproxSimple);
            for (var i = 0; i < contours.Size; i++)
            {
                using var contour = contours[i];
                var area = CvInvoke.ContourArea(contour);
                if (area < profile.ContourMinArea || area > profile.CountourMaxArea) continue;
                var rect = CvInvoke.MinAreaRect(contour);
                if (rect.Size.Width <= 0 || rect.Size.Height <= 0) continue;
                result.Rectangles.Add(new FoundRectangle(rect.Center.X, rect.Center.Y,
                    rect.Size.Width, rect.Size.Height, rect.Angle));
            }
        }
        return result;
    }
}
