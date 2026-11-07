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

            var circles = CvInvoke.HoughCircles(input.Image, HoughModes.Gradient, profile.HoughCirclesDP, profile.HoughCirclesMinDistance * camera.EffectivePixelsPerMM, profile.HoughCirclesParam1, profile.HoughCirclesParam2,
                Convert.ToInt32(profile.HoughCirclesMinRadius * camera.EffectivePixelsPerMM), Convert.ToInt32(profile.HoughCirclesMaxRadius * camera.EffectivePixelsPerMM));

            var stagingPlateHole = profile.Key == VisionProfile.VisionProfile_StagingPlateHole;
            var candidates = circles.Where(circle => search.WithinRadius(circle));

            if (stagingPlateHole)
            {
                // The staging-plate profile expects one hole. Keep one continuous track
                // instead of switching between multiple Hough candidates each frame.
                var current = _foundCircles.FirstOrDefault();
                var expectedX = current == null ? center.X : current.CenterPixels.X;
                var expectedY = current == null ? center.Y : current.CenterPixels.Y;
                candidates = candidates
                    .OrderBy(circle => Math.Pow(circle.Center.X - expectedX, 2) +
                                       Math.Pow(circle.Center.Y - expectedY, 2))
                    .Take(1);
            }

            foreach (var circle in candidates)
            {
                // Give the same staging-plate hole room for small Hough center jitter.
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

        public MVLocatedCircle FoundCircle { get; private set; }

        public ObservableCollection<MVLocatedCircle> FoundCircles => _foundCircles;

        public void Reset()
        {
            _foundCircles.Clear();
            FoundCircle = null;
        }
    }
}
 
