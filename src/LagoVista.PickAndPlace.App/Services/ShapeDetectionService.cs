// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 124360ea49658ba55ce8dde97fef563a8d326e2feb4f1c28d495a669e72273da
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Emgu.CV;
using LagoVista.Manufacturing.Models;
using System;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.Services;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.App.MachineVision;
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

namespace LagoVista.PickAndPlace.App.Services
{
    internal class ShapeDetectionService : ViewModelBase, IShapeDetectorService<Image<Bgr, byte>>
    {
        IMachineRepo _machineRepo;
    
        IImageHelper<IInputOutputArray> _imageHelper;
        IRectangleDetector<IInputOutputArray> _rectangleDetector;
        ICircleDetector<IInputOutputArray> _circleDetector;
        ICornerDetector<IInputOutputArray> _cornerDetector;
        ILineDetector<IInputOutputArray> _lineDetector;
        FeederBarcodeDetector.Observation _feederBarcode;
        bool? _feederBarcodeVertical;

        public ShapeDetectionService(IMachineRepo machineRepo,
            IImageHelper<IInputOutputArray> imageHelper, IRectangleDetector<IInputOutputArray> rectLocator,
            ICircleDetector<IInputOutputArray> circleLocator, ICornerDetector<IInputOutputArray> cornerDetector,
            ILineDetector<IInputOutputArray> lineDetector)
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _imageHelper = imageHelper ?? throw new ArgumentNullException(nameof(imageHelper));
            _rectangleDetector = rectLocator ?? throw new ArgumentNullException(nameof(rectLocator));
            _circleDetector = circleLocator ?? throw new ArgumentNullException(nameof(circleLocator));
            _lineDetector = lineDetector ?? throw new ArgumentNullException(nameof(lineDetector));
            _cornerDetector = cornerDetector ?? throw new ArgumentNullException(nameof(cornerDetector));
        }

        private void RenderOutput(IMVImage<IInputOutputArray> img, MachineCamera camera, System.Drawing.Size size)
        {
            if (camera.CurrentVisionProfile.ShowCrossHairs) _imageHelper.DrawCrossHairs(img, camera, size);
            if (camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_FeederOrigin)
                FeederBarcodeDetector.Draw(img.Image, _feederBarcode, size, _feederBarcodeVertical);
            if (camera.CurrentVisionProfile.Show200PixelSquare) _imageHelper.ShowCalibrationSquare(img, size);

            if (camera.CurrentVisionProfile.FindLines)
            {
                foreach (var line in _lineDetector.FoundLines)
                {
                    CvInvoke.Line(img.Image,
                        new Point((int)Math.Round(line.Start.X), (int)Math.Round(line.Start.Y)),
                        new Point((int)Math.Round(line.End.X), (int)Math.Round(line.End.Y)),
                        new MCvScalar(0, 255, 0), 2);
                }
            }

            var stagingHole = camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_StagingPlateHole;
            var circleNumber = 0;
            foreach (var foundCircle in _circleDetector.FoundCircles)
            {
                _imageHelper.Circle(img, camera.CurrentVisionProfile.ZoomLevel, foundCircle, size);
                if (stagingHole)
                    DrawStagingHoleOutline(img, foundCircle);
                else if (camera.CurrentVisionProfile.FindCircles)
                    DrawCircleCandidateAnnotation(img, camera, foundCircle, size, ++circleNumber);
            }
            if (stagingHole && camera.CurrentVisionProfile.FindCircles)
                DrawStagingHoleMeasurements(img, camera, size);

            
            if (camera.CurrentVisionProfile.LocateByPads)
            {
                foreach(var rect in _rectangleDetector.FoundRectangles)
                {
                    _imageHelper.DrawRect(img, camera.CurrentVisionProfile.ZoomLevel, rect, rect.Centered ? System.Drawing.Color.Green : System.Drawing.Color.Red);
                }
            }
            else
            {
                var rect = _rectangleDetector.FoundRectangles.OrderByDescending(rect => rect.FoundCount).FirstOrDefault();
                if (rect != null)
                {
                    _imageHelper.DrawRect(img, camera.CurrentVisionProfile.ZoomLevel, rect, rect.Centered ? System.Drawing.Color.Green : System.Drawing.Color.Red);
                }
             
            }
        }

