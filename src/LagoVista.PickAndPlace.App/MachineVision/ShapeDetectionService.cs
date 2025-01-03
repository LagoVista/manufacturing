using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV.Util;
using Emgu.CV;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Util;
using System;
using LagoVista.Core.ViewModels;
using System.Drawing.Drawing2D;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using System.Collections.Generic;
using System.Windows.Media.Media3D;
using System.Linq;
using System.Drawing.Text;


namespace LagoVista.PickAndPlace.App.MachineVision
{
    internal class ShapeDetectionService : ViewModelBase
    {
        private List<FoundCircle> _foundCircles = new List<FoundCircle>();
        private List<FoundLine> _foundLines = new List<FoundLine>();
        private List<FoundRectangle> _foundRectangles = new List<FoundRectangle>();

        FloatMedianFilter _cornerMedianFilter = new FloatMedianFilter(4, 1);
        FloatMedianFilter _circleMedianFilter = new FloatMedianFilter(4, 1);
        FloatMedianFilter _circleRadiusMedianFilter = new FloatMedianFilter(4, 1);

        ImageHelper _imageHelper;

        ILocatorViewModel _locatorViewModel;
        IMachineRepo _machineRepo;

        const double PIXEL_PER_MM = 20.0;
        public ShapeDetectionService(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel)
        {
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _imageHelper = new ImageHelper();
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
        private void DrawCrossHairs(IInputOutputArray destImage, MachineCamera camera, System.Drawing.Size size)
        {
            var profile = camera.CurrentVisionProfile;

            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            var scaledRadius = profile.TargetImageRadius * camera.PixelsPerMM;


            _imageHelper.Line(destImage, 0, center.Y, center.X - (int)scaledRadius, center.Y, System.Drawing.Color.Yellow);
            _imageHelper.Line(destImage, center.X + (int)scaledRadius, center.Y, size.Width, center.Y, System.Drawing.Color.Yellow);

            _imageHelper.Line(destImage, center.X, 0, center.X, center.Y - (int)scaledRadius, System.Drawing.Color.Yellow);
            _imageHelper.Line(destImage, center.X, center.Y + (int)scaledRadius, center.X, size.Height, System.Drawing.Color.Yellow);

            _imageHelper.Line(destImage, center.X - (int)scaledRadius, center.Y, center.X + (int)scaledRadius, center.Y, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));
            _imageHelper.Line(destImage, center.X, center.Y - (int)scaledRadius, center.X, center.Y + (int)scaledRadius, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));

