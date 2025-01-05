using Emgu.CV.CvEnum;
using Emgu.CV;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces;
using Emgu.CV.Structure;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public class CircleDetector : ICircleDetector<IInputOutputArray>
    {
        private List<MVLocatedCircle> _foundCircles = new List<MVLocatedCircle>();
        private readonly ILocatorViewModel _locatorViewModel;

        public CircleDetector(ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
        }

        public void FindCircles(IMVImage<IInputOutputArray> input, MachineCamera camera, System.Drawing.Size size)
        {
            var profile = camera.CurrentVisionProfile;
            var center = size.Center();

            var scaledTarget = Convert.ToInt32(profile.TargetImageRadius * camera.CurrentVisionProfile.PixelsPerMM);
            var search = new CircleF(new System.Drawing.PointF(center.X, center.Y), scaledTarget);

            var circles = CvInvoke.HoughCircles(input.Image, HoughModes.Gradient, profile.HoughCirclesDP, profile.HoughCirclesMinDistance, profile.HoughCirclesParam1, profile.HoughCirclesParam2,
                Convert.ToInt32(profile.HoughCirclesMinRadius * camera.CurrentVisionProfile.PixelsPerMM), Convert.ToInt32(profile.HoughCirclesMaxRadius * camera.CurrentVisionProfile.PixelsPerMM));

            var currentFoundCircles = new List<MVLocatedCircle>();

            foreach (var circle in circles)
            {
                if (search.WithinRadius(circle))
                {
                    var previous = _foundCircles.FindPrevious(circle, 10);
                    if (previous != null)
                    {
                        previous.Add(circle.Center, circle.Radius);
                        _locatorViewModel.CircleLocated(previous);
                        currentFoundCircles.Add(previous);
                    }
                    else
                    {
                        var foundCircle = new MVLocatedCircle(camera.CameraType.Value, center, profile.PixelsPerMM, profile.ErrorToleranceMM, profile.StabilizationCount);
                        foundCircle.Add(circle.Center, circle.Radius);
                        currentFoundCircles.Add(foundCircle);
                        _locatorViewModel.CircleLocated(foundCircle);
                    }
                }
            }

            _foundCircles.Clear();
            _foundCircles.AddRange(currentFoundCircles);
        }

        public MVLocatedCircle FoundCircle { get; private set; }

        public List<MVLocatedCircle> FoundCircles => _foundCircles;

        public void Reset()
        {
            _foundCircles.Clear();
            FoundCircle = null;
        }
    }
}
 