        // Diagnostic annotations are applied only to the displayed staging-hole image.
        // They do not affect the input used by the circle detector or its calibration result.
        private static void DrawStagingHoleOutline(IMVImage<IInputOutputArray> image, MVLocatedCircle circle)
        {
            var center = new Point((int)Math.Round(circle.CenterPixels.X), (int)Math.Round(circle.CenterPixels.Y));
            var radius = Math.Max(1, (int)Math.Round(circle.RadiusPixels));
            var color = circle.Centered ? new MCvScalar(0, 255, 0)
                      : circle.PositionStable ? new MCvScalar(0, 255, 255)
                      : new MCvScalar(0, 0, 255);

            CvInvoke.Circle(image.Image, center, radius, color, 2, LineType.AntiAlias);
        }

        // Other circle-detection profiles may show several candidates in a single frame.
        // Attach measurements to each visible crosshair rather than a shared unlabelled list.
        private static void DrawCircleCandidateAnnotation(
            IMVImage<IInputOutputArray> image, MachineCamera camera,
            MVLocatedCircle circle, Size size, int number)
        {
            if (camera.EffectivePixelsPerMM <= 0 || size.Width < 260 || size.Height < 50)
                return;

            var scale = camera.EffectivePixelsPerMM;
            var center = new Point((int)Math.Round(circle.CenterPixels.X), (int)Math.Round(circle.CenterPixels.Y));
            var radius = Math.Max(1, (int)Math.Round(circle.RadiusPixels));
            var color = circle.Centered ? new MCvScalar(0, 255, 0)
                      : circle.PositionStable ? new MCvScalar(0, 255, 255)
                      : new MCvScalar(0, 0, 255);

            CvInvoke.Circle(image.Image, center, radius, color, 2, LineType.AntiAlias);

            const int labelWidth = 245;
            var labelX = center.X + radius + 8;
            if (labelX + labelWidth > size.Width)
                labelX = center.X - radius - labelWidth - 8;
            labelX = Math.Max(6, Math.Min(labelX, size.Width - labelWidth - 3));
            var labelY = Math.Max(19, Math.Min(center.Y, size.Height - 25));

            var offsetX = circle.OffsetPixels.X / scale;
            var offsetY = -circle.OffsetPixels.Y / scale;
            var diameter = circle.RadiusPixels * 2 / scale;
            var status = !circle.Stabilized ? "TRACKING"
                       : circle.Centered ? "CENTERED"
                       : circle.PositionStable ? "STABLE" : "DETECTED";

            CvInvoke.Rectangle(image.Image, new Rectangle(labelX - 3, labelY - 15, labelWidth, 38),
                new MCvScalar(0, 0, 0), -1);
            CvInvoke.PutText(image.Image,
                $"#{number} X {offsetX:+0.00;-0.00;0.00} Y {offsetY:+0.00;-0.00;0.00} mm",
                new Point(labelX, labelY),
                FontFace.HersheySimplex, 0.38, new MCvScalar(255, 255, 255), 1, LineType.AntiAlias);
            CvInvoke.PutText(image.Image,
                $"D {diameter:0.00} mm  {status}  N={circle.FoundCount}",
                new Point(labelX, labelY + 17),
                FontFace.HersheySimplex, 0.38, new MCvScalar(255, 255, 255), 1, LineType.AntiAlias);
        }

