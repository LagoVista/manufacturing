// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: bea698690c50f27634f2629c2f2cd8deb142d3e27327fdbb54a31efd0d0a15ef
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces;
using System.Collections.ObjectModel;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public class CornerDetector : ICornerDetector<IInputOutputArray>
    {
        public ObservableCollection<MVLocatedCorner> _locatedCorners = new ObservableCollection<MVLocatedCorner>();
        private readonly ILocatorViewModel _locatorViewModel;

        private int _iteration;

        public CornerDetector(ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
        }

        public void FindCorners(IMVImage<IInputOutputArray> blurredGray, MachineCamera camera, System.Drawing.Size size)
        {
            _iteration++;

            var profile = camera.CurrentVisionProfile;

            var scaledTarget = Convert.ToInt32(profile.TargetImageRadius * camera.CurrentVisionProfile.PixelsPerMM);

            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            using (var cornerDest = new Image<Gray, float>(size))
            using (var matNormalized = new Image<Gray, float>(size))
            using (var matScaled = new Image<Gray, float>(size))
            {
                cornerDest.SetZero();

                int max = -1;
                int x = -1, y = -1;

                CvInvoke.CornerHarris(blurredGray.Image, cornerDest, profile.HarrisCornerBlockSize, profile.HarrisCornerAperture, profile.HarrisCornerK, BorderType.Default);

                CvInvoke.Normalize(cornerDest, matNormalized, 0, 255, NormType.MinMax, DepthType.Cv32F);
                CvInvoke.ConvertScaleAbs(matNormalized, matScaled, 10, 5);

                var minX = size.Width / 2 - scaledTarget;
                var maxX = size.Width / 2 + scaledTarget;
                var minY = size.Height / 2 - scaledTarget;
                var maxY = size.Height / 2 + scaledTarget;

                /* Go through all the returned points and find the one with the highest intensity.  This will be our corner */
                for (int j = minX; j < maxX; j++)
                {
                    for (int i = minY; i < maxY; i++)
                    {
                        var value = (int)matNormalized.Data[i, j, 0];
                        if (value > max)
                        {
                            x = j;
                            y = i;
                            max = value;
                        }
                    }
                }

                var offset = new Point2D<double>(center.X - x, center.Y - y);
                var locatedCorner = new MVLocatedCorner()
                {
                    Camera = camera.CameraType.Value,
                    FoundCount = 1                    
                };

                _locatorViewModel.CornerLocated(locatedCorner);
            }
        }

        public ObservableCollection<MVLocatedCorner> FoundCorners { get => _locatedCorners; }
    
        public void Reset()
        {
            _locatedCorners.Clear();
        }
    }
}
