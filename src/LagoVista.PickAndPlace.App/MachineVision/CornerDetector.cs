using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public class CornerDetector : ICornerDetector<IInputOutputArray>
    {
        private readonly ObservableCollection<MVLocatedCorner> _locatedCorners =
            new ObservableCollection<MVLocatedCorner>();
        private readonly ILocatorViewModel _locatorViewModel;
        private int _iteration;
        private string _latestStatus = String.Empty;
        public string LatestStatus => _latestStatus;

        public CornerDetector(ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
        }

        public void FindCorners(IMVImage<IInputOutputArray> grayscale,
            MachineCamera camera, System.Drawing.Size size)
        {
            _iteration++;
            var profile = camera.CurrentVisionProfile;
            var nominalScale = camera.EffectivePixelsPerMM;
            if (nominalScale <= 0 || double.IsNaN(nominalScale) ||
                double.IsInfinity(nominalScale))
            {
                Reset();
                return;
            }

            if (profile.Key == VisionProfile.VisionProfile_FeederOrigin)
            {
                // This profile means the upper-left WHITE cut corner, not the
                // black START bar. Use its existing threshold/search-radius
                // controls. Image is the raw grayscale camera frame after
                // the configured camera mirrors have been applied.
                var threshold = profile.ApplyThreshold
                    ? (byte)Math.Round(profile.PrimaryThreshold / 100.0 * 255)
                    : (byte)128;
                using (var binary = new Image<Gray, byte>(size))
                {
                    CvInvoke.Threshold(grayscale.Image, binary, threshold,
                        255, ThresholdType.Binary);

                    var radius = Math.Max(20,
                        (int)Math.Round(profile.TargetImageRadius * nominalScale));
                    var white = FeederWhiteCornerFinder.Find(size.Width, size.Height,
                        (x, y) => binary.Data[y, x, 0], 128, radius);
                    if (!white.Found)
                    {
                        Reset();
                        _latestStatus = white.Status;
                        return;
                    }
                    _latestStatus = white.Status;

                    // Prefer the known 2.5 mm start reference when the camera
                    // currently sees enough barcode bars to verify it. Near
                    // the cut corner it may not, in which case retain the
                    // position camera's calibrated scale and report that fact.
                    var measuredScale = FeederWhiteCornerFinder.MeasureStartScale(
                        size.Width, size.Height, (x, y) => binary.Data[y, x, 0],
                        128, white, nominalScale);
                    var useStartScale = measuredScale > 0;
                    Track(white.X, white.Y, camera, size,
                        useStartScale ? measuredScale : nominalScale,
                        useStartScale, white);
                }
                return;
            }

            // Preserve the normal Harris detector for non-feeder profiles.
            using (var corners = new Image<Gray, float>(size))
            using (var normalized = new Image<Gray, float>(size))
            {
                CvInvoke.CornerHarris(grayscale.Image, corners,
                    profile.HarrisCornerBlockSize, profile.HarrisCornerAperture,
                    profile.HarrisCornerK, BorderType.Default);
                CvInvoke.Normalize(corners, normalized, 0, 255,
                    NormType.MinMax, DepthType.Cv32F);

                var r = Math.Max(8, (int)Math.Round(
                    profile.TargetImageRadius * nominalScale));
                var minX = Math.Max(0, size.Width / 2 - r);
                var maxX = Math.Min(size.Width, size.Width / 2 + r + 1);
                var minY = Math.Max(0, size.Height / 2 - r);
                var maxY = Math.Min(size.Height, size.Height / 2 + r + 1);
                var best = -1.0;
                var foundX = -1;
                var foundY = -1;

                for (var y = minY; y < maxY; y++)
                {
                    for (var x = minX; x < maxX; x++)
                    {
                        var response = normalized.Data[y, x, 0];
                        if (response > best)
                        {
                            best = response;
                            foundX = x;
                            foundY = y;
                        }
                    }
                }

                if (foundX < 0 || best < profile.HarrisCornerThreshold)
                {
                    Reset();
                    return;
                }

                Track(foundX, foundY, camera, size, nominalScale, false);
            }
        }

        private void Track(int x, int y, MachineCamera camera,
            System.Drawing.Size size, double scale, bool usesStartScale,
            FeederWhiteCornerObservation fittedEdges = null)
        {
            var tracked = _locatedCorners.FirstOrDefault();
            if (tracked == null)
            {
                tracked = new MVLocatedCorner { Camera = camera.CameraType.Value };
                _locatedCorners.Add(tracked);
            }

            tracked.Iteration = _iteration;
            tracked.Add(x, y, size.Width, size.Height, scale,
                camera.CurrentVisionProfile.ErrorToleranceMM,
                camera.CurrentVisionProfile.StabilizationCount, usesStartScale);
            tracked.SetFittedEdges(fittedEdges);
            _locatorViewModel.CornerLocated(tracked);
        }

        public ObservableCollection<MVLocatedCorner> FoundCorners => _locatedCorners;

        public void Reset()
        {
            _latestStatus = String.Empty;
            foreach (var corner in _locatedCorners)
                corner.Reset();
            _locatedCorners.Clear();
        }
    }
}