            if (_locatorViewModel.LocatorState == MVLocatorState.PartInTape)
            {
                _imageHelper.Line(destImage, center.X - PartSizeWidth, center.Y - PartSizeHeight, center.X - PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);
                _imageHelper.Line(destImage, center.X + PartSizeWidth, center.Y - PartSizeHeight, center.X + PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);

                _imageHelper.Line(destImage, center.X - PartSizeWidth, center.Y + PartSizeHeight, center.X + PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);
                _imageHelper.Line(destImage, center.X - PartSizeWidth, center.Y - PartSizeHeight, center.X + PartSizeWidth, center.Y - PartSizeHeight, System.Drawing.Color.Yellow);
            }
            else
            {
                _imageHelper.Circle(destImage, center.X, center.Y, (int)scaledRadius, System.Drawing.Color.Yellow);
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


        private FoundCircle FindPrevious(Point2D<int> center, int radius)
        {
            var deltaPixel = 10;
            return _foundCircles.FirstOrDefault(cir => Math.Max(Math.Abs(cir.CenterPixels.X - center.X), Math.Abs(cir.CenterPixels.Y - center.Y)) < deltaPixel
                        && cir.RadiusPixels - deltaPixel < radius && cir.RadiusPixels + deltaPixel > radius);
        }

        private void FindCircles(IInputOutputArray input, IInputOutputArray output, MachineCamera camera, System.Drawing.Size size)
        {

            var profile = camera.CurrentVisionProfile;
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            var scaledTarget = Convert.ToInt32(profile.TargetImageRadius * camera.PixelsPerMM);

            var circles = CvInvoke.HoughCircles(input, HoughModes.Gradient, profile.HoughCirclesDP, profile.HoughCirclesMinDistance, profile.HoughCirclesParam1, profile.HoughCirclesParam2, Convert.ToInt32(profile.HoughCirclesMinRadius * camera.PixelsPerMM), Convert.ToInt32(profile.HoughCirclesMaxRadius * camera.PixelsPerMM));

            var currentFoundCircles = new List<FoundCircle>();

            foreach (var circle in circles)
            {
                if (circle.Center.X > ((size.Width / 2) - scaledTarget) && circle.Center.X < ((size.Width / 2) + scaledTarget) &&
                   circle.Center.Y > ((size.Height / 2) - scaledTarget) && circle.Center.Y < ((size.Height / 2) + scaledTarget))
                {
                    var foundCircle = new FoundCircle()
                    {
                        CenterPixels = new Point2D<int>(Convert.ToInt32(circle.Center.X), Convert.ToInt32(circle.Center.Y)),
                        RadiusPixels = Convert.ToInt32(circle.Radius),
                        RadiusMM = Math.Round(circle.Radius / camera.PixelsPerMM, 2),
                        OffsetPixels = new Point2D<int>(Convert.ToInt32(circle.Center.X - center.X), Convert.ToInt32(circle.Center.Y - center.X))
                    };

                    foundCircle.OffsetMM = new Point2D<double>(Math.Round((foundCircle.OffsetPixels.X / camera.PixelsPerMM), 2), Math.Round((foundCircle.OffsetPixels.Y / camera.PixelsPerMM), 2));
                    
                    var previous = FindPrevious(foundCircle.CenterPixels, foundCircle.RadiusPixels);
                    if (previous != null)
                    {
                        previous.FoundCount++;
                        currentFoundCircles.Add(previous);
                        foundCircle = previous;
                    }
                    else
                    {
                        foundCircle.FoundCount = 1;
                        currentFoundCircles.Add(foundCircle);
                    }                    

                    /* If within one pixel of center, state we have a match */
                    //TODO Replace this with a profile setting for image accuracy, points per mm and mm
                    if (foundCircle.OffsetMM.X < 0.1 && foundCircle.OffsetMM.Y < 0.1)
                    {
                        if ( true ) //foundCircle.StandardDeviation.X < 0.7 && foundCircle.StandardDeviation.Y < 0.7 && _stabilizedPointCount++ > 5)
                        {
                            _locatorViewModel.CircleCentered(foundCircle.OffsetMM, camera.CameraType.Value, foundCircle.RadiusMM);
                            foundCircle.Centered = true;
                        }
                        else
                            foundCircle.Centered = false;

                    }
                    else
                    {
                        foundCircle.Centered = false;
                        _locatorViewModel.CircleLocated(foundCircle.OffsetMM, camera.CameraType.Value, foundCircle.RadiusMM, null);
                    }

                    _foundCircles.Clear();
                    _foundCircles.AddRange(currentFoundCircles);
                }
            }
        }


        private void FindCorners(Image<Gray, byte> blurredGray, IInputOutputArray output, MachineCamera camera, System.Drawing.Size size)
        {
            var profile = camera.CurrentVisionProfile;

            var scaledTarget = Convert.ToInt32(profile.TargetImageRadius * camera.PixelsPerMM);

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

                var minX = (size.Width / 2) - scaledTarget;
                var maxX = (size.Width / 2) + scaledTarget;
                var minY = (size.Height / 2) - scaledTarget;
                var maxY = (size.Height / 2) + scaledTarget;

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
                    _locatorViewModel.CornerLocated(offset, camera.CameraType.Value, _cornerMedianFilter.StandardDeviation);
                }
            }
        }


