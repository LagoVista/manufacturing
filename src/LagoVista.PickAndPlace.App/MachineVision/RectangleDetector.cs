using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using System.Drawing.Drawing2D;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.Services;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    internal class RectangleDetector : IRectangleDetector<IInputOutputArray>
    {
        private readonly ILocatorViewModel _locatorViewModel;
        private UMat _edges;

        private List<MVLocatedRectangle> _foundRectangles = new List<MVLocatedRectangle>();

        public RectangleDetector(ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
            _edges = new UMat();
        }

        public void FindRectangles(IMVImage<IInputOutputArray> input, MachineCamera camera, System.Drawing.Size size)
        {
            var profile = camera.CurrentVisionProfile;

            if (profile.UseCannyEdgeDetection)
            {
                CvInvoke.Canny(input.Image, _edges, profile.CannyLowThreshold, profile.CannyHighThreshold, profile.CannyApetureSize, profile.CannyGradient);
            }
            else
            {
                CvInvoke.Threshold(input.Image, _edges, profile.ThresholdEdgeDetection, 255, ThresholdType.Binary);
            }

            var currentRects = new List<MVLocatedRectangle>();

            using (var contours = new VectorOfVectorOfPoint())
            {
                var center = size.Center();
                var scaledTarget = Convert.ToInt32(profile.TargetImageRadius * camera.CurrentVisionProfile.PixelsPerMM);
                var searchBounds = new CircleF(new System.Drawing.PointF(center.X, center.Y), scaledTarget);

                CvInvoke.FindContours(_edges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxNone);
                int count = contours.Size;
                for (int i = 0; i < count; i++)
                {
                    using (var contour = contours[i])
                    using (var approxContour = new VectorOfPoint())
                    {
                        CvInvoke.ApproxPolyDP(contour, approxContour, CvInvoke.ArcLength(contour, true) * profile.PolygonEpsilonFactor, profile.ContourFindOnlyClosed);
                        var area = CvInvoke.ContourArea(approxContour, false);
                        if (area > profile.ContourMinArea && area < profile.CountourMaxArea) //only consider contours with area greater than 250
                        {
                            var pts = approxContour.ToArray();

                            if (approxContour.Size == 4 || true) //The contour has 4 vertices.
                            {
                                bool isRectangle = true;
                                var rectEdges = PointCollection.PolyLine(pts, true);

                                if (!profile.FindIrregularPolygons)
                                {
                                    for (var j = 0; j < rectEdges.Length; j++)
                                    {
                                        var angle = Math.Abs(rectEdges[(j + 1) % rectEdges.Length].GetExteriorAngleDegree(rectEdges[j]));
                                        if (angle < 80 || angle > 100)
                                        {
                                            isRectangle = false;
                                            break;
                                        }
                                    }
                                }

                                if (isRectangle || true)
                                {
                                    var rect = CvInvoke.MinAreaRect(approxContour);
                                    if (searchBounds.WithinRadius(rect) )
                                    {
                                        if (rect.Size.Width > rect.Size.Height && profile.FindLandScape || rect.Size.Height > rect.Size.Width && profile.FindPortrait)
                                        {
                                            var previous = _foundRectangles.FindPrevious(rect, 10);
                                            if (previous != null)
                                            {
                                                previous.Add(rect);
                                                _locatorViewModel.RectLocated(previous);
                                                currentRects.Add(previous);
                                            }
                                            else
                                            {
                                                var foundRect = new MVLocatedRectangle(camera.CameraType.Value, center, profile.PixelsPerMM, profile.ErrorToleranceMM, profile.StabilizationCount);
                                                foundRect.Add(rect);
                                                _locatorViewModel.RectLocated(foundRect);
                                                currentRects.Add(foundRect);
                                            }

                                        }
                                    }
                                }
                            }
                            else if (profile.FindIrregularPolygons)
                            {
                                var rectEdges = PointCollection.PolyLine(pts, true);
                                //for (var idx = 0; idx < rectEdges.Length - 1; ++idx)
                                //{
                                //    CvInvoke.Line(output, rectEdges[idx].P1, rectEdges[idx].P2, new Bgr(System.Drawing.Color.LightBlue).MCvScalar);
                                //}

                            }
                        }
                    }
                }

                _foundRectangles.Clear();
                _foundRectangles.AddRange(currentRects);
            }
        }

        public List<MVLocatedRectangle> FoundRectangles { get => _foundRectangles; }

        public IInputOutputArray Image => _edges;

        public void Reset()
        {
            FoundRectangles.Clear();
        }
    }
}
