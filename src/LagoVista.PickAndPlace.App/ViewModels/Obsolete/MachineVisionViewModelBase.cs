﻿
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Util;
using LagoVista.PickAndPlace.ViewModels.Obsolete;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public abstract partial class MachineVisionViewModelBase : GCodeAppViewModelBase
    {
        DoubleMedianFilter _cornerMedianFilter = new DoubleMedianFilter(4, 1);
        DoubleMedianFilter _circleMedianFilter = new DoubleMedianFilter(4, 1);
        DoubleMedianFilter _circleRadiusMedianFilter = new DoubleMedianFilter(4, 1);


        IMachine _machine;
        ILocatorViewModel _locatorViewModel;
        
        const double PIXEL_PER_MM = 20.0;
        public MachineVisionViewModelBase(IMachine machine, ILocatorViewModel locatorViewModel) : base(machine)
        {
            _machine = machine ?? throw new ArgumentNullException(nameof(machine));
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));

        }

        public override Task InitAsync()
        {
            return Task.CompletedTask;
        }


        public async Task<InvokeResult> SaveMachineAsync()
        {
            var rest = SLWIOC.Get<IRestClient>();
            return await rest.PutAsync("/api/mfg/machine", Machine.Settings);
        }

        protected void Circle(IInputOutputArray img, int x, int y, int radius, System.Drawing.Color color, int thickness = 1)
        {
            if (!ShowOriginalImage)
            {
                color = System.Drawing.Color.White;
            }

            CvInvoke.Circle(img,
            new System.Drawing.Point(x, y), radius,
            new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);

        }

        protected void Line(IInputOutputArray img, int x1, int y1, int x2, int y2, System.Drawing.Color color, int thickness = 1)
        {
            if (!ShowOriginalImage)
            {
                color = System.Drawing.Color.White;
            }

            CvInvoke.Line(img, new System.Drawing.Point(x1, y1),
                new System.Drawing.Point(x2, y2),
                new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
        }

        private Point2D<double> _foundCorner;
        public Point2D<double> FoundCorner
        {
            get { return _foundCorner; }
            set { Set(ref _foundCorner, value); }
        }

        public virtual void RectLocated(RotatedRect rect, Point2D<double> stdDeviation) { }
        public virtual void RectCentered(RotatedRect rect, Point2D<double> stdDeviation) { }


        public virtual void CornerLocated(Point2D<double> point, Point2D<double> stdDeviation) { }
        public virtual void CornerCentered(Point2D<double> point, Point2D<double> stdDeviation) { }


        public virtual void CircleLocated(Point2D<double> point, double diameter, Point2D<double> stdDeviation) { }
        public virtual void CircleCentered(Point2D<double> point, double diameter) { }

        private void ShowCalibrationSquare(IInputOutputArray destImage, System.Drawing.Size size)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            CvInvoke.Rectangle(destImage, new System.Drawing.Rectangle(center.X - 100, center.Y - 100, 200, 200),
            new Bgr(System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0xFF)).MCvScalar);
        }

        #region Show Cross Hairs
        private void DrawCrossHairs(IInputOutputArray destImage, VisionProfile profile, System.Drawing.Size size)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            //Line(destImage, 0, center.Y, center.X - profile.TargetImageRadius, center.Y, System.Drawing.Color.Yellow);
            //Line(destImage, center.X + profile.TargetImageRadius, center.Y, size.Width, center.Y, System.Drawing.Color.Yellow);

            //Line(destImage, center.X, 0, center.X, center.Y - profile.TargetImageRadius, System.Drawing.Color.Yellow);
            //Line(destImage, center.X, center.Y + profile.TargetImageRadius, center.X, size.Height, System.Drawing.Color.Yellow);

            //Line(destImage, center.X - profile.TargetImageRadius, center.Y, center.X + profile.TargetImageRadius, center.Y, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));
            //Line(destImage, center.X, center.Y - profile.TargetImageRadius, center.X, center.Y + profile.TargetImageRadius, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));

            ////if (_visionProfileManagerViewModel.CurrentMVProfile.Id == "squarepart")
            ////{
            ////    Line(destImage, center.X - PartSizeWidth, center.Y - PartSizeHeight, center.X - PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);
            ////    Line(destImage, center.X + PartSizeWidth, center.Y - PartSizeHeight, center.X + PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);

            ////    Line(destImage, center.X - PartSizeWidth, center.Y + PartSizeHeight, center.X + PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);
            ////    Line(destImage, center.X - PartSizeWidth, center.Y - PartSizeHeight, center.X + PartSizeWidth, center.Y - PartSizeHeight, System.Drawing.Color.Yellow);
            ////}
            ////else
            //{
            //    Circle(destImage, center.X, center.Y, profile.TargetImageRadius, System.Drawing.Color.Yellow);
            //}
        }
        #endregion

        public int PartSizeWidth { get; set; } = 12;
        public int PartSizeHeight { get; set; } = 24;

        protected Point2D<double> RequestedPosition
        {
            get;
            set;
        }


        protected virtual void FoundHomePosition()
        {
            _locatorViewModel.SetLocatorState(MVLocatorState.Idle);
            Machine.SetWorkspaceHome();
        }


        protected void JogToLocation(Point2D<double> offset)
        {
//            var pixelsPerMM = Machine.Settings.PositionCameraPixelsPerMM > 0 ? Machine.Settings.PositionCameraPixelsPerMM : PIXEL_PER_MM;
//            var deltaX = Machine.MachinePosition.X - (offset.X / pixelsPerMM);
//            var deltaY = Machine.MachinePosition.Y + (offset.Y / pixelsPerMM);

////            if (deltaX > 50 || deltaY > 50)
//  //              return;

//            var threshold = Math.Abs(deltaX) > 2 || Math.Abs(deltaY) > 2 ? 1.0f : 1;
//            threshold = 3;

//            if (StandardDeviation.X < threshold && StandardDeviation.Y < threshold)
//            {
//                _stabilizedPointCount++;
//                if (_stabilizedPointCount > 5)
//                {
//                    var offsetX = (offset.X / 20);
//                    var newLocationX = Machine.Settings.PositioningCamera.MirrorXAxis ? Math.Round(Machine.MachinePosition.X + offsetX, 4) : Math.Round(Machine.MachinePosition.X - offsetX, 4);
//                    var newLocationY = Machine.Settings.PositioningCamera.MirrorYAxis ? Math.Round(Machine.MachinePosition.Y + (offset.Y / 20), 4) : Math.Round(Machine.MachinePosition.Y - (offset.Y / 20), 4);
//                    RequestedPosition = new Point2D<double>() { X = newLocationX, Y = newLocationY };

//                    Machine.GotoPoint(RequestedPosition, true);
//                    _stabilizedPointCount = 0;
//                }
//            }
//            else
//            {
//                _stabilizedPointCount = 0;
//            }
        }


        int _stabilizedPointCount = 0;

        #region Find Circles
        private void FindCircles(IInputOutputArray input, IInputOutputArray output, System.Drawing.Size size, VisionProfile profile)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            //var circles = CvInvoke.HoughCircles(input, HoughModes.Gradient, profile.HoughCirclesDP, profile.HoughCirclesMinDistance, profile.HoughCirclesParam1, profile.HoughCirclesParam2, profile.HoughCirclesMinRadius, profile.HoughCirclesMaxRadius);

            //var foundCircle = false;
            ///* Above will return ALL maching circles, we only want the first one that is in the target image radius in the middle of the screen */
            //foreach (var circle in circles)
            //{
            //    if (circle.Center.X > ((size.Width / 2) - profile.TargetImageRadius) && circle.Center.X < ((size.Width / 2) + profile.TargetImageRadius) &&
            //       circle.Center.Y > ((size.Height / 2) - profile.TargetImageRadius) && circle.Center.Y < ((size.Height / 2) + profile.TargetImageRadius))
            //    {
            //        _circleMedianFilter.Add(circle.Center.X, circle.Center.Y);
            //        _circleRadiusMedianFilter.Add(circle.Radius, 0);
            //        foundCircle = true;
            //        break;
            //    }
            //}

            //if (!foundCircle)
            //{
            //    _circleMedianFilter.Add(null);
            //    _circleRadiusMedianFilter.Add(null);
            //}

            //var avg = _circleMedianFilter.Filtered;
            //if (avg != null)
            //{
            //    CircleCenter = new Point2D<double>(Math.Round(avg.X, 2), Math.Round(avg.Y, 2));
            //    StandardDeviation = _circleMedianFilter.StandardDeviation;

            //    var offset = new Point2D<double>(center.X - avg.X, center.Y - avg.Y);

            //    var deltaX = Math.Abs(avg.X - center.X);
            //    var deltaY = Math.Abs(avg.Y - center.Y);
            //    //Debug.WriteLine($"{deltaX}, {deltaY} - {_stabilizedPointCount} - {_circleRadiusMedianFilter.StandardDeviation.X},{_circleRadiusMedianFilter.StandardDeviation.Y}");
            //    /* If within one pixel of center, state we have a match */
            //    if (deltaX < 1.5 && deltaY < 1.5)
            //    {
            //        Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Green);
            //        Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Green);
            //        Circle(output, (int)avg.X, (int)avg.Y, (int)_circleRadiusMedianFilter.Filtered.X, System.Drawing.Color.Green);
            //        if (StandardDeviation.X < 0.7 && StandardDeviation.Y < 0.7)
            //        {
            //            _stabilizedPointCount++;
            //            if (_stabilizedPointCount > 5)
            //            {
            //                CircleCentered(offset, _circleRadiusMedianFilter.Filtered.X);
            //            }
            //        }
            //    }
            //    else
            //    {
            //        Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Red);
            //        Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Red);
            //        Circle(output, (int)avg.X, (int)avg.Y, (int)_circleRadiusMedianFilter.Filtered.X, System.Drawing.Color.Red);
            //        CircleLocated(offset, _circleRadiusMedianFilter.Filtered.X, _circleRadiusMedianFilter.StandardDeviation);
            //    }
            //}
            //else
            //{
            //    CircleCenter = null;
            //}
        }
        #endregion

        #region Find Corners
        private void FindCorners(Image<Gray, byte> blurredGray, IInputOutputArray output, System.Drawing.Size size, VisionProfile profile)
        {
            //var center = new Point2D<int>()
            //{
            //    X = size.Width / 2,
            //    Y = size.Height / 2
            //};

            //using (var cornerDest = new Image<Gray, float>(blurredGray.Size))
            //using (var matNormalized = new Image<Gray, float>(blurredGray.Size))
            //using (var matScaled = new Image<Gray, float>(blurredGray.Size))
            //{
            //    cornerDest.SetZero();

            //    int max = -1;
            //    int x = -1, y = -1;

            //    CvInvoke.CornerHarris(blurredGray, cornerDest, profile.HarrisCornerBlockSize, profile.HarrisCornerAperture, profile.HarrisCornerK, BorderType.Default);

            //    CvInvoke.Normalize(cornerDest, matNormalized, 0, 255, NormType.MinMax, DepthType.Cv32F);
            //    CvInvoke.ConvertScaleAbs(matNormalized, matScaled, 10, 5);

            //    var minX = (size.Width / 2) - profile.TargetImageRadius;
            //    var maxX = (size.Width / 2) + profile.TargetImageRadius;
            //    var minY = (size.Height / 2) - profile.TargetImageRadius;
            //    var maxY = (size.Height / 2) + profile.TargetImageRadius;

            //    /* Go through all the returned points and find the one with the highest intensity.  This will be our corner */
            //    for (int j = minX; j < maxX; j++)
            //    {
            //        for (int i = minY; i < maxY; i++)
            //        {
            //            var value = (int)matNormalized.Data[i, j, 0];
            //            if (value > max)
            //            {
            //                x = j;
            //                y = i;
            //                max = value;
            //            }
            //        }
            //    }

            //    if (x > 0 && y > 0)
            //    {
            //        _cornerMedianFilter.Add(new Point2D<float>(x, y));

            //    }

            //    var avg = _cornerMedianFilter.Filtered;
            //    if (avg != null)
            //    {
            //        Circle(output, (int)avg.X, (int)avg.Y, 5, System.Drawing.Color.Blue);
            //        Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Blue);
            //        Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Blue);

            //        var offset = new Point2D<double>(center.X - avg.X, center.Y - avg.Y);
            //        CornerLocated(offset, _cornerMedianFilter.StandardDeviation);
            //    }
            //}
        }
        #endregion

        DoubleMedianFilter _rectP1 = new DoubleMedianFilter();
        DoubleMedianFilter _rectP2 = new DoubleMedianFilter();
        DoubleMedianFilter _rectP3 = new DoubleMedianFilter();
        DoubleMedianFilter _rectP4 = new DoubleMedianFilter();
        DoubleMedianFilter _rectTheta = new DoubleMedianFilter();

        protected RotatedRect? FoundRectangle { get; set; }

        private void FindRect2(Image<Gray, byte> input, IInputOutputArray output, System.Drawing.Size size)
        {
            var rect = CvInvoke.BoundingRectangle(input);
            if (rect.Width > 0 && rect.Height > 0)
            {
                Line(output, (int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Top, System.Drawing.Color.White);
                Line(output, (int)rect.Right, (int)rect.Top, (int)rect.Right, (int)rect.Bottom, System.Drawing.Color.White);
                Line(output, (int)rect.Right, (int)rect.Bottom, (int)rect.Left, (int)rect.Bottom, System.Drawing.Color.White);
                Line(output, (int)rect.Left, (int)rect.Bottom, (int)rect.Left, (int)rect.Top, System.Drawing.Color.White);

                //var msg = $"{rect.Angle:0.0} {(rect.Sz.W / Machine.Settings.InspectionCameraPixelsPerMM):0.0}x{(rect.Sz.H / Machine.Settings.InspectionCameraPixelsPerMM):0.0}";

                //var center = new Point()
                //{
                //    X = (int)rect.Center.X,
                //    Y = (int)rect.Center.Y,
                //};

                //CvInvoke.PutText(output, msg, center, FontFace.HersheyPlain, 1, new Bgr(System.Drawing.S.White).MCvScalar);
            }

        }

        #region Find Rotated Rectangles
        private void FindRectangles(Image<Gray, byte> input, IInputOutputArray output, System.Drawing.Size size, VisionProfile profile)
        {
            UMat edges = new UMat();


            if (profile.FindLines)
            {
                var lines = CvInvoke.HoughLinesP(edges, profile.HoughLinesRHO, profile.HoughLinesTheta * (Math.PI / 180), profile.HoughLinesThreshold, profile.HoughLinesMinLineLength, profile.HoughLinesMaxLineGap);
                foreach (var line in lines)
                {
                    Line(output, line.P1.X, line.P1.Y, line.P2.X, line.P2.Y, System.Drawing.Color.Yellow);
                }
            }

            if (profile.UseCannyEdgeDetection)
            {
                CvInvoke.Canny(input, edges, profile.CannyLowThreshold, profile.CannyHighThreshold, profile.CannyApetureSize, profile.CannyGradient);
            }
            else
            {

                CvInvoke.Threshold(input, edges, profile.ThresholdEdgeDetection, 255, ThresholdType.Binary);
            }

            using (var contours = new VectorOfVectorOfPoint())
            {
                CvInvoke.FindContours(edges, contours, null, RetrType.List, ChainApproxMethod.ChainApproxSimple);
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

                                if (isRectangle)
                                {
                                    var rect = CvInvoke.MinAreaRect(approxContour);

                                    if (rect.Center.X > ((size.Width / 2) - profile.TargetImageRadius) && rect.Center.X < ((size.Width / 2) + profile.TargetImageRadius) &&
                                        rect.Center.Y > ((size.Height / 2) - profile.TargetImageRadius) && rect.Center.Y < ((size.Height / 2) + profile.TargetImageRadius))
                                    {
                                        if (rect.Size.Width > rect.Size.Height && profile.FindLandScape ||
                                            rect.Size.Height > rect.Size.Width && profile.FindPortrait)
                                        {
                                            var point1 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));
                                            var point2 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                            var point3 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                            var point4 = new System.Drawing.Point(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));

                                            var p1 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));
                                            var p2 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                            var p3 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
                                            var p4 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));

                                            var p = new PointF[]
                                            {
                                                new PointF(p1.X, p1.Y),
                                                new PointF(p2.X, p2.Y),
                                                new PointF(p3.X, p3.Y),
                                                new PointF(p4.X, p4.Y),
                                            };

                                            var matrix = new Matrix();

                                            matrix.RotateAt(rect.Angle, rect.Center);
                                            matrix.TransformPoints(p);

                                            FoundRectangle = rect;

                                            Line(output, (int)p[0].X, (int)p[0].Y, (int)p[1].X, (int)p[1].Y, System.Drawing.Color.Red);
                                            Line(output, (int)p[1].X, (int)p[1].Y, (int)p[2].X, (int)p[2].Y, System.Drawing.Color.Red);
                                            Line(output, (int)p[2].X, (int)p[2].Y, (int)p[3].X, (int)p[3].Y, System.Drawing.Color.Red);
                                            Line(output, (int)p[3].X, (int)p[3].Y, (int)p[0].X, (int)p[0].Y, System.Drawing.Color.Red);

                                            //var msg = $"{rect.Angle:0.0} {(rect.Size.Width / Machine.Settings.InspectionCameraPixelsPerMM):0.0}x{(rect.Size.Height / Machine.Settings.InspectionCameraPixelsPerMM):0.0}";

                                            //var center = new Point()
                                            //{
                                            //    X = (int)rect.Center.X,
                                            //    Y = (int)rect.Center.Y,
                                            //};

                                            //CvInvoke.PutText(output, msg, center, FontFace.HersheyPlain, 1, new Bgr(System.Drawing.Color.Red).MCvScalar);


                                            //_rectP1.Add(p1);
                                            //_rectP2.Add(p2);
                                            //_rectP3.Add(p3);
                                            //_rectP4.Add(p4);

                                            /*
                                            CvInvoke.Line(output, point1, point2, new Bgr(System.Drawing.S.Red).MCvScalar);
                                            CvInvoke.Line(output, point2, point3, new Bgr(System.Drawing.S.Red).MCvScalar);
                                            CvInvoke.Line(output, point3, point4, new Bgr(System.Drawing.S.Red).MCvScalar);
                                            CvInvoke.Line(output, point4, point1, new Bgr(System.Drawing.S.Red).MCvScalar);
                                            */
                                        }
                                    }
                                }

                            }
                            else if (profile.FindIrregularPolygons)
                            {
                                var rectEdges = PointCollection.PolyLine(pts, true);
                                for (var idx = 0; idx < rectEdges.Length - 1; ++idx)
                                {
                                    CvInvoke.Line(output, rectEdges[idx].P1, rectEdges[idx].P2, new Bgr(System.Drawing.Color.LightBlue).MCvScalar);
                                }

                            }
                        }
                    }
                }

                //var avg1 = _rectP1.Filtered;
                //var avg2 = _rectP2.Filtered;
                //var avg3 = _rectP3.Filtered;
                //var avg4 = _rectP4.Filtered;

                //if (avg1 != null && avg2 != null && avg3 != null && avg4 != null)
                //{
                //    Line(output, (int)avg1.X, (int)avg1.Y, (int)avg2.X, (int)avg2.Y, System.Drawing.S.Red);
                //    Line(output, (int)avg2.X, (int)avg2.Y, (int)avg3.X, (int)avg3.Y, System.Drawing.S.Red);
                //    Line(output, (int)avg3.X, (int)avg3.Y, (int)avg4.X, (int)avg4.Y, System.Drawing.S.Red);
                //    Line(output, (int)avg4.X, (int)avg4.Y, (int)avg1.X, (int)avg1.Y, System.Drawing.S.Red);
                //}
            }
        }
        #endregion


        public UMat PerformShapeDetection(Image<Bgr, byte> img, MachineCamera camera, VisionProfile profile)
        {
            if (img == null)
            {
                return null;
            }

            try
            {
                

                using (var rotated = new Image<Bgr, byte>(img.Size))
                {
                    if (camera.MirrorXAxis)
                    {
                        CvInvoke.Flip(img, rotated, FlipType.Horizontal);
                        img = rotated;
                    }

                    if (camera.MirrorYAxis)
                    {
                        CvInvoke.Flip(img, rotated, FlipType.Horizontal);
                        img = rotated;
                    }

                    var raw = img;

                    using (var masked = new Image<Bgr, byte>(img.Size))
                    {

                        if (profile.ApplyMask)
                        {
                            using (var mask = new Image<Gray, byte>(img.Size))

                            {
                                //CvInvoke.Circle(mask, new System.Drawing.Point() { X = img.Size.Width / 2, Y = img.Size.Height / 2 }, profile.TargetImageRadius, new Bgr(System.Drawing.Color.White).MCvScalar, -1, Emgu.CV.CvEnum.LineType.AntiAlias);
                                //CvInvoke.BitwiseAnd(img, img, masked, mask);
                                raw = masked;
                            }
                        }

                        using (Image<Gray, Byte> gray = raw.Convert<Gray, Byte>())
                        {
                            using (var blurredGray = new Image<Gray, byte>(gray.Size))
                            using (var thresholdGray = new Image<Gray, byte>(gray.Size))
                            using (var inverted = new Image<Gray, byte>(gray.Size))

                            {
                                var input = gray;

                                if (profile.Invert)
                                {
                                    CvInvoke.BitwiseNot(input, inverted);
                                    input = inverted;
                                }

                                if (profile.ApplyThreshold)
                                {
                                    CvInvoke.Threshold(input, thresholdGray, (profile.PrimaryThreshold / 100.0) * 255, 255, ThresholdType.Binary);
                                    input = thresholdGray;
                                }


                                if (UseBlurredImage)
                                {
                                    CvInvoke.GaussianBlur(input, blurredGray, new System.Drawing.Size(5, 5), profile.GaussianSigma);
                                    input = blurredGray;
                                }

                                var output = ShowOriginalImage ? raw : (IInputOutputArray)input;

                                if (!Machine.Busy)
                                {
                                    if (Show200PixelSquare) ShowCalibrationSquare(output, img.Size);
                                    if (ShowCrossHairs) DrawCrossHairs(output, profile, img.Size);
                                    if (profile.FindCircles) FindCircles(input, output, img.Size, profile);
                                    if (profile.FindCorners) FindCorners(input, output, img.Size, profile);
                                    if (profile.FindRectangles)
                                    {
                                        FindRectangles(input, output, img.Size, profile);
                                    }
                                }
                                else
                                {
                                    _stabilizedPointCount = 0;
                                }

                                if (ShowOriginalImage)
                                    return raw.Clone().ToUMat();
                                else
                                {
                                    using (Image<Bgr, Byte> final = input.Convert<Bgr, Byte>())
                                    {
                                        if (ShowCrossHairs) DrawCrossHairs(final, profile, img.Size);
                                        return final.Clone().ToUMat();
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                /*NOP, sometimes OpenCV acts a little funny. */
                return null;
            }
        }
    }
}