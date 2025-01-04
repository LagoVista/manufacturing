using Emgu.CV.CvEnum;
using Emgu.CV;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces;

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

        private MVLocatedCircle FindPrevious(Point2D<int> center, int radius)
        {
            var deltaPixel = 10;
            return _foundCircles.FirstOrDefault(cir => Math.Max(Math.Abs(cir.CenterPixels.X - center.X), Math.Abs(cir.CenterPixels.Y - center.Y)) < deltaPixel
                        && cir.RadiusPixels - deltaPixel < radius && cir.RadiusPixels + deltaPixel > radius);
        }

        public void FindCircles(IMVImage<IInputOutputArray> input, MachineCamera camera, System.Drawing.Size size)
        {
            var profile = camera.CurrentVisionProfile;
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            var scaledTarget = Convert.ToInt32(profile.TargetImageRadius * camera.PixelsPerMM);

            var circles = CvInvoke.HoughCircles(input.Image, HoughModes.Gradient, profile.HoughCirclesDP, profile.HoughCirclesMinDistance, profile.HoughCirclesParam1, profile.HoughCirclesParam2, Convert.ToInt32(profile.HoughCirclesMinRadius * camera.PixelsPerMM), Convert.ToInt32(profile.HoughCirclesMaxRadius * camera.PixelsPerMM));

            var currentFoundCircles = new List<MVLocatedCircle>();

            foreach (var circle in circles)
            {
                if (circle.Center.X > size.Width / 2 - scaledTarget && circle.Center.X < size.Width / 2 + scaledTarget &&
                   circle.Center.Y > size.Height / 2 - scaledTarget && circle.Center.Y < size.Height / 2 + scaledTarget)
                {
                    var foundCircle = new MVLocatedCircle()
                    {
                        CenterPixels = new Point2D<int>(Convert.ToInt32(circle.Center.X), Convert.ToInt32(circle.Center.Y)),
                        RadiusPixels = Convert.ToInt32(circle.Radius),
                        RadiusMM = Math.Round(circle.Radius / camera.PixelsPerMM, 2),
                        OffsetPixels = new Point2D<int>(Convert.ToInt32(circle.Center.X - center.X), Convert.ToInt32(circle.Center.Y - center.X)),
                        FoundCount = 1
                    };

                    foundCircle.OffsetMM = new Point2D<double>(Math.Round(foundCircle.OffsetPixels.X / camera.PixelsPerMM, 2), Math.Round(foundCircle.OffsetPixels.Y / camera.PixelsPerMM, 2));

                    var previous = FindPrevious(foundCircle.CenterPixels, foundCircle.RadiusPixels);
                    if (previous != null)
                    {
                        previous.FoundCount++;
                        currentFoundCircles.Add(previous);
                        foundCircle = previous;
                    }
                    else
                    {
                        currentFoundCircles.Add(foundCircle);
                    }

                    /* If within one pixel of center, state we have a match */
                    //TODO Replace this with a profile setting for image accuracy, points per mm and mm
                    if (Math.Abs(foundCircle.OffsetMM.X) < profile.ErrorToleranceMM && Math.Abs(foundCircle.OffsetMM.Y) < profile.ErrorToleranceMM)
                    {
                        if (true) //foundCircle.StandardDeviation.X < 0.7 && foundCircle.StandardDeviation.Y < 0.7 && _stabilizedPointCount++ > 5)
                        {
                            foundCircle.Centered = true;
                            _locatorViewModel.CircleLocated(foundCircle);

                        }
                        else
                            foundCircle.Centered = false;

                    }
                    else
                    {
                        foundCircle.Centered = false;
                        _locatorViewModel.CircleLocated(foundCircle);
                    }

                    _foundCircles.Clear();
                    _foundCircles.AddRange(currentFoundCircles);
                }
            }
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
