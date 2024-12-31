using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Util;
using System;
using System.Drawing;
using LagoVista.Core.ViewModels;
using System.Drawing.Drawing2D;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    internal class ShapeDetectionService : ViewModelBase
    {
        FloatMedianFilter _cornerMedianFilter = new FloatMedianFilter(4, 1);
        FloatMedianFilter _circleMedianFilter = new FloatMedianFilter(4, 1);
        FloatMedianFilter _circleRadiusMedianFilter = new FloatMedianFilter(4, 1);

        ImageHelper _imageHelper;

        ILocatorViewModel _locatorViewModel;
        LocatedByCamera _camera;
        IMachineRepo _machineRepo;

        const double PIXEL_PER_MM = 20.0;
        public ShapeDetectionService(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel, LocatedByCamera camera)
        {
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));            
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _camera = camera;
            _imageHelper = new ImageHelper();  
        }

        public VisionSettings VisionSettings { get; set; }

        private Point2D<double> _foundCorner;
        public Point2D<double> FoundCorner
        {
            get { return _foundCorner; }
            set { Set(ref _foundCorner, value); }
        }


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
        private void DrawCrossHairs(IInputOutputArray destImage, VisionSettings profile, System.Drawing.Size size)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            _imageHelper.Line(destImage, 0, center.Y, center.X - profile.TargetImageRadius, center.Y, System.Drawing.Color.Yellow);
            _imageHelper.Line(destImage, center.X + profile.TargetImageRadius, center.Y, size.Width, center.Y, System.Drawing.Color.Yellow);

            _imageHelper.Line(destImage, center.X, 0, center.X, center.Y - profile.TargetImageRadius, System.Drawing.Color.Yellow);
            _imageHelper.Line(destImage, center.X, center.Y + profile.TargetImageRadius, center.X, size.Height, System.Drawing.Color.Yellow);

            _imageHelper.Line(destImage, center.X - profile.TargetImageRadius, center.Y, center.X + profile.TargetImageRadius, center.Y, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));
            _imageHelper.Line(destImage, center.X, center.Y - profile.TargetImageRadius, center.X, center.Y + profile.TargetImageRadius, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));

            if (_locatorViewModel.LocatorState == MVLocatorState.PartInTape)
            {
                _imageHelper.Line(destImage, center.X - PartSizeWidth, center.Y - PartSizeHeight, center.X - PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);
                _imageHelper.Line(destImage, center.X + PartSizeWidth, center.Y - PartSizeHeight, center.X + PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);

                _imageHelper.Line(destImage, center.X - PartSizeWidth, center.Y + PartSizeHeight, center.X + PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);
                _imageHelper.Line(destImage, center.X - PartSizeWidth, center.Y - PartSizeHeight, center.X + PartSizeWidth, center.Y - PartSizeHeight, System.Drawing.Color.Yellow);
            }
            else
            {
               _imageHelper.Circle(destImage, center.X, center.Y, profile.TargetImageRadius, System.Drawing.Color.Yellow);
            }
        }
        #endregion

        public int PartSizeWidth { get; set; } = 12;
        public int PartSizeHeight { get; set; } = 24;

        protected Point2D<double> RequestedPosition
        {
            get;
            set;
        }




        int _stabilizedPointCount = 0;

        #region Find_imageHelper.Circles
        private void FindCircles(IInputOutputArray input, IInputOutputArray output, System.Drawing.Size size, VisionSettings profile)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            var circles = CvInvoke.HoughCircles(input, HoughModes.Gradient, profile.HoughCirclesDP, profile.HoughCirclesMinDistance, profile.HoughCirclesParam1, profile.HoughCirclesParam2, profile.HoughCirclesMinRadius, profile.HoughCirclesMaxRadius);

            var foundCircle = false;
            /* Above will return ALL maching circles, we only want the first one that is in the target image radius in the middle of the screen */
            foreach (var circle in circles)
            {
                if (circle.Center.X > ((size.Width / 2) - profile.TargetImageRadius) && circle.Center.X < ((size.Width / 2) + profile.TargetImageRadius) &&
                   circle.Center.Y > ((size.Height / 2) - profile.TargetImageRadius) && circle.Center.Y < ((size.Height / 2) + profile.TargetImageRadius))
                {
                    _circleMedianFilter.Add(circle.Center.X, circle.Center.Y);
                    _circleRadiusMedianFilter.Add(circle.Radius, 0);
                    foundCircle = true;
                    break;
                }
            }

            if (!foundCircle)
            {
                _circleMedianFilter.Add(null);
                _circleRadiusMedianFilter.Add(null);
            }

            var avg = _circleMedianFilter.Filtered;
            if (avg != null)
            {
                CircleCenter = new Point2D<double>(Math.Round(avg.X, 2), Math.Round(avg.Y, 2));
                StandardDeviation = _circleMedianFilter.StandardDeviation;

                var offset = new Point2D<double>(center.X - avg.X, center.Y - avg.Y);

                var deltaX = Math.Abs(avg.X - center.X);
                var deltaY = Math.Abs(avg.Y - center.Y);
                //Debug.WriteLine($"{deltaX}, {deltaY} - {_stabilizedPointCount} - {_circleRadiusMedianFilter.StandardDeviation.X},{_circleRadiusMedianFilter.StandardDeviation.Y}");
                /* If within one pixel of center, state we have a match */
                if (deltaX < 1.5 && deltaY < 1.5)
                {
                   _imageHelper.Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Green);
                   _imageHelper.Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Green);
                   _imageHelper.Circle(output, (int)avg.X, (int)avg.Y, (int)_circleRadiusMedianFilter.Filtered.X, System.Drawing.Color.Green);
                    if (StandardDeviation.X < 0.7 && StandardDeviation.Y < 0.7)
                    {
                        _stabilizedPointCount++;
                        if (_stabilizedPointCount > 5)
                        {
                          _locatorViewModel.CircleCentered(offset, _camera, _circleRadiusMedianFilter.Filtered.X);
                        }
                    }
                }
                else
                {
                   _imageHelper.Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Red);
                   _imageHelper.Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Red);
                   _imageHelper.Circle(output, (int)avg.X, (int)avg.Y, (int)_circleRadiusMedianFilter.Filtered.X, System.Drawing.Color.Red);
                    _locatorViewModel.CircleLocated(offset, _camera, _circleRadiusMedianFilter.Filtered.X, _circleRadiusMedianFilter.StandardDeviation);
                }
            }
            else
            {
               CircleCenter = null;
            }
        }
        #endregion

        #region Find Corners
        private void FindCorners(Image<Gray, byte> blurredGray, IInputOutputArray output, System.Drawing.Size size, VisionSettings profile)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            using (var cornerDest = new Image<Gray, float>(blurredGray.Size))
            using (var matNormalized = new Image<Gray, float>(blurredGray.Size))
            using (var matScaled = new Image<Gray, float>(blurredGray.Size))
            {
                cornerDest.SetZero();

                int max = -1;
                int x = -1, y = -1;

                CvInvoke.CornerHarris(blurredGray, cornerDest, profile.HarrisCornerBlockSize, profile.HarrisCornerAperture, profile.HarrisCornerK, BorderType.Default);

                CvInvoke.Normalize(cornerDest, matNormalized, 0, 255, NormType.MinMax, DepthType.Cv32F);
                CvInvoke.ConvertScaleAbs(matNormalized, matScaled, 10, 5);

                var minX = (size.Width / 2) - profile.TargetImageRadius;
                var maxX = (size.Width / 2) + profile.TargetImageRadius;
                var minY = (size.Height / 2) - profile.TargetImageRadius;
                var maxY = (size.Height / 2) + profile.TargetImageRadius;

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

                if (x > 0 && y > 0)
                {
                    _cornerMedianFilter.Add(new Point2D<float>(x, y));

                }

                var avg = _cornerMedianFilter.Filtered;
                if (avg != null)
                {
                   _imageHelper.Circle(output, (int)avg.X, (int)avg.Y, 5, System.Drawing.Color.Blue);
                   _imageHelper.Line(output, 0, (int)avg.Y, size.Width, (int)avg.Y, System.Drawing.Color.Blue);
                   _imageHelper.Line(output, (int)avg.X, 0, (int)avg.X, size.Height, System.Drawing.Color.Blue);

                    var offset = new Point2D<double>(center.X - avg.X, center.Y - avg.Y);
                    _locatorViewModel.CornerLocated(offset, _camera, _cornerMedianFilter.StandardDeviation);
                }
            }
        }
        #endregion

        FloatMedianFilter _rectP1 = new FloatMedianFilter();
        FloatMedianFilter _rectP2 = new FloatMedianFilter();
        FloatMedianFilter _rectP3 = new FloatMedianFilter();
        FloatMedianFilter _rectP4 = new FloatMedianFilter();
        FloatMedianFilter _rectTheta = new FloatMedianFilter();

        protected RotatedRect? FoundRectangle { get; set; }

        private void FindRect2(Image<Gray, byte> input, IInputOutputArray output, System.Drawing.Size size)
        {
            var rect = CvInvoke.BoundingRectangle(input);
            if (rect.Width > 0 && rect.Height > 0)
            {
               _imageHelper.Line(output, (int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Top, System.Drawing.Color.White);
               _imageHelper.Line(output, (int)rect.Right, (int)rect.Top, (int)rect.Right, (int)rect.Bottom, System.Drawing.Color.White);
               _imageHelper.Line(output, (int)rect.Right, (int)rect.Bottom, (int)rect.Left, (int)rect.Bottom, System.Drawing.Color.White);
               _imageHelper.Line(output, (int)rect.Left, (int)rect.Bottom, (int)rect.Left, (int)rect.Top, System.Drawing.Color.White);

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
        private void FindRectangles(Image<Gray, byte> input, IInputOutputArray output, System.Drawing.Size size, VisionSettings profile)
        {
            UMat edges = new UMat();


            if (profile.FindLines)
            {
                var lines = CvInvoke.HoughLinesP(edges, profile.HoughLinesRHO, profile.HoughLinesTheta * (Math.PI / 180), profile.HoughLinesThreshold, profile.HoughLinesMinLineLength, profile.HoughLinesMaxLineGap);
                foreach (var line in lines)
                {
                   _imageHelper.Line(output, line.P1.X, line.P1.Y, line.P2.X, line.P2.Y, System.Drawing.Color.Yellow);
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
                                            var p = rect.ToPointArray();

                                            var matrix = new Matrix();
                                            matrix.RotateAt(rect.Angle, rect.Center);
                                            matrix.TransformPoints(p);
                                            FoundRectangle = rect;

                                            var msg = $"{rect.Angle:0.0} {(rect.Size.Width / _machineRepo.CurrentMachine.Settings.InspectionCameraPixelsPerMM):0.0}x{(rect.Size.Height / _machineRepo.CurrentMachine.Settings.InspectionCameraPixelsPerMM):0.0}";
                                            _imageHelper.DrawRect(output, p, msg, System.Drawing.Color.Red);
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
                }}
            }
        
        #endregion

        public UMat PerformShapeDetection(Image<Bgr, byte> img, MachineCamera camera, VisionSettings profile)
        {
            if (img == null)
            {
                return null;
            }

            VisionSettings = profile;

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
                        CvInvoke.Flip(img, rotated, FlipType.Vertical);
                        img = rotated;
                    }

                    var raw = img;

                    using (var masked = new Image<Bgr, byte>(img.Size))
                    {

                        if (profile.ApplyMask)
                        {
                            using (var mask = new Image<Gray, byte>(img.Size))

                            {
                                CvInvoke.Circle(mask, new System.Drawing.Point() { X = img.Size.Width / 2, Y = img.Size.Height / 2 }, profile.TargetImageRadius, new Bgr(System.Drawing.Color.White).MCvScalar, -1, Emgu.CV.CvEnum.LineType.AntiAlias);
                                CvInvoke.BitwiseAnd(img, img, masked, mask);
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


                                if (VisionSettings.UseBlurredImage)
                                {
                                    CvInvoke.GaussianBlur(input, blurredGray, new System.Drawing.Size(5, 5), profile.GaussianSigmaX);
                                    input = blurredGray;
                                }

                                var output = VisionSettings.ShowOriginalImage ? raw : (IInputOutputArray)input;

                                if (!_machineRepo.CurrentMachine.Busy)
                                {
                                    if (VisionSettings.Show200PixelSquare) ShowCalibrationSquare(output, img.Size);
                                    if (VisionSettings.ShowCrossHairs) DrawCrossHairs(output, profile, img.Size);
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

                                if (VisionSettings.ShowOriginalImage)
                                    return raw.Clone().ToUMat();
                                else
                                {
                                    using (Image<Bgr, Byte> final = input.Convert<Bgr, Byte>())
                                    {
                                        if (VisionSettings.ShowCrossHairs) DrawCrossHairs(final, profile, img.Size);
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


        

        private Point2D<double> _circleCenter;
        public Point2D<double> CircleCenter
        {
            get { return _circleCenter; }
            set
            {
                if (value == null)
                {
                    Set(ref _circleCenter, null);
                }
                else if (_circleCenter == null || (_circleCenter.X != value.X ||
                    _circleCenter.Y != value.Y))
                {
                    Set(ref _circleCenter, value);
                }
            }
        }

        private Point2D<double> _standardDeviation;
        public Point2D<double> StandardDeviation
        {
            get { return _standardDeviation; }
            set
            {
                if (value == null)
                {
                    Set(ref _standardDeviation, null);
                }
                else if (_standardDeviation == null || (_standardDeviation.X != value.X || _standardDeviation.Y != value.Y))
                {
                    Set(ref _standardDeviation, value);
                }
            }
        }

    }
}