        private void DrawStagingHoleMeasurements(IMVImage<IInputOutputArray> image, MachineCamera camera, Size size)
        {
            var tracked = _circleDetector.FoundCircles
                .OrderBy(circle => circle.OffsetPixels.X * circle.OffsetPixels.X +
                                   circle.OffsetPixels.Y * circle.OffsetPixels.Y)
                .FirstOrDefault();

            var status = tracked == null ? "NO HOLE DETECTED"
                       : tracked.Centered ? "CENTERED"
                       : tracked.PositionStable ? "STABLE - CORRECTION NEEDED"
                       : "TRACKING";

            // Keep the text readable on both the raw and thresholded camera views.
            var width = Math.Min(size.Width - 8, 460);
            if (width <= 0 || size.Height < 48)
                return;

            CvInvoke.Rectangle(image.Image, new Rectangle(4, 4, width, 44),
                new MCvScalar(0, 0, 0), -1);

            string measurement;
            if (tracked != null && camera.EffectivePixelsPerMM > 0)
            {
                var scale = camera.EffectivePixelsPerMM;
                // Match the sign convention used by MVLocatedCircle.OffsetMM.
                var offsetX = tracked.OffsetPixels.X / scale;
                var offsetY = -tracked.OffsetPixels.Y / scale;
                var diameter = tracked.RadiusPixels * 2 / scale;
                measurement = $"X {offsetX:+0.00;-0.00;0.00}  Y {offsetY:+0.00;-0.00;0.00} mm  D {diameter:0.00} mm";
            }
            else
                measurement = "Offset / diameter unavailable";

            CvInvoke.PutText(image.Image, status, new Point(10, 20),
                FontFace.HersheySimplex, 0.43, new MCvScalar(255, 255, 255), 1, LineType.AntiAlias);
            CvInvoke.PutText(image.Image, measurement, new Point(10, 40),
                FontFace.HersheySimplex, 0.42, new MCvScalar(255, 255, 255), 1, LineType.AntiAlias);
        }

        private void FindShapes(IMVImage<IInputOutputArray> input, IInputOutputArray output, MachineCamera camera, System.Drawing.Size size)
        {
            if (!_machineRepo.CurrentMachine.Busy)
            {
                if (camera.CurrentVisionProfile.FindCircles) _circleDetector.FindCircles(input, camera, size); else _circleDetector.Reset();
                if (camera.CurrentVisionProfile.FindCorners) _cornerDetector.FindCorners(input, camera, size); else _cornerDetector.Reset();
                if (camera.CurrentVisionProfile.FindRectangles) _rectangleDetector.FindRectangles(input, camera, size); else _rectangleDetector.Reset();
                if (camera.CurrentVisionProfile.FindLines) _lineDetector.FindLines(input, camera, size); else _lineDetector.Reset();
                //if(camera.CurrentVisionProfile.FindLines) _line
            }
            else
            {
            //    _cornerDetector.Reset();
            //    _circleDetector.Reset();
            //    _rectangleDetector.Reset();
            //    _lineDetector.Reset();
            //
            }
        }

