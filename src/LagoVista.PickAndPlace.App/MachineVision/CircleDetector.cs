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

        public CircleDetector(ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
        }

        public void FindCircles(IMVImage<IInputOutputArray> input, MachineCamera camera, System.Drawing.Size size)
        {
            _iteration++;

            var profile = camera.CurrentVisionProfile;
            var center = size.Center();

            var scaledTarget = Convert.ToInt32(profile.TargetImageRadius * camera.EffectivePixelsPerMM);
            var search = new CircleF(new System.Drawing.PointF(center.X, center.Y), scaledTarget);

            var stagingPlateHole = profile.Key == VisionProfile.VisionProfile_StagingPlateHole;
            CircleF[] circles;
            if (stagingPlateHole)
            {
                // The illuminated hole is a high-contrast filled region. Its area centroid
                // is more repeatable than a Hough center on this staging-plate image.
                var hole = FindStagingPlateHole(input, camera, profile, search, center);
                circles = hole.HasValue ? new[] { hole.Value } : Array.Empty<CircleF>();
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
                // Match the current track while tolerating small center and radius changes.
                // Large jumps must establish a new track after the stale one expires.
                var previous = _foundCircles.FindPrevious(circle, stagingPlateHole ? 20 : 10);
                if (previous != null)
                {
                    previous.Iteration = _iteration;
                    previous.Add(circle.Center, circle.Radius);
                    _locatorViewModel.CircleLocated(previous);
                }
                else if (!stagingPlateHole || _foundCircles.Count == 0)
                {
                    var foundCircle = new MVLocatedCircle(
                        camera.CameraType.Value, center, camera.EffectivePixelsPerMM,
                        profile.ErrorToleranceMM, profile.StabilizationCount, stagingPlateHole);
                    foundCircle.Add(circle.Center, circle.Radius);
                    foundCircle.Iteration = _iteration;
                    _foundCircles.Add(foundCircle);
                    _locatorViewModel.CircleLocated(foundCircle);
                }
            }

            var staleRects = FoundCircles.Where(itr => itr.Iteration < _iteration - 2).ToList();
            foreach (var rect in staleRects)
            {
                FoundCircles.Remove(rect);
            }
        }

        private CircleF? FindStagingPlateHole(IMVImage<IInputOutputArray> input, MachineCamera camera,
            VisionProfile profile, CircleF search, LagoVista.Core.Models.Drawing.Point2D<int> frameCenter)
        {
            var pixelsPerMM = camera.EffectivePixelsPerMM;
            var minimumRadius = profile.HoughCirclesMinRadius * pixelsPerMM;
            var maximumRadius = profile.HoughCirclesMaxRadius * pixelsPerMM;
            var previous = _foundCircles.FirstOrDefault();
            var expectedX = previous == null ? frameCenter.X : previous.CenterPixels.X;
            var expectedY = previous == null ? frameCenter.Y : previous.CenterPixels.Y;

            using (var binary = new Mat())
            using (var contours = new VectorOfVectorOfPoint())
            {
                // Use the processed grayscale image, with Otsu handling both gray and
                // already-thresholded staging-plate profiles. Never modify the input.
                CvInvoke.Threshold(input.Image, binary, 0, 255,
                    ThresholdType.Binary | ThresholdType.Otsu);
                // Staging-plate holes can be light-on-dark or dark-on-light.
                // Retrieve nested contours as well: External discards a dark hole
                // inside the bright circular area left by Apply Mask.
                CvInvoke.FindContours(binary, contours, null, RetrType.List,
                    ChainApproxMethod.ChainApproxSimple);

                CircleF? best = null;
                var bestDistanceSquared = double.MaxValue;
                for (var i = 0; i < contours.Size; i++)
                {
                    using (var contour = contours[i])
                    {
                        // CvInvoke.Moments returns MCvMoments (a value type).
                        var moments = CvInvoke.Moments(contour);
                        if (moments.M00 <= 0)
                            continue;

                            var radius = Math.Sqrt(moments.M00 / Math.PI);
                            if (radius < minimumRadius || radius > maximumRadius)
                                continue;

                            var candidate = new CircleF(
                                new System.Drawing.PointF((float)(moments.M10 / moments.M00),
                                    (float)(moments.M01 / moments.M00)), (float)radius);
                            if (!search.WithinRadius(candidate))
                                continue;

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

                return best;
            }
        }

        public MVLocatedCircle FoundCircle { get; private set; }

        public ObservableCollection<MVLocatedCircle> FoundCircles => _foundCircles;

        public void Reset()
        {
            _foundCircles.Clear();
            FoundCircle = null;
        }
    }
}
 
