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
        private readonly FeederBarcodeTemporalMonitor _feederBarcodeTemporal = new FeederBarcodeTemporalMonitor();
        private readonly NozzleOpeningHeadContext _nozzleOpeningHeadContext =
            new NozzleOpeningHeadContext();
        private double? _activeNozzleExpectedDiameterMm;
        private int? _activeNozzleHeadIndex;
        private readonly KnownLocationAutoThresholdGate _autoThresholdGate = new KnownLocationAutoThresholdGate();
        private VisionProfile _autoProfile;
        private (double Scale, double Search, double Min, double Max, double Tolerance, int Count) _autoGeometry;

        public byte[] LatestFeederBarcodeSignal => _feederBarcode?.Signal;
        private string _latestCalibrationDiscDiagnostic =
            "No calibration-disc frame processed.";
        public string LatestCalibrationDiscDiagnostic => _latestCalibrationDiscDiagnostic;

        public void RegisterVerifiedNozzleOpening(string nozzleIdentity,
            double diameterMm, double pixelsPerMm)
        {
            _nozzleOpeningHeadContext.RegisterVerifiedTune(
                nozzleIdentity, diameterMm, pixelsPerMm);
            _circleDetector.Reset();
        }

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

        private static void DrawFeederCornerLabel(IInputOutputArray image,
            System.Drawing.Size size, string message, int line,
            MCvScalar foreground, bool prominent = false)
        {
            if (String.IsNullOrWhiteSpace(message))
                return;

            // The camera frame is often 1920px wide but is displayed in a
            // roughly 400px window. Proportional text remains legible after
            // WPF scales the image down. Keep a dark stroke for white-image
            // backgrounds, without adding a filled box over the fiducial.
            // Give the first line a little more top clearance after WPF
            // shrinks the 1920px frame into the camera preview. Slightly
            // smaller font and increased line spacing avoid clipping.
            var fontScale = Math.Max(0.72, Math.Min(2.8,
                size.Width / (prominent ? 680.0 : 760.0)));
            var thickness = Math.Max(1, (int)Math.Round(size.Width / 620.0));
            var x = Math.Max(8, (int)Math.Round(size.Width * 0.016));
            var y = Math.Max(34, (int)Math.Round(size.Width * 0.083)) +
                line * Math.Max(25, (int)Math.Round(size.Width * 0.052));
            var position = new Point(x, y);
            CvInvoke.PutText(image, message, position,
                FontFace.HersheySimplex, fontScale, new MCvScalar(0, 0, 0),
                thickness + 4, LineType.AntiAlias);
            CvInvoke.PutText(image, message, position,
                FontFace.HersheySimplex, fontScale, foreground,
                thickness, LineType.AntiAlias);
        }

        private void RenderOutput(IMVImage<IInputOutputArray> img, MachineCamera camera, System.Drawing.Size size)
        {
            // During final stationary disc proof, the WPF overlay is the only
            // source of the acceptance outline and crosshair. The detector
            // still runs on every frame; only these raw annotations are hidden.
            if (camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_CalibrationDisc &&
                camera.CurrentVisionProfile.HideCalibrationDiscCandidateOverlay)
                return;
            if (camera.CurrentVisionProfile.ShowCrossHairs) _imageHelper.DrawCrossHairs(img, camera, size);
            if (camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_FeederBarcode)
            {
                // Barcode decoding is separate from Feeder Origin corner alignment.
                // Only show the fixed sampling band and measured barcode runs.
                FeederBarcodeDetector.Draw(img.Image, _feederBarcode, size, _feederBarcodeVertical);
                return;
            }
            if (camera.CurrentVisionProfile.Show200PixelSquare) _imageHelper.ShowCalibrationSquare(img, size);

            if (camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_FeederOrigin)
            {
                var corner = _cornerDetector.FoundCorners.FirstOrDefault();
                if (corner?.Position != null)
                {
                    // Draw exactly the two line segments accepted by
                    // FeederWhiteCornerFinder. Their extrapolated intersection
                    // is the orange/green crosshair; the physical cut tip may
                    // be slightly rounded and need not contain a white pixel.
                    if (corner.TopEdgeStart != null && corner.TopEdgeEnd != null)
                    {
                        CvInvoke.Line(img.Image,
                            new Point((int)Math.Round(corner.TopEdgeStart.X),
                                (int)Math.Round(corner.TopEdgeStart.Y)),
                            new Point((int)Math.Round(corner.TopEdgeEnd.X),
                                (int)Math.Round(corner.TopEdgeEnd.Y)),
                            new MCvScalar(255, 230, 0), 2);
                    }
                    if (corner.LeftEdgeStart != null && corner.LeftEdgeEnd != null)
                    {
                        CvInvoke.Line(img.Image,
                            new Point((int)Math.Round(corner.LeftEdgeStart.X),
                                (int)Math.Round(corner.LeftEdgeStart.Y)),
                            new Point((int)Math.Round(corner.LeftEdgeEnd.X),
                                (int)Math.Round(corner.LeftEdgeEnd.Y)),
                            new MCvScalar(255, 0, 255), 2);
                    }

                    var point = new Point((int)Math.Round(corner.Position.X),
                        (int)Math.Round(corner.Position.Y));
                    // A stable corner is not necessarily centered. Only the
                    // profile's existing Centered check earns a green marker.
                    // Amber means locked but the operator must still jog.
                    var color = corner.Centered
                        ? new MCvScalar(0, 255, 0)
                        : corner.PositionStable
                            ? new MCvScalar(0, 255, 255)
                            : new MCvScalar(0, 180, 255);
                    CvInvoke.Circle(img.Image, point, 12, color, 2);
                    CvInvoke.Line(img.Image, new Point(point.X - 18, point.Y),
                        new Point(point.X + 18, point.Y), color, 1);
                    CvInvoke.Line(img.Image, new Point(point.X, point.Y - 18),
                        new Point(point.X, point.Y + 18), color, 1);
                    // The 1920px position-camera image is displayed much
                    // smaller in WPF. The old fixed font scales (0.42-0.49)
                    // became practically unreadable. Draw fewer, large lines
                    // sized to the source frame and put them at the top-left
                    // away from the camera Play controls at the bottom.
                    // State comes from the detector's existing per-profile
                    // centering tolerance; no second threshold or gate.
                    var headline = corner.Centered
                        ? $"CORNER CENTERED ({point.X},{point.Y})"
                        : corner.PositionStable
                            ? "CORNER LOCKED - JOG TO CENTER"
                            : $"CORNER ACQUIRING ({point.X},{point.Y})";
                    var offsets = corner.OffsetMM;
                    var measurements = $"X {offsets.X:+0.00;-0.00;0.00}  " +
                        $"Y {offsets.Y:+0.00;-0.00;0.00} mm";
                    var scale = $"CAMERA SCALE {corner.PixelsPerMm:F1} px/mm";
                    DrawFeederCornerLabel(img.Image, size, headline, 0, color, true);
                    DrawFeederCornerLabel(img.Image, size, measurements, 1, color);
                    DrawFeederCornerLabel(img.Image, size, scale, 2, color);
                    DrawFeederCornerLabel(img.Image, size, corner.EdgeSummary, 3, color);
                }
                else
                {
                    var detail = _cornerDetector.LatestStatus ?? String.Empty;
                    var ambiguous = detail.StartsWith("WHITE CORNER AMBIGUOUS",
                        StringComparison.Ordinal);
                    var warning = new MCvScalar(0, 180, 255);
                    DrawFeederCornerLabel(img.Image, size,
                        ambiguous ? "MULTIPLE CORNERS" : "CORNER NOT FOUND",
                        0, warning, true);
                    DrawFeederCornerLabel(img.Image, size,
                        ambiguous ? "REDUCE SEARCH RADIUS" : "CHECK THRESHOLD / RADIUS",
                        1, warning);
                }
                return;
            }

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
            var circles = _circleDetector.FoundCircles.ToList();
            foreach (var foundCircle in circles)
            {
                if (stagingHole)
                {
                    // Preserve the staging-hole rendering and its guided calibration display.
                    _imageHelper.Circle(img, camera.CurrentVisionProfile.ZoomLevel, foundCircle, size);
                    DrawStagingHoleOutline(img, foundCircle);
                }
                else if (camera.CurrentVisionProfile.FindCircles)
                {
                    // Keep the observed nozzle visible instead of drawing full-frame
                    // candidate crosshairs and labels through its concentric features.
                    DrawCircleCandidateMarker(img, foundCircle,
                        camera.CurrentVisionProfile.Key != VisionProfile.VisionProfile_CalibrationDisc);
                }
            }
            if (stagingHole && camera.CurrentVisionProfile.FindCircles)
                DrawStagingHoleMeasurements(img, camera, size);
            else if (camera.CurrentVisionProfile.FindCircles)
            {
                // Render all candidate measurements at a fixed top position.
                // Draw text after candidate markers so no crosshair covers it.
                for (var index = 0; index < circles.Count; index++)
                    DrawCircleCandidateAnnotation(img, camera, circles[index], size, index + 1);
            }

            // A failed nozzle acquisition must still tell the operator which
            // calibration is actually active. No measurements are drawn near
            // the opening itself. This is diagnostic UI only, not input.
            var activeProfile = camera.CurrentVisionProfile;
            var nozzleProfile = activeProfile.Key == VisionProfile.VisionProfile_Nozzle ||
                                activeProfile.Key == VisionProfile.VisionProfile_NozzleCalibration;
            if (nozzleProfile && circles.Count == 0 && size.Width >= 260)
            {
                var scale = camera.EffectivePixelsPerMM;
                var source = activeProfile.PixelsPerMM.HasValue ? "PROFILE" : "CAMERA";
                var enabled = activeProfile.PerformShapeDetection && activeProfile.FindCircles;
                var state = !enabled ? "DETECTION OFF" :
                    activeProfile.UseAdaptiveDarkOpening ? "NO OPENING LOCK" : "NO HOUGH CIRCLE";
                var expected = _activeNozzleExpectedDiameterMm ??
                    DarkOpeningScaleGuard.EffectiveExpectedDiameterMm(
                        activeProfile.ExpectedDarkOpeningDiameterMm,
                        activeProfile.DarkOpeningTunedPixelsPerMM, scale);
                var headLabel = _activeNozzleHeadIndex.HasValue
                    ? $"HEAD {_activeNozzleHeadIndex.Value}" : "NO HEAD";
                var mode = expected > 0
                    ? $"{headLabel} DIA {expected:F2} MM"
                    : $"{headLabel} DIA REACQUIRE";
                var acquisitionPx = activeProfile.UseAdaptiveDarkOpening
                    ? DarkCircularOpeningLocator.EffectiveAcquisitionRadiusPixels(
                        size.Width, size.Height, scale, activeProfile.TargetImageRadius)
                    : activeProfile.TargetImageRadius * scale;
                DrawOutlinedCircleText(img.Image,
                    $"{state}  {scale:F1} PX/MM ({source})",
                    new Point(10, 22), 0.46, 1, new MCvScalar(0, 255, 255));
                DrawOutlinedCircleText(img.Image,
                    $"ROI {acquisitionPx:F0} PX  BASE {activeProfile.TargetImageRadius:F1} MM  {mode}",
                    new Point(10, 43), 0.43, 1, new MCvScalar(0, 255, 255));
                // These counts come from raw grayscale candidates, not the
                // thresholded display. Diagnostic-only: never influences lock.
                if (activeProfile.UseAdaptiveDarkOpening &&
                    !String.IsNullOrWhiteSpace(_circleDetector.LatestNozzleRejection))
                    DrawOutlinedCircleText(img.Image,
                        _circleDetector.LatestNozzleRejection,
                        new Point(10, 64), 0.36, 1, new MCvScalar(0, 255, 255));
            }

            if (camera.CurrentVisionProfile.LocateByPads)
            {
                foreach(var rect in _rectangleDetector.FoundRectangles)
                {
                    _imageHelper.DrawRect(img, camera.CurrentVisionProfile.ZoomLevel, rect,
                        rect.Centered ? System.Drawing.Color.Lime : System.Drawing.Color.Red);
                }
            }
            else
            {
                var rect = _rectangleDetector.FoundRectangles.OrderByDescending(rect => rect.FoundCount).FirstOrDefault();
                if (rect != null)
                {
                    _imageHelper.DrawRect(img, camera.CurrentVisionProfile.ZoomLevel, rect,
                        rect.Centered ? System.Drawing.Color.Lime : System.Drawing.Color.Red);
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

        // Non-staging circle profiles draw only the candidate outline and a short
        // center marker. No candidate line spans the entire frame.
        private static void DrawCircleCandidateMarker(
            IMVImage<IInputOutputArray> image, MVLocatedCircle circle,
            bool permitImageCenteredGreen)
        {
            var center = new Point((int)Math.Round(circle.CenterPixels.X), (int)Math.Round(circle.CenterPixels.Y));
            var radius = Math.Max(1, (int)Math.Round(circle.RadiusPixels));
            // The calibration disc may be centered in the IMAGE yet still be
            // off-center relative to the NOZZLE. Only the saved-offset third
            // pickup overlay may turn that disc green.
            var color = permitImageCenteredGreen && circle.Centered
                ? new MCvScalar(0, 255, 0)
                : new MCvScalar(0, 255, 255);

            CvInvoke.Circle(image.Image, center, radius, color, 2, LineType.AntiAlias);
            var cross = Math.Max(5, Math.Min(12, radius / 4));
            CvInvoke.Line(image.Image, new Point(center.X - cross, center.Y),
                new Point(center.X + cross, center.Y), color, 2, LineType.AntiAlias);
            CvInvoke.Line(image.Image, new Point(center.X, center.Y - cross),
                new Point(center.X, center.Y + cross), color, 2, LineType.AntiAlias);
        }

        // Keep the diagnostics at the top of the camera preview, away from the
        // centered nozzle. Multiple tracked circles get distinct rows. The
        // existing diagnostics list below the video remains available as well.
        private static void DrawCircleCandidateAnnotation(
            IMVImage<IInputOutputArray> image, MachineCamera camera,
            MVLocatedCircle circle, Size size, int number)
        {
            if (camera.EffectivePixelsPerMM <= 0 || size.Width < 260 || size.Height < 90)
                return;

            var fontScale = Math.Max(0.5, Math.Min(2.2, size.Width / 1000.0));
            var thickness = Math.Max(1, (int)Math.Round(fontScale));
            var lineHeight = Math.Max(20, (int)Math.Round(27 * fontScale));
            var rowHeight = lineHeight * 2 + 10;
            var firstBaseline = 8 + (number - 1) * rowHeight + lineHeight;
            // Cap annotations to the top third of the preview. Additional
            // candidates still appear in the existing diagnostic list.
            if (firstBaseline + lineHeight > size.Height / 3)
                return;

            var scale = camera.EffectivePixelsPerMM;
            var offsetX = circle.OffsetPixels.X / scale;
            var offsetY = -circle.OffsetPixels.Y / scale;
            var diameter = circle.RadiusPixels * 2 / scale;
            var disc = camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_CalibrationDisc;
            var status = !circle.Stabilized ? "TRACKING"
                       : !disc && circle.Centered ? "CENTERED"
                       : circle.PositionStable ? "STABLE" : "DETECTED";
            var color = !disc && circle.Centered ? new MCvScalar(0, 255, 0)
                                                : new MCvScalar(0, 255, 255);
            var first = $"#{number} {status}  X {offsetX:+0.00;-0.00;0.00}  Y {offsetY:+0.00;-0.00;0.00} mm";
            var jitter = circle.PositionSpreadMM.HasValue
                ? $"{circle.PositionSpreadMM.Value:0.00} mm" : "--";
            var second = $"D {diameter:0.00} mm   JIT {jitter}   N={circle.FoundCount}";

            // Black stroke keeps colored text legible on both grayscale and
            // thresholded images without a solid box obscuring the camera.
            DrawOutlinedCircleText(image.Image, first, new Point(12, firstBaseline),
                fontScale, thickness, color);
            DrawOutlinedCircleText(image.Image, second, new Point(12, firstBaseline + lineHeight),
                fontScale, thickness, color);
        }

        private static void DrawOutlinedCircleText(IInputOutputArray image, string text,
            Point position, double fontScale, int thickness, MCvScalar color)
        {
            CvInvoke.PutText(image, text, position, FontFace.HersheySimplex,
                fontScale, new MCvScalar(0, 0, 0), thickness + 4, LineType.AntiAlias);
            CvInvoke.PutText(image, text, position, FontFace.HersheySimplex,
                fontScale, color, thickness, LineType.AntiAlias);
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

        private void FindShapes(IMVImage<IInputOutputArray> input, IInputOutputArray output,
            MachineCamera camera, System.Drawing.Size size, double? expectedOpeningDiameterOverrideMm)
        {
            if (!_machineRepo.CurrentMachine.Busy)
            {
                if (camera.CurrentVisionProfile.FindCircles)
                {
                    _circleDetector.FindCircles(input, camera, size, expectedOpeningDiameterOverrideMm);
                    if (camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_CalibrationDisc)
                        _latestCalibrationDiscDiagnostic = _circleDetector.LatestCalibrationDiscDiagnostic;
                }
                else
                {
                    _circleDetector.Reset();
                    if (camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_CalibrationDisc)
                        _latestCalibrationDiscDiagnostic = "Circle detection disabled in active profile.";
                }
                if (camera.CurrentVisionProfile.FindCorners) _cornerDetector.FindCorners(input, camera, size); else _cornerDetector.Reset();
                if (camera.CurrentVisionProfile.FindRectangles) _rectangleDetector.FindRectangles(input, camera, size); else _rectangleDetector.Reset();
                if (camera.CurrentVisionProfile.FindLines) _lineDetector.FindLines(input, camera, size); else _lineDetector.Reset();
                //if(camera.CurrentVisionProfile.FindLines) _line
            }
            else
            {
                if (camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_CalibrationDisc)
                    _latestCalibrationDiscDiagnostic = "Detector paused: machine Busy.";
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
                VisionProfile.VisionProfile_FeederBarcode;
            var adaptiveNozzle = camera.CurrentVisionProfile.UseAdaptiveDarkOpening &&
                (camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_Nozzle ||
                 camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_NozzleCalibration);

            // The camera's saved Nozzle profile is shared across both installed
            // heads. Never enforce Head 1's diameter against Head 2 on selection.
            // A new head acquires without an expected diameter until fresh tuning
            // associates a dimension with that nozzle in this session.
            double? expectedOpeningDiameterOverrideMm = null;
            _activeNozzleExpectedDiameterMm = null;
            _activeNozzleHeadIndex = null;
            if (adaptiveNozzle && camera.CameraType?.Value == CameraTypes.PartInspection)
            {
                var head = _machineRepo.CurrentMachine?.CurrentMachineToolHead;
                var nozzleId = head?.CurrentNozzle == null ? null
                    : Convert.ToString(head.CurrentNozzle.Id);
                var identity = String.IsNullOrWhiteSpace(nozzleId) ? null
                    : $"{head.HeadIndex}:{nozzleId}";
                var effectiveDiameter = _nozzleOpeningHeadContext.Resolve(
                    identity, camera.CurrentVisionProfile,
                    camera.EffectivePixelsPerMM, out var headChanged);
                if (headChanged)
                    _circleDetector.Reset();
                expectedOpeningDiameterOverrideMm = effectiveDiameter;
                _activeNozzleExpectedDiameterMm = effectiveDiameter;
                _activeNozzleHeadIndex = head?.HeadIndex;
            }

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
                    var profile = camera.CurrentVisionProfile;
                    var autoKnownLocation = camera.CameraType?.Value == CameraTypes.Position &&
                        (profile.Key == VisionProfile.VisionProfile_KnownLocation ||
                         profile.Key == VisionProfile.VisionProfile_BoardFiducial) && profile.AutoThreshold;
                    if (camera.CameraType?.Value == CameraTypes.Position &&
                        profile.Key == VisionProfile.VisionProfile_FeederLandmark)
                        return PerformFeederLandmark(img, camera, size);
                    if (autoKnownLocation)
                        return PerformKnownLocationAutoThreshold(img, camera, size);
                    if (_autoProfile != null)
                    {
                        _autoProfile = null;
                        _autoThresholdGate.Reset();
                        _circleDetector.Reset();
                    }

                    // The adaptive nozzle detector must see the REAL camera
                    // pixels, before any profile mask/threshold/inversion.
                    // With a newly calibrated px/mm value, the old circular
                    // mask can otherwise clip the off-center nozzle completely.
                    // Preserve all legacy display processing and other detectors.
                    using (var unmaskedNozzleGray = adaptiveNozzle
                        ? img.Convert<Gray, byte>() : null)
                    using (var masked = new Image<Bgr, byte>(size))
                    {

                        // The circular mask used for holes can truncate a long
                        // feeder barcode. Leave the entire camera image available
                        // to the independent rectangular barcode sampler.
                        if (camera.CurrentVisionProfile.ApplyMask && !feederBarcodeMode &&
                            camera.CurrentVisionProfile.Key != VisionProfile.VisionProfile_BoardFiducial)
                        {
                            using (var mask = new Image<Gray, byte>(size))
                            {
                                var isStagingHole = camera.CurrentVisionProfile.Key ==
                                    VisionProfile.VisionProfile_StagingPlateHole;
                                var searchCenter = isStagingHole
                                    ? StagingHoleSearchRegion.Center(size.Width, size.Height,
                                        camera.EffectivePixelsPerMM,
                                        camera.CurrentVisionProfile.StagingSearchOffsetXmm,
                                        camera.CurrentVisionProfile.StagingSearchOffsetYmm)
                                    : new LagoVista.Core.Models.Drawing.Point2D<double>(
                                        size.Width / 2, size.Height / 2);
                                CvInvoke.Circle(mask, new System.Drawing.Point(
                                        (int)Math.Round(searchCenter.X),
                                        (int)Math.Round(searchCenter.Y)),
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
                            if (camera.CurrentVisionProfile.ApplyMask && camera.CurrentVisionProfile.Invert && !feederBarcodeMode &&
                                camera.CurrentVisionProfile.Key != VisionProfile.VisionProfile_BoardFiducial)
                            {
                                using (var finalMask = new Image<Gray, byte>(size))
                                {
                                    var isStagingHole = camera.CurrentVisionProfile.Key ==
                                        VisionProfile.VisionProfile_StagingPlateHole;
                                    var searchCenter = isStagingHole
                                        ? StagingHoleSearchRegion.Center(size.Width, size.Height,
                                            camera.EffectivePixelsPerMM,
                                            camera.CurrentVisionProfile.StagingSearchOffsetXmm,
                                            camera.CurrentVisionProfile.StagingSearchOffsetYmm)
                                        : new LagoVista.Core.Models.Drawing.Point2D<double>(
                                            size.Width / 2, size.Height / 2);
                                    CvInvoke.Circle(finalMask, new Point(
                                        (int)Math.Round(searchCenter.X),
                                        (int)Math.Round(searchCenter.Y)),
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
                                    feederBarcodeVertical, _feederBarcodeTemporal)
                                : null;

                            var output = camera.CurrentVisionProfile.ShowOriginalImage ? raw : (IInputOutputArray)input;

                            if (camera.CurrentVisionProfile.PerformShapeDetection && !feederBarcodeMode)
                            {
                                // The white cut edge must be measured against
                                // actual black/white pixels. Do not pass the
                                // feeder-origin profile's inverted/blurred/masked
                                // display image to the corner detector.
                                // A white calibration disc must be segmented
                                // from original grayscale, regardless of the
                                // selected profile's display threshold or mask.
                                var shapeInput = adaptiveNozzle
                                    ? (IInputOutputArray)unmaskedNozzleGray
                                    : camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_FeederOrigin ||
                                      camera.CurrentVisionProfile.Key == VisionProfile.VisionProfile_CalibrationDisc
                                        ? (IInputOutputArray)gray
                                        : input;
                                FindShapes(new MVImage<IInputOutputArray>(shapeInput),
                                    output, camera, size, expectedOpeningDiameterOverrideMm);
                            }

                            // Visualize the very same Otsu binary segmentation
                            // consumed by CalibrationDisc contour detection.
                            // This changes display only, never detection input.
                            if (camera.CurrentVisionProfile.Key ==
                                    VisionProfile.VisionProfile_CalibrationDisc &&
                                !camera.CurrentVisionProfile.ShowOriginalImage)
                            {
                                using (var smoothedDisc = new Image<Gray, byte>(gray.Size))
                                using (var binaryDisc = new Image<Gray, byte>(gray.Size))
                                {
                                    // Match the CalibrationDisc detector precisely.
                                    // Blur reduces single-pixel edge noise before
                                    // thresholding without changing acceptance gates.
                                    CvInvoke.GaussianBlur(gray, smoothedDisc,
                                        new Size(5, 5), 0);
                                    CvInvoke.Threshold(smoothedDisc, binaryDisc, 0, 255,
                                        ThresholdType.Binary | ThresholdType.Otsu);
                                    using (var displayDisc = binaryDisc.Convert<Bgr, byte>())
                                    {
                                        RenderOutput(new MVImage<IInputOutputArray>(displayDisc),
                                            camera, size);
                                        return displayDisc.Clone();
                                    }
                                }
                            }

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
                                DrawKnownLocationManualThreshold(img, camera, size);
                                return raw.Clone();
                            }
                            else
                            {
                                using (Image<Bgr, byte> final = input.Convert<Bgr, byte>())
                                {
                                    RenderOutput(new MVImage<IInputOutputArray>(final), camera, size);
                                    DrawKnownLocationManualThreshold(final, camera, size);
                                    return final.Clone();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                if (camera?.CurrentVisionProfile?.Key ==
                    VisionProfile.VisionProfile_CalibrationDisc)
                    _latestCalibrationDiscDiagnostic = "Disc detector threw: " + ex.Message;
                if (_autoProfile != null)
                {
                    _autoThresholdGate.Reset();
                    _circleDetector.Reset();
                }
                Debug.WriteLine(ex.Message);
                Debug.WriteLine(ex.StackTrace);
                /*NOP, sometimes OpenCV acts a little funny. */
                return null;
            }
        }

        private void DrawKnownLocationManualThreshold(IInputOutputArray image, MachineCamera camera, Size size)
        {
            var p = camera.CurrentVisionProfile;
            if (camera.CameraType?.Value != CameraTypes.Position || (p.Key != VisionProfile.VisionProfile_KnownLocation && p.Key != VisionProfile.VisionProfile_BoardFiducial))
                return;
            var label = p.ApplyThreshold
                ? $"MANUAL THRESHOLD {p.PrimaryThreshold / 100.0 * 255:F0}/255 ({p.PrimaryThreshold}/100)"
                : "MANUAL THRESHOLD OFF";
            DrawFeederCornerLabel(image, size, label, 0, new MCvScalar(255, 255, 255));
        }

        private Image<Bgr, byte> PerformFeederLandmark(Image<Bgr, byte> raw, MachineCamera camera, Size size)
        {
            var p=camera.CurrentVisionProfile;
            var geometry=(camera.EffectivePixelsPerMM,p.TargetImageRadius,p.HoughCirclesMinRadius,
                p.HoughCirclesMaxRadius,p.ErrorToleranceMM,p.StabilizationCount);
            if(!ReferenceEquals(_autoProfile,p) || !_autoGeometry.Equals(geometry))
            {
                _autoProfile=p; _autoGeometry=geometry; _autoThresholdGate.Reset();
                _circleDetector.Reset(); _cornerDetector.Reset(); _rectangleDetector.Reset(); _lineDetector.Reset();
            }
            using(var gray=raw.Convert<Gray,byte>())
            using(var smoothed=new Image<Gray,byte>(gray.Size))
            using(var binary=new Image<Gray,byte>(gray.Size))
            {
                CvInvoke.GaussianBlur(gray,smoothed,new Size(3,3),0);
                var type=p.Invert ? ThresholdType.BinaryInv : ThresholdType.Binary;
                var threshold=CvInvoke.Threshold(smoothed,binary,p.AutoThreshold ? 0 : p.PrimaryThreshold*255.0/100.0,
                    255,p.AutoThreshold ? type | ThresholdType.Otsu : type);
                var status="DETECTION PAUSED";
                var scale=camera.EffectivePixelsPerMM;
                if(_machineRepo.CurrentMachine.Busy || !p.PerformShapeDetection || !p.FindCircles ||
                    double.IsNaN(scale) || double.IsInfinity(scale) || scale<=0 ||
                    double.IsNaN(p.TargetImageRadius) || double.IsInfinity(p.TargetImageRadius) || p.TargetImageRadius<=0 ||
                    p.TargetImageRadius*scale>int.MaxValue)
                {
                    _circleDetector.Reset(); _autoThresholdGate.Reset();
                }
                else
                {
                    if(!_autoThresholdGate.ObserveThreshold(threshold)) _circleDetector.Reset();
                    _circleDetector.FindCircles(new MVImage<IInputOutputArray>(binary),camera,size);
                    var tracked=_circleDetector.FoundCircles.FirstOrDefault();
                    status=tracked==null ? "NO UNIQUE THREE-DOT LANDMARK" :
                        tracked.Stabilized && tracked.PositionStable ? "STABLE THREE-DOT LANDMARK" : "ACQUIRING THREE-DOT LANDMARK";
                }
                using(var display=p.ShowOriginalImage ? raw.Clone() : binary.Convert<Bgr,byte>())
                {
                    RenderOutput(new MVImage<IInputOutputArray>(display),camera,size);
                    DrawFeederCornerLabel(display,size,$"{(p.AutoThreshold ? "AUTO OTSU" : "MANUAL")} {threshold:F0}/255",0,new MCvScalar(255,255,255));
                    DrawFeederCornerLabel(display,size,status,1,status.StartsWith("STABLE") ? new MCvScalar(0,255,0) : new MCvScalar(0,255,255));
                    return display.Clone();
                }
            }
        }

        private Image<Bgr, byte> PerformKnownLocationAutoThreshold(Image<Bgr, byte> raw,
            MachineCamera camera, Size size)
        {
            var p = camera.CurrentVisionProfile;
            var scale = camera.EffectivePixelsPerMM;
            var boardFiducial = p.Key == VisionProfile.VisionProfile_BoardFiducial;
            var geometry = (scale, p.TargetImageRadius, p.HoughCirclesMinRadius,
                p.HoughCirclesMaxRadius, p.ErrorToleranceMM, p.StabilizationCount);
            if (!ReferenceEquals(_autoProfile, p) || !_autoGeometry.Equals(geometry))
            {
                _autoProfile = p;
                _autoGeometry = geometry;
                _autoThresholdGate.Reset();
                _circleDetector.Reset();
                _cornerDetector.Reset();
                _rectangleDetector.Reset();
                _lineDetector.Reset();
            }

            using (var gray = raw.Convert<Gray, byte>())
            using (var smoothed = new Image<Gray, byte>(gray.Size))
            using (var binary = new Image<Gray, byte>(gray.Size))
            {
                string status = "INVALID CALIBRATED SEARCH REGION";
                double? threshold = null;
                var separation = 0.0;
                // The center gate is unchanged. Include room for the complete
                // largest accepted target so the histogram never cuts a valid edge.
                var extent = (p.TargetImageRadius + (boardFiducial ? p.HoughCirclesMaxRadius : Math.Min(p.HoughCirclesMaxRadius,
                    KnownLocationCylinderCenteringMath.MaximumDiameterMm / 2))) * scale;
                int boardSamplingRadius = 0;
                var valid = boardFiducial
                    ? BoardFiducialImageGeometry.TrySamplingRadius(size.Width, size.Height, scale,
                        p.TargetImageRadius, p.HoughCirclesMaxRadius, out boardSamplingRadius)
                    : !Double.IsNaN(extent) && !Double.IsInfinity(extent) &&
                        extent >= 3 && scale > 0 && p.TargetImageRadius > 0 &&
                        extent < Math.Min(size.Width, size.Height) / 2.0;
                if (valid)
                {
                    var radius = boardFiducial ? boardSamplingRadius : (int)Math.Ceiling(extent);
                    var roi = new Rectangle(size.Width / 2 - radius, size.Height / 2 - radius,
                        radius * 2, radius * 2);
                    CvInvoke.GaussianBlur(gray, smoothed, boardFiducial ? new Size(3, 3) : new Size(5, 5), 0);
                    using (var region = smoothed.GetSubRect(roi))
                    using (var rawRegion = gray.GetSubRect(roi))
                    using (var regionBinary = new Image<Gray, byte>(roi.Size))
                    {
                        threshold = CvInvoke.Threshold(region, regionBinary, 0, 255,
                            ThresholdType.Binary | ThresholdType.Otsu);
                        // Same thresholded pixels feed the preview and centroid detector.
                        CvInvoke.Threshold(smoothed, binary, threshold.Value, 255,
                            boardFiducial && p.Invert ? ThresholdType.BinaryInv : ThresholdType.Binary);
                        valid = KnownLocationAutoThresholdGate.Assess(region.Data, rawRegion.Data,
                            (int)threshold.Value, out separation, out status, boardFiducial ? 0.005 : 0.05);
                    }
                }

                if (!valid || _machineRepo.CurrentMachine.Busy ||
                    !p.PerformShapeDetection || !p.FindCircles)
                {
                    _autoThresholdGate.Reset();
                    _circleDetector.Reset();
                    if (valid) status = "DETECTION PAUSED";
                }
                else
                {
                    if (!_autoThresholdGate.ObserveThreshold(threshold.Value))
                        _circleDetector.Reset();
                    _circleDetector.FindCircles(new MVImage<IInputOutputArray>(binary), camera, size);
                    var tracked = _circleDetector.FoundCircles.FirstOrDefault();
                    var targetName = boardFiducial ? "PCB FIDUCIAL" : "DISC";
                    status = tracked == null ? "NO UNIQUE VALID " + targetName :
                        tracked.Stabilized && tracked.PositionStable ? "STABLE " + targetName : "ACQUIRING FRESH " + targetName;
                }

                using (var display = p.ShowOriginalImage ? raw.Clone() : binary.Convert<Bgr, byte>())
                {
                    RenderOutput(new MVImage<IInputOutputArray>(display), camera, size);
                    var label = threshold.HasValue
                        ? $"AUTO OTSU {threshold.Value:F0}/255 ({threshold.Value * 100 / 255:F1}/100)"
                        : "AUTO OTSU UNAVAILABLE";
                    DrawFeederCornerLabel(display, size, label, 0, new MCvScalar(255, 255, 255));
                    DrawFeederCornerLabel(display, size, $"{status} / CONTRAST {separation:F0}", 1,
                        status.StartsWith("STABLE ") ? new MCvScalar(0, 255, 0) : new MCvScalar(0, 255, 255));
                    if (boardFiducial && valid)
                        DrawFeederCornerLabel(display, size,
                            _circleDetector.LatestBoardFiducialDiagnostic ?? $"ROI {boardSamplingRadius * 2}px",
                            2, new MCvScalar(255, 255, 255));
                    return display.Clone();
                }
            }
        }

        public IEnumerable<MVLocatedCircle> FoundCircles => _circleDetector.FoundCircles;

        public IEnumerable<MVLocatedRectangle> FoundRectangles => _rectangleDetector.FoundRectangles;

        public IEnumerable<MVLocatedCorner> FoundCorners => _cornerDetector.FoundCorners;

        public void Reset()
        {
            _feederBarcode = null;
            _feederBarcodeVertical = null;
            _feederBarcodeTemporal.Reset();
            _latestCalibrationDiscDiagnostic = "No calibration-disc frame processed since reset.";
            _autoThresholdGate.Reset();
            _autoProfile = null;
            _circleDetector.Reset();
            _rectangleDetector.FoundRectangles.Clear();
            _cornerDetector.FoundCorners.Clear();
            _lineDetector.Reset();
        }
    }
}
