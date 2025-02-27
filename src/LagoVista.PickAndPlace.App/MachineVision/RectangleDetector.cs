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
using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Linq;
using System.Diagnostics;
using LagoVista.UserAdmin.Models.Contacts;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    internal class RectangleDetector : IRectangleDetector<IInputOutputArray>
    {
        private readonly ILocatorViewModel _locatorViewModel;
        private UMat _edges;

        private int _iteration = 0;
        private ObservableCollection<MVLocatedRectangle> _foundRectangles = new ObservableCollection<MVLocatedRectangle>();

        public RectangleDetector(ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel;
            _edges = new UMat();
        }

        public void FindRectangles(IMVImage<IInputOutputArray> input, MachineCamera camera, System.Drawing.Size size)
        {
            _iteration++;

            var profile = camera.CurrentVisionProfile;

            var bb = CvInvoke.BoundingRectangle(input.Image);

            

            if (profile.UseCannyEdgeDetection)
            {
                CvInvoke.Canny(input.Image, _edges, profile.CannyLowThreshold, profile.CannyHighThreshold, profile.CannyApetureSize, profile.CannyGradient);
            }
            else
            {
                CvInvoke.Threshold(input.Image, _edges, profile.ThresholdEdgeDetection, 255, ThresholdType.Binary);
            }

           
            using (var contours = new VectorOfVectorOfPoint())
            {
                var center = size.Center();
                var scaledTarget = Convert.ToInt32(profile.TargetImageRadius * camera.CurrentVisionProfile.PixelsPerMM);
                var searchBounds = new CircleF(new System.Drawing.PointF(center.X, center.Y), scaledTarget);

                var retrType = RetrType.List;

                switch (profile.ContourRetrieveMode.Value)
                {
                    case ContourRetrieveModes.List: retrType = RetrType.List; break;
                    case ContourRetrieveModes.External: retrType = RetrType.External; break;
                    case ContourRetrieveModes.Tree: retrType = RetrType.Tree; break;
                    case ContourRetrieveModes.FloodFill: retrType = RetrType.Floodfill; break;
                    case ContourRetrieveModes.TwoLevelHierarchy: retrType = RetrType.Ccomp; break;
                }

                CvInvoke.FindContours(_edges, contours, null, retrType, ChainApproxMethod.ChainApproxSimple);
                int count = contours.Size;
           
                for (int i = 0; i < count; i++)
                {
                    using (var contour = contours[i])
                    using (var approxContour = new VectorOfPoint())
                    {
                        //var epsilon = CvInvoke.ArcLength(contour, true) * profile.PolygonEpsilonFactor;

                        //var mar = CvInvoke.MinAreaRect(contour);

                        //CvInvoke.ApproxPolyDP(contour, approxContour, epsilon, profile.ContourFindOnlyClosed);
                        var area = CvInvoke.ContourArea(contour, false);
                        if(area < profile.ContourMinArea)
                        {
                           
                        }
                        else if (area > profile.CountourMaxArea)
                        {
                           
                        }
                        else
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
                                    var rect = CvInvoke.MinAreaRect(contour);
                                    if (searchBounds.WithinRadius(rect))
                                    {
                                        if ((rect.Size.Width > rect.Size.Height && profile.FindLandScape) || ( rect.Size.Height > rect.Size.Width && profile.FindPortrait))
                                        {
                                            var previous = _foundRectangles.FindPrevious(rect, 20);
                                            if (previous != null)
                                            {
                                                previous.Iteration = _iteration;
                                                previous.Add(rect);
                                                _locatorViewModel.RectLocated(previous);
                                            }
                                            else
                                            {
                                                var foundRect = new MVLocatedRectangle(camera.CameraType.Value, center, profile.PixelsPerMM, profile.ErrorToleranceMM, profile.StabilizationCount);
                                                foundRect.Iteration = _iteration;
                                                foundRect.Add(rect);
                                                _locatorViewModel.RectLocated(foundRect);
                                                FoundRectangles.Add(foundRect);
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
            }

            // Get a list of stale ones, run the query so they can be removed..w/o ToList it will run the query when trying to delete.
            var staleRects = FoundRectangles.Where(itr => itr.Iteration < _iteration -4).ToList();
            foreach (var rect in staleRects)
            {
                _foundRectangles.Remove(rect);
            }
        }

        public ObservableCollection<MVLocatedRectangle> FoundRectangles { get => _foundRectangles; }

        public IInputOutputArray Image => _edges;

        public void Reset()
        {
            FoundRectangles.Clear();
        }
    }
}