        private void FindRectangles(Image<Gray, byte> input, IInputOutputArray output, MachineCamera camera, System.Drawing.Size size)
        {
            _foundRectangles.Clear();
            _foundLines.Clear();

            UMat edges = new UMat();
            var profile = camera.CurrentVisionProfile;

            if (profile.FindLines)
            {
                var lines = CvInvoke.HoughLinesP(edges, profile.HoughLinesRHO, profile.HoughLinesTheta * (Math.PI / 180), profile.HoughLinesThreshold, profile.HoughLinesMinLineLength, profile.HoughLinesMaxLineGap);
                foreach (var line in lines)
                {
                    _foundLines.Add(new FoundLine()
                    {
                        Start = new Point2D<double>(line.P1.X, line.P1.Y),
                        End = new Point2D<double>(line.P2.X, line.P2.Y),
                    });

//                    _imageHelper.Line(output, line.P1.X, line.P1.Y, line.P2.X, line.P2.Y, System.Drawing.Color.Yellow);
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
                _foundRectangles.Clear();

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
                                var rectEdges = Emgu.CV.PointCollection.PolyLine(pts, true);

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
                                        if (rect.Size.Width > rect.Size.Height && profile.FindLandScape || rect.Size.Height > rect.Size.Width && profile.FindPortrait)
                                        {
                                            var p = rect.ToPointArray();

                                            var matrix = new System.Drawing.Drawing2D.Matrix();
                                            matrix.RotateAt(rect.Angle, rect.Center);
                                            matrix.TransformPoints(p);
                                            
                                            _foundRectangles.Add(new MachineVision.FoundRectangle()
                                            {
                                                 Angle = rect.Angle,
                                                 CenterPixels = new Point2D<double>() {X = rect.Center.X, Y = rect.Center.Y},
                                                 RotatedRect = rect,
                                            });                                            
                                        }
                                    }
                                }
                            }
                            else if (profile.FindIrregularPolygons)
                            {
                                var rectEdges = Emgu.CV.PointCollection.PolyLine(pts, true);
                                for (var idx = 0; idx < rectEdges.Length - 1; ++idx)
                                {
                                    CvInvoke.Line(output, rectEdges[idx].P1, rectEdges[idx].P2, new Bgr(System.Drawing.Color.LightBlue).MCvScalar);
                                }

                            }
                        }
                    }
                }
            }
        }


        private void RenderOutput(Image<Bgr, byte> img, MachineCamera camera, System.Drawing.Size size)
        {
            if (camera.CurrentVisionProfile.ShowCrossHairs) DrawCrossHairs(img, camera, img.Size);
            if (camera.CurrentVisionProfile.Show200PixelSquare) ShowCalibrationSquare(img, img.Size);

            foreach(var foundCircle in _foundCircles)
            {
                _imageHelper.Circle(img, foundCircle, size);
            }
        }

        private void FindShapes(Image<Gray, byte> input, IInputOutputArray output, MachineCamera camera, System.Drawing.Size size)
        {
            if (!_machineRepo.CurrentMachine.Busy)
            {
                if (camera.CurrentVisionProfile.FindCircles) FindCircles(input, output, camera, size);
                if (camera.CurrentVisionProfile.FindCorners) FindCorners(input, output, camera, size);
                if (camera.CurrentVisionProfile.FindRectangles) FindRectangles(input, output, camera, size);
            }
            else
            {
                _foundCircles.Clear();
                _foundLines.Clear();
                _foundRectangles.Clear();
            }           
        }



        public UMat PerformShapeDetection(Image<Bgr, byte> img, MachineCamera camera)
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
                        CvInvoke.Flip(img, rotated, FlipType.Vertical);
                        img = rotated;
                    }

                    var raw = img;

                    using (var masked = new Image<Bgr, byte>(img.Size))
                    {

                        if (camera.CurrentVisionProfile.ApplyMask)
                        {
                            using (var mask = new Image<Gray, byte>(img.Size))

                            {
                                CvInvoke.Circle(mask, new System.Drawing.Point() { X = img.Size.Width / 2, Y = img.Size.Height / 2 }, Convert.ToInt32(camera.CurrentVisionProfile.TargetImageRadius * camera.PixelsPerMM), new Bgr(System.Drawing.Color.White).MCvScalar, -1, Emgu.CV.CvEnum.LineType.AntiAlias);
                                CvInvoke.BitwiseAnd(img, img, masked, mask);
                                raw = masked;
                            }
                        }

                        using (var gray = raw.Convert<Gray, Byte>())
                        using (var blurredGray = new Image<Gray, byte>(gray.Size))
                        using (var thresholdGray = new Image<Gray, byte>(gray.Size))
                        using (var inverted = new Image<Gray, byte>(gray.Size))
                        {
                            var input = gray;

                            if (camera.CurrentVisionProfile.Invert)
                            {
                                CvInvoke.BitwiseNot(input, inverted);
                                input = inverted;
                            }

                            if (camera.CurrentVisionProfile.ApplyThreshold)
                            {
                                CvInvoke.Threshold(input, thresholdGray, (camera.CurrentVisionProfile.PrimaryThreshold / 100.0) * 255, 255, ThresholdType.Binary);
                                input = thresholdGray;
                            }

                            if (camera.CurrentVisionProfile.UseBlurredImage)
                            {
                                CvInvoke.GaussianBlur(input, blurredGray, new System.Drawing.Size(5, 5), camera.CurrentVisionProfile.GaussianSigmaX);
                                input = blurredGray;
                            }

                            var output = camera.CurrentVisionProfile.ShowOriginalImage ? raw : (IInputOutputArray)input;

                            if (camera.CurrentVisionProfile.PerformShapeDetection)
                                FindShapes(input, output, camera, img.Size);

                            if (camera.CurrentVisionProfile.ShowOriginalImage)
                            {
                                RenderOutput(raw, camera, img.Size);
                                return raw.Clone().ToUMat();
                            }
                            else
                            {
                                using (Image<Bgr, Byte> final = input.Convert<Bgr, Byte>())
                                {
                                    RenderOutput(final, camera, img.Size);
                                    return final.Clone().ToUMat();
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

    public class FoundLine
    {
        public Point2D<double> Start { get; set; }
        public Point2D<double> End { get; set; }
    }

    public class FoundCircle
    {
        public Point2D<int> CenterPixels { get; set; }
        public Point2D<int> OffsetPixels { get; set; }
        public Point2D<double> OffsetMM { get; set; }
        public int RadiusPixels { get; set; }
        public double RadiusMM { get; set; }
        public bool Centered { get; set; }
        public Point2D<double> OffsetFromCenterMM { get; set; }
        public Point2D<double> Position { get; set; }
        public Point2D<double> StandardDeviation { get; set; }

        public int FoundCount { get; set; }
    }

    public class FoundRectangle
    {
        public Point2D<double> CenterPixels { get; set; }
        public Point2D<double> Position { get; set; }
        public double Angle { get; set; }
        public RotatedRect RotatedRect { get; set; }
    }
}