        public Image<Bgr, byte> PerformShapeDetection(IMVImage<Image<Bgr, byte>> source, MachineCamera camera, Size size, bool? feederBarcodeVertical = null)
        {
            if (source == null || source.Image == null)
            {
                return null;
            }

            var img = source.Image;
            _feederBarcodeVertical = feederBarcodeVertical;
            var feederBarcodeMode = camera.CurrentVisionProfile.Key ==
                VisionProfile.VisionProfile_FeederOrigin;

            try
            {
                using (var rotated = new Image<Bgr, byte>(size))
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

                    using (var masked = new Image<Bgr, byte>(size))
                    {

                        // The circular mask used for holes can truncate a long
                        // feeder barcode. Leave the entire camera image available
                        // to the independent rectangular barcode sampler.
                        if (camera.CurrentVisionProfile.ApplyMask && !feederBarcodeMode)
                        {
                            using (var mask = new Image<Gray, byte>(size))
                            {
                                CvInvoke.Circle(mask, new System.Drawing.Point() { X = size.Width / 2, Y = size.Height / 2 },
                                Convert.ToInt32(camera.CurrentVisionProfile.TargetImageRadius * camera.EffectivePixelsPerMM),
                                 new Bgr(System.Drawing.Color.White).MCvScalar,
                                    -1, LineType.AntiAlias);
                                CvInvoke.BitwiseAnd(img, img, masked, mask);
                                raw = masked;
                            }
                        }

                        using (var gray = raw.Convert<Gray, byte>())
                        using (var blurredGray = new Image<Gray, byte>(gray.Size))
                        using (var thresholdGray = new Image<Gray, byte>(gray.Size))
                        using (var inverted = new Image<Gray, byte>(gray.Size))
                        using (var invertedMasked = new Image<Gray, byte>(gray.Size))
                        {
                            var input = gray;

                            if (camera.CurrentVisionProfile.Invert)
                            {
                                CvInvoke.BitwiseNot(input, inverted);
                                input = inverted;
                            }

                            if (camera.CurrentVisionProfile.ApplyThreshold)
                            {
                                CvInvoke.Threshold(input, thresholdGray, camera.CurrentVisionProfile.PrimaryThreshold / 100.0 * 255, 255, ThresholdType.Binary);
                                input = thresholdGray;
                            }

                            if (camera.CurrentVisionProfile.UseBlurredImage)
                            {
                                if (camera.CurrentVisionProfile.GausianKernellSize == 0)
                                    camera.CurrentVisionProfile.GausianKernellSize = 1;
                                if (camera.CurrentVisionProfile.GausianKernellSize % 2 == 0)
                                    camera.CurrentVisionProfile.GausianKernellSize = camera.CurrentVisionProfile.GausianKernellSize - 1;

                                CvInvoke.GaussianBlur(input, blurredGray, new Size(camera.CurrentVisionProfile.GausianKernellSize,camera.CurrentVisionProfile.GausianKernellSize), camera.CurrentVisionProfile.GaussianSigma);
                                input = blurredGray;
                            }

                            // ApplyMask runs before Invert above. Inverting also turns the
                            // masked-out (black) exterior white, which causes external-contour
                            // detection to select the mask background instead of the hole.
                            // Restore a black exterior after all image processing, without
                            // changing the proven centroid detector or the mask-off path.
                            if (camera.CurrentVisionProfile.ApplyMask && camera.CurrentVisionProfile.Invert && !feederBarcodeMode)
                            {
                                using (var finalMask = new Image<Gray, byte>(size))
                                {
                                    CvInvoke.Circle(finalMask, new Point(size.Width / 2, size.Height / 2),
                                        Convert.ToInt32(camera.CurrentVisionProfile.TargetImageRadius * camera.EffectivePixelsPerMM),
                                        new MCvScalar(255), -1, LineType.AntiAlias);
                                    CvInvoke.BitwiseAnd(input, input, invertedMasked, finalMask);
                                    input = invertedMasked;
                                }
                            }

                            // Prototype feeder-marker measurements use raw grayscale so
                            // profile invert/mask settings do not reverse the black-on-white bars.
                            _feederBarcode = feederBarcodeMode
                                ? FeederBarcodeDetector.FindBars(gray, camera.EffectivePixelsPerMM,
                                    camera.CurrentVisionProfile.ApplyThreshold
                                        ? (byte)Math.Round(camera.CurrentVisionProfile.PrimaryThreshold / 100.0 * 255)
                                        : (byte)128,
                                    feederBarcodeVertical)
                                : null;

                            var output = camera.CurrentVisionProfile.ShowOriginalImage ? raw : (IInputOutputArray)input;

                            if (camera.CurrentVisionProfile.PerformShapeDetection)
                                FindShapes(new MVImage<IInputOutputArray>( input), output, camera, size);

                            if(camera.CurrentVisionProfile.ShowRectLocatorImage)
                            {
                                var mat = _rectangleDetector.Image as Mat;
                                CvInvoke.CvtColor(_rectangleDetector.Image, raw, ColorConversion.Gray2Bgr);
                                RenderOutput(new MVImage<IInputOutputArray>(img), camera, size);
                                return raw.Clone();

                            }
                            if (camera.CurrentVisionProfile.ShowOriginalImage)
                            {
                                RenderOutput(new MVImage<IInputOutputArray>(img), camera, size);
                                return raw.Clone();
                            }
                            else
                            {
                                using (Image<Bgr, byte> final = input.Convert<Bgr, byte>())
                                {
                                    RenderOutput(new MVImage<IInputOutputArray>(final), camera, size);
                                    return final.Clone();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                /*NOP, sometimes OpenCV acts a little funny. */
                return null;
            }
        }

        public IEnumerable<MVLocatedCircle> FoundCircles => _circleDetector.FoundCircles;

        public IEnumerable<MVLocatedRectangle> FoundRectangles => _rectangleDetector.FoundRectangles;

        public IEnumerable<MVLocatedCorner> FoundCorners => _cornerDetector.FoundCorners;

        public void Reset()
        {
            _feederBarcode = null;
            _feederBarcodeVertical = null;
            _circleDetector.FoundCircles.Clear();
            _rectangleDetector.FoundRectangles.Clear();
            _cornerDetector.FoundCorners.Clear();
            _lineDetector.Reset();
        }
    }
}
