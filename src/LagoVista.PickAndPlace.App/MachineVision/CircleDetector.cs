// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b1455b029fe86d3df1f67aa03aff42940f6bb30f0d3b677cdfaaf22e49a5ab92
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV.CvEnum;
using Emgu.CV;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public class CircleDetector : ICircleDetector<IInputOutputArray>
    {
        private ObservableCollection<MVLocatedCircle> _foundCircles = new ObservableCollection<MVLocatedCircle>();
        private readonly ILocatorViewModel _locatorViewModel;

        int _iteration = 0;

        public string LatestNozzleRejection { get; private set; }
        public string LatestCalibrationDiscDiagnostic { get; private set; } =
            "No calibration-disc frame processed yet.";

        public CircleDetector(ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
        }

        public void FindCircles(IMVImage<IInputOutputArray> input, MachineCamera camera,
            System.Drawing.Size size, double? expectedOpeningDiameterOverrideMm = null)
        {
            _iteration++;
            LatestNozzleRejection = null;

            var profile = camera.CurrentVisionProfile;
            var center = size.Center();

            var adaptiveNozzle = profile.UseAdaptiveDarkOpening &&
                (profile.Key == VisionProfile.VisionProfile_Nozzle ||
                 profile.Key == VisionProfile.VisionProfile_NozzleCalibration);
            var scaledTarget = adaptiveNozzle
                ? (int)Math.Round(DarkCircularOpeningLocator.EffectiveAcquisitionRadiusPixels(
                    size.Width, size.Height, camera.EffectivePixelsPerMM, profile.TargetImageRadius))
                : Convert.ToInt32(profile.TargetImageRadius * camera.EffectivePixelsPerMM);
            var stagingPlateHole = profile.Key == VisionProfile.VisionProfile_StagingPlateHole;
            // Search-region position is profile-controlled for staging holes.
            // The MVLocatedCircle optical center stays at frame center, so
            // moving the ROI does NOT change the target machine alignment.
            var searchCenter = stagingPlateHole
                ? StagingHoleSearchRegion.Center(size.Width, size.Height,
                    camera.EffectivePixelsPerMM, profile.StagingSearchOffsetXmm,
                    profile.StagingSearchOffsetYmm)
                : new LagoVista.Core.Models.Drawing.Point2D<double>(center.X, center.Y);
            var search = new CircleF(new System.Drawing.PointF(
                (float)searchCenter.X, (float)searchCenter.Y), scaledTarget);
            var calibrationDisc = profile.Key == VisionProfile.VisionProfile_CalibrationDisc;
            CircleF[] circles;
            if (stagingPlateHole || calibrationDisc)
            {
                // Both are bright filled targets on dark backgrounds. Reuse
                // Otsu/contour-area centroids instead of a jittery Hough edge.
                // Calibration Disc additionally verifies shape circularity.
                var located = FindStagingPlateHole(input, camera, profile, search,
                    center, calibrationDisc, out var contourDiagnostics);
                if (calibrationDisc)
                    LatestCalibrationDiscDiagnostic = contourDiagnostics;
                circles = located.HasValue ? new[] { located.Value } : Array.Empty<CircleF>();
            }
            else if (adaptiveNozzle)
            {
                // Match the verified dark core in raw grayscale, independently
                // of the profile's legacy Hough and manual threshold settings.
                var gray = input.Image as Image<Gray, byte>;
                var rejection = new DarkCircularOpeningLocator.RejectionDiagnostics();
                var opening = gray == null ? null : DarkCircularOpeningLocator.Find(
                    gray.Data, gray.Width, gray.Height, camera.EffectivePixelsPerMM,
                    profile.TargetImageRadius,
                    expectedOpeningDiameterOverrideMm ??
                        DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                            profile.ExpectedDarkOpeningDiameterMm,
                            profile.DarkOpeningTunedPixelsPerMM,
                            camera.EffectivePixelsPerMM), rejection);
                LatestNozzleRejection = opening == null
                    ? gray == null ? "INPUT IS NOT GRAYSCALE" :
                        rejection.PrimaryReason + " / " + rejection.ShortSummary
                    : rejection.TinyRingGapRepairUsed
                        ? "LOCKED OPENING: TINY RING GAP REPAIRED / RAW RING VERIFIED"
                        : null;
                circles = opening == null ? Array.Empty<CircleF>() :
                    new[] { new CircleF(new System.Drawing.PointF(
                        (float)opening.X, (float)opening.Y), (float)opening.RadiusPixels) };
            }
            else
            {
                // Keep the existing Hough behavior for machine, board and feeder profiles.
                circles = CvInvoke.HoughCircles(input.Image, HoughModes.Gradient,
                    profile.HoughCirclesDP, profile.HoughCirclesMinDistance * camera.EffectivePixelsPerMM,
                    profile.HoughCirclesParam1, profile.HoughCirclesParam2,
                    Convert.ToInt32(profile.HoughCirclesMinRadius * camera.EffectivePixelsPerMM),
                    Convert.ToInt32(profile.HoughCirclesMaxRadius * camera.EffectivePixelsPerMM));
            }

            var candidates = circles.Where(circle => search.WithinRadius(circle));

            foreach (var circle in candidates)
            {
                // Match by physical center, not fluctuating Hough radius.
                // Otherwise one hole appears as two overlapping tracks for
                // a couple of frames despite Hough minDist working correctly.
                var previous = _foundCircles.FindPrevious(circle,
                    stagingPlateHole || calibrationDisc ? 20 : 10);
                if (previous != null)
                {
                    previous.Iteration = _iteration;
                    previous.Add(circle.Center, circle.Radius);
                    _locatorViewModel.CircleLocated(previous);
                }
                else if (!(stagingPlateHole || calibrationDisc) || _foundCircles.Count == 0)
                {
                    var foundCircle = new MVLocatedCircle(
                        camera.CameraType.Value, center, camera.EffectivePixelsPerMM,
                        profile.ErrorToleranceMM, profile.StabilizationCount,
                        stagingPlateHole || calibrationDisc,
                        positionStabilityToleranceMm: adaptiveNozzle ? 0.20 : 0.10);
                    foundCircle.Add(circle.Center, circle.Radius);
                    foundCircle.Iteration = _iteration;
                    _foundCircles.Add(foundCircle);
                    _locatorViewModel.CircleLocated(foundCircle);
                }
            }

            // Adaptive nozzle openings may be only 7-8 pixels across at the
            // calibrated camera scale. A few missed frames must not discard
            // a valid history before the next fresh observation; missing
            // frames NEVER count as successful measurements. Runout resets
            // all histories for each new angular sample.
            var maxMissedFrames = adaptiveNozzle ? 6 : 2;
            var staleRects = FoundCircles.Where(itr =>
                itr.Iteration < _iteration - maxMissedFrames).ToList();
            foreach (var rect in staleRects)
            {
                FoundCircles.Remove(rect);
            }
        }

        private CircleF? FindStagingPlateHole(IMVImage<IInputOutputArray> input, MachineCamera camera,
            VisionProfile profile, CircleF search, LagoVista.Core.Models.Drawing.Point2D<int> frameCenter,
            bool requireCircularity, out string diagnostics)
        {
            var pixelsPerMM = camera.EffectivePixelsPerMM;
            var minimumRadius = profile.HoughCirclesMinRadius * pixelsPerMM;
            var maximumRadius = profile.HoughCirclesMaxRadius * pixelsPerMM;
            var previous = _foundCircles.FirstOrDefault();
            var expectedX = previous == null ? frameCenter.X : previous.CenterPixels.X;
            var expectedY = previous == null ? frameCenter.Y : previous.CenterPixels.Y;

            using (var smoothed = new Mat())
            using (var binary = new Mat())
            using (var contours = new VectorOfVectorOfPoint())
            {
                // The white-disc boundary is pixel-jagged on actual camera
                // frames. At 12.5 px/mm, even a visually round disc can score
                // below our strict circularity gate with raw Otsu contours.
                // Smooth only CalibrationDisc before Otsu (NOT staging holes,
                // nozzle runout, or any other shape detector). The displayed
                // CalibrationDisc preview uses the identical 5x5 pipeline.
                if (requireCircularity)
                    CvInvoke.GaussianBlur(input.Image, smoothed,
                        new System.Drawing.Size(5, 5), 0);
                var otsu = requireCircularity
                    ? CvInvoke.Threshold(smoothed, binary, 0, 255,
                        ThresholdType.Binary | ThresholdType.Otsu)
                    : CvInvoke.Threshold(input.Image, binary, 0, 255,
                        ThresholdType.Binary | ThresholdType.Otsu);
                CvInvoke.FindContours(binary, contours, null, RetrType.External,
                    ChainApproxMethod.ChainApproxSimple);

                var sizeAccepted = 0;
                var roundAccepted = 0;
                var aspectAccepted = 0;
                var roiAccepted = 0;
                // Diagnostic only; retain at most four physical-radius matches.
                // A contour may be bright and the right size but have a low
                // roundness score because the dark nozzle occludes its edge.
                var radiusMatchDetails = new System.Collections.Generic.List<string>();
                double nearestOutsideMm = double.MaxValue;
                CircleF? best = null;
                var bestDistanceSquared = double.MaxValue;
                for (var i = 0; i < contours.Size; i++)
                {
                    using (var contour = contours[i])
                    using (var moments = CvInvoke.Moments(contour))
                    {
                        if (moments.M00 <= 0)
                            continue;
                        var radius = Math.Sqrt(moments.M00 / Math.PI);
                        if (radius < minimumRadius || radius > maximumRadius)
                            continue;
                        sizeAccepted++;

                        if (requireCircularity)
                        {
                            var perimeter = CvInvoke.ArcLength(contour, true);
                            var circularity = perimeter > 0
                                ? 4 * Math.PI * moments.M00 / (perimeter * perimeter)
                                : 0;
                            var bounds = CvInvoke.BoundingRectangle(contour);
                            var aspect = bounds.Height > 0
                                ? (double)bounds.Width / bounds.Height : 0;
                            if (radiusMatchDetails.Count < 4)
                            {
                                var cx = moments.M10 / moments.M00;
                                var cy = moments.M01 / moments.M00;
                                var centerDistanceMm = Math.Sqrt(
                                    (cx - search.Center.X) * (cx - search.Center.X) +
                                    (cy - search.Center.Y) * (cy - search.Center.Y)) / pixelsPerMM;
                                radiusMatchDetails.Add(
                                    $"D={2 * radius / pixelsPerMM:F2}mm " +
                                    $"round={circularity:F3} (min .780) " +
                                    $"aspect={aspect:F3} (0.85-1.15) " +
                                    $"center=({cx:F0},{cy:F0})px " +
                                    $"dist={centerDistanceMm:F2}mm");
                            }
                            if (perimeter <= 0 || circularity < 0.78)
                                continue;
                            roundAccepted++;
                            if (aspect < 0.85 || aspect > 1.15)
                                continue;
                            aspectAccepted++;
                        }
                        else
                        {
                            roundAccepted++;
                            aspectAccepted++;
                        }

                        var candidate = new CircleF(
                            new System.Drawing.PointF((float)(moments.M10 / moments.M00),
                                (float)(moments.M01 / moments.M00)), (float)radius);
                        if (!search.WithinRadius(candidate))
                        {
                            var deltaX = candidate.Center.X - search.Center.X;
                            var deltaY = candidate.Center.Y - search.Center.Y;
                            nearestOutsideMm = Math.Min(nearestOutsideMm,
                                Math.Sqrt(deltaX * deltaX + deltaY * deltaY) / pixelsPerMM);
                            continue;
                        }
                        roiAccepted++;

                        var dx = candidate.Center.X - expectedX;
                        var dy = candidate.Center.Y - expectedY;
                        var distanceSquared = dx * dx + dy * dy;
                        if (distanceSquared < bestDistanceSquared)
                        {
                            bestDistanceSquared = distanceSquared;
                            best = candidate;
                        }
                    }
                }

                var outside = nearestOutsideMm < double.MaxValue
                    ? $"; nearest excluded center {nearestOutsideMm:F1} mm from image center"
                    : String.Empty;
                diagnostics = $"segmentation={(requireCircularity ? "Gaussian5+Otsu" : "Otsu")}; " +
                    $"Otsu={otsu:F0}; bright contours={contours.Size}; " +
                    $"radius-pass={sizeAccepted}, circularity-pass={roundAccepted}, " +
                    $"aspect-pass={aspectAccepted}, search-pass={roiAccepted}; " +
                    $"diameter range {2 * profile.HoughCirclesMinRadius:F1}-" +
                    $"{2 * profile.HoughCirclesMaxRadius:F1} mm, center ROI " +
                    $"{profile.TargetImageRadius:F1} mm, scale {pixelsPerMM:F1} px/mm" +
                    outside +
                    (radiusMatchDetails.Count > 0
                        ? "; size-matched contours: " +
                          String.Join(" | ", radiusMatchDetails)
                        : "; no size-matched contour") +
                    (best.HasValue ? "; centroid accepted" : "; no accepted centroid");
                return best;
            }
        }

        public MVLocatedCircle FoundCircle { get; private set; }

        public ObservableCollection<MVLocatedCircle> FoundCircles => _foundCircles;

        public void Reset()
        {
            _foundCircles.Clear();
            FoundCircle = null;
            LatestNozzleRejection = null;
            LatestCalibrationDiscDiagnostic = "No calibration-disc frame processed since reset.";
        }
    }
}
 
