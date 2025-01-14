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
using System.Drawing;
using System.Collections.Generic;

namespace LagoVista.PickAndPlace.App.Services
{
    internal class ShapeDetectionService : ViewModelBase, IShapeDetectorService<Image<Bgr, byte>>
    {
        ILocatorViewModel _locatorViewModel;
        IMachineRepo _machineRepo;
    
        IImageHelper<IInputOutputArray> _imageHelper;
        IRectangleDetector<IInputOutputArray> _rectangleDetector;
        ICircleDetector<IInputOutputArray> _circleDetector;
        ICornerDetector<IInputOutputArray> _cornerDetector;


        public ShapeDetectionService(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel, 
            IImageHelper<IInputOutputArray> imageHelper, IRectangleDetector<IInputOutputArray> rectLocator,
            ICircleDetector<IInputOutputArray> circleLocator, ICornerDetector<IInputOutputArray> cornerDetector)
        {
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _imageHelper = imageHelper ?? throw new ArgumentNullException(nameof(imageHelper));
            _rectangleDetector = rectLocator ?? throw new ArgumentNullException(nameof(rectLocator));
            _circleDetector = circleLocator ?? throw new ArgumentNullException(nameof(circleLocator));
            _cornerDetector = cornerDetector ?? throw new ArgumentNullException(nameof(cornerDetector));
        }

        private void RenderOutput(IMVImage<IInputOutputArray> img, MachineCamera camera, System.Drawing.Size size)
        {
            if (camera.CurrentVisionProfile.ShowCrossHairs) _imageHelper.DrawCrossHairs(img, camera, size);
            if (camera.CurrentVisionProfile.Show200PixelSquare) _imageHelper.ShowCalibrationSquare(img, size);

            foreach (var foundCircle in _circleDetector.FoundCircles)
            {
                _imageHelper.Circle(img, foundCircle, size);
            }

            foreach(var rect in _rectangleDetector.FoundRectangles)
            {
                _imageHelper.DrawRect(img, rect, Color.Red);
            }
        }

        private void FindShapes(IMVImage<IInputOutputArray> input, IInputOutputArray output, MachineCamera camera, System.Drawing.Size size)
        {
            if (!_machineRepo.CurrentMachine.Busy)
            {
                if (camera.CurrentVisionProfile.FindCircles) _circleDetector.FindCircles(input, camera, size); else _circleDetector.Reset();
                if (camera.CurrentVisionProfile.FindCorners) _cornerDetector.FindCorners(input, camera, size); else _cornerDetector.Reset();
                if (camera.CurrentVisionProfile.FindRectangles) _rectangleDetector.FindRectangles(input, camera, size); else _rectangleDetector.Reset();
                //if(camera.CurrentVisionProfile.FindLines) _line
            }
            else
            {
                _cornerDetector.Reset();
                _circleDetector.Reset();
                _rectangleDetector.Reset();
            }
        }

        public Image<Bgr, byte> PerformShapeDetection(IMVImage<Image<Bgr, byte>> source, MachineCamera camera, Size size)
        {
            if (source == null || source.Image == null)
            {
                return null;
            }

            var img = source.Image;

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

                        if (camera.CurrentVisionProfile.ApplyMask)
                        {
                            using (var mask = new Image<Gray, byte>(size))
                            {
                                CvInvoke.Circle(mask, new System.Drawing.Point() { X = size.Width / 2, Y = size.Height / 2 },
                                Convert.ToInt32(camera.CurrentVisionProfile.TargetImageRadius * camera.CurrentVisionProfile.PixelsPerMM),
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
                                CvInvoke.GaussianBlur(input, blurredGray, new System.Drawing.Size(5, 5), camera.CurrentVisionProfile.GaussianSigma);
                                input = blurredGray;
                            }

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
            catch (Exception)
            {
                /*NOP, sometimes OpenCV acts a little funny. */
                return null;
            }
        }

        public IEnumerable<MVLocatedCircle> FoundCircles => _circleDetector.FoundCircles;

        public IEnumerable<MVLocatedRectangle> FoundRectangles => _rectangleDetector.FoundRectangles;

        public IEnumerable<MVLocatedCorner> FoundCorners => _cornerDetector.FoundCorners;

    }
}
