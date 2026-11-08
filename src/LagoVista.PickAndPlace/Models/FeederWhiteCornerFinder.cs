using System;
using LagoVista.Core.Models.Drawing;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    /// <summary>
    /// Optical top-left of the WHITE cut label, not the black barcode start bar.
    /// This is only a measured pixel location; it never moves or calibrates a feeder.
    /// </summary>
    public sealed class FeederWhiteCornerObservation
    {
        public bool Found { get; }
        public int X { get; }
        public int Y { get; }
        public int FrameWidth { get; }
        public int FrameHeight { get; }
        public int ComponentAreaPixels { get; }
        public double OpticalPixelsPerMm { get; }
        // Lines are fitted to straight edge sections AWAY from the rough tip.
        // Both can be drawn over the live Feeder Origin camera image.
        public Point2D<double> TopEdgeStart { get; }
        public Point2D<double> TopEdgeEnd { get; }
        public Point2D<double> LeftEdgeStart { get; }
        public Point2D<double> LeftEdgeEnd { get; }
        public int TopEdgeSamples { get; }
        public int LeftEdgeSamples { get; }
        public double EdgeFitErrorPixels { get; }
        public string Status { get; }

        public FeederWhiteCornerObservation(bool found, int x, int y,
            int frameWidth, int frameHeight, int componentAreaPixels, string status,
            double opticalPixelsPerMm = 0,
            Point2D<double> topEdgeStart = null, Point2D<double> topEdgeEnd = null,
            Point2D<double> leftEdgeStart = null, Point2D<double> leftEdgeEnd = null,
            int topEdgeSamples = 0, int leftEdgeSamples = 0, double edgeFitErrorPixels = 0)
        {
            Found = found;
            X = x;
            Y = y;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            ComponentAreaPixels = componentAreaPixels;
            Status = status;
            OpticalPixelsPerMm = opticalPixelsPerMm;
            TopEdgeStart = topEdgeStart;
            TopEdgeEnd = topEdgeEnd;
            LeftEdgeStart = leftEdgeStart;
            LeftEdgeEnd = leftEdgeEnd;
            TopEdgeSamples = topEdgeSamples;
            LeftEdgeSamples = leftEdgeSamples;
            EdgeFitErrorPixels = edgeFitErrorPixels;
        }

        public FeederWhiteCornerObservation WithOpticalScale(double pixelsPerMm) =>
            new FeederWhiteCornerObservation(Found, X, Y, FrameWidth,
                FrameHeight, ComponentAreaPixels, Status, pixelsPerMm,
                TopEdgeStart, TopEdgeEnd, LeftEdgeStart, LeftEdgeEnd,
                TopEdgeSamples, LeftEdgeSamples, EdgeFitErrorPixels);
    }

    /// <summary>
    /// Finds the first high-contrast white cut edge in a bounded region around
    /// the calculated feeder origin. Uses binary pixels and 4-connected runs,
    /// not a general Harris/contour search or barcode symbol recognition.
    /// </summary>
    public static class FeederWhiteCornerFinder
    {
        /// <summary>
        /// Measure the 5-unit START on the SAME printed label, to convert the
        /// optical corner offset to physical millimeters. This is essential:
        /// the camera's nominal pixels/mm is not trusted for a machine jog.
        /// Returns zero (jog disabled) if START is not verified.
        /// </summary>
        public static double MeasureStartScale(int width, int height,
            Func<int, int, byte> sampleGray, byte threshold,
            FeederWhiteCornerObservation corner, double nominalPixelsPerMm)
        {
            if (sampleGray == null)
                throw new ArgumentNullException(nameof(sampleGray));
            if (corner == null || !corner.Found || nominalPixelsPerMm <= 0 ||
                double.IsNaN(nominalPixelsPerMm) || double.IsInfinity(nominalPixelsPerMm))
                return 0;

            // Start slightly ABOVE the white quiet zone. The exterior may be
            // black; FindBars deliberately ignores its clipped initial run.
            var firstY = Math.Max(0, corner.Y - Math.Max(12,
                (int)Math.Round(nominalPixelsPerMm * 0.25)));
            var length = Math.Min(1150, height - firstY);
            if (length < 250)
                return 0;

            // The label is 2 mm wide and the black barcode bars sit inside
            // it. Try a few positions within its interior to accommodate
            // normal cutting and mounting variation; no image-wide hunt.
            foreach (var fraction in new[] { 0.8, 1.0, 1.2 })
            {
                var startX = corner.X + (int)Math.Round(nominalPixelsPerMm * fraction);
                if (startX <= 0 || startX + 10 >= width)
                    continue;

                var signal = new byte[length];
                for (var y = 0; y < length; y++)
                {
                    var bright = 0;
                    for (var x = startX; x < startX + 10; x++)
                        if (sampleGray(x, firstY + y) >= threshold)
                            bright++;
                    signal[y] = bright >= 6 ? (byte)255 : (byte)0;
                }

                var read = FeederBarcodeSignal.ReadFromStart(signal,
                    nominalPixelsPerMm, threshold);
                if (read.HasVerifiedStart)
                    return read.PixelsPerMmFromStart;
            }

            return 0;
        }


        private sealed class EdgeFit
        {
            public double Slope { get; set; }
            public double Intercept { get; set; }
            public double RmsError { get; set; }
            public int Count { get; set; }
            public double First { get; set; }
            public double Last { get; set; }

            public double At(double along) => Slope * along + Intercept;
        }

        private sealed class CornerCandidate
        {
            public FeederWhiteCornerObservation Corner { get; set; }
            public double Score { get; set; }
        }

        /// <summary>
        /// Fit y = slope*x + intercept (top edge), or x = slope*y + intercept
        /// (left edge). One trimming pass rejects occasional cut/threshold
        /// outliers. Reject poorly defined lines rather than invent a corner.
        /// </summary>
        private static EdgeFit Fit(IReadOnlyList<(double Along, double Across)> points,
            int minimumCount)
        {
            if (points.Count < minimumCount)
                return null;

            EdgeFit Calculate(IEnumerable<(double Along, double Across)> samples)
            {
                var values = samples.ToArray();
                var n = values.Length;
                if (n < minimumCount)
                    return null;
                var meanX = values.Average(item => item.Along);
                var meanY = values.Average(item => item.Across);
                var variance = values.Sum(item => (item.Along - meanX) *
                    (item.Along - meanX));
                if (variance < 0.001)
                    return null;
                var slope = values.Sum(item =>
                    (item.Along - meanX) * (item.Across - meanY)) / variance;
                var intercept = meanY - slope * meanX;
                var error = Math.Sqrt(values.Average(item =>
                    Math.Pow(item.Across - (slope * item.Along + intercept), 2)));
                return new EdgeFit
                {
                    Slope = slope, Intercept = intercept, RmsError = error,
                    Count = n, First = values.Min(item => item.Along),
                    Last = values.Max(item => item.Along)
                };
            }

            var initial = Calculate(points);
            if (initial == null)
                return null;
            var cleaned = points.Where(item =>
                Math.Abs(item.Across - initial.At(item.Along)) <= 3.5).ToArray();
            var fit = Calculate(cleaned);
            if (fit == null || Math.Abs(fit.Slope) > 0.18 ||
                fit.RmsError > 2.3 ||
                fit.Last - fit.First < minimumCount - 1)
                return null;
            return fit;
        }

        /// <summary>
        /// Identify the recognizable WHITE L-shaped label region and fit its
        /// long top and left boundaries. The intersection is more repeatable
        /// than the slightly rounded/scuffed visible cut tip.
        ///
        /// Search is bounded by the Feeder Origin profile's TargetImageRadius;
        /// whiteThreshold is taken from its image-threshold setting.
        /// No Hough/Harris parameters or additional detector are involved.
        /// </summary>
        public static FeederWhiteCornerObservation Find(int width, int height,
            Func<int, int, byte> sampleGray, byte whiteThreshold,
            int searchRadiusPixels)
        {
            if (sampleGray == null)
                throw new ArgumentNullException(nameof(sampleGray));
            if (width < 32 || height < 32)
                throw new ArgumentOutOfRangeException(nameof(width));
            if (searchRadiusPixels < 16)
                throw new ArgumentOutOfRangeException(nameof(searchRadiusPixels));

            var radius = Math.Min(searchRadiusPixels, Math.Min(width, height) / 2 - 5);
            var left = Math.Max(0, width / 2 - radius);
            var top = Math.Max(0, height / 2 - radius);
            var right = Math.Min(width, width / 2 + radius + 1);
            var bottom = Math.Min(height, height / 2 + radius + 1);
            var roiWidth = right - left;
            var roiHeight = bottom - top;
            var count = roiWidth * roiHeight;

            var white = new bool[count];
            var visited = new bool[count];
            var queue = new int[count];
            for (var y = 0; y < roiHeight; y++)
                for (var x = 0; x < roiWidth; x++)
                    white[y * roiWidth + x] =
                        sampleGray(left + x, top + y) >= whiteThreshold;

            var minimumArea = Math.Max(120, radius * radius / 180);
            var minimumHorizontal = Math.Max(32, radius / 8);
            var minimumVertical = Math.Max(48, radius / 5);
            var candidates = new List<CornerCandidate>();

            for (var seed = 0; seed < count; seed++)
            {
                if (visited[seed] || !white[seed])
                    continue;
                visited[seed] = true;
                var head = 0;
                var tail = 1;
                queue[0] = seed;
                var minX = roiWidth;
                var minY = roiHeight;
                var maxX = 0;
                var maxY = 0;

                while (head < tail)
                {
                    var idx = queue[head++];
                    var y = idx / roiWidth;
                    var x = idx - y * roiWidth;
                    minX = Math.Min(minX, x);
                    maxX = Math.Max(maxX, x);
                    minY = Math.Min(minY, y);
                    maxY = Math.Max(maxY, y);

                    if (x > 0) Add(idx - 1);
                    if (x + 1 < roiWidth) Add(idx + 1);
                    if (y > 0) Add(idx - roiWidth);
                    if (y + 1 < roiHeight) Add(idx + roiWidth);
                }

                void Add(int index)
                {
                    if (!visited[index] && white[index])
                    {
                        visited[index] = true;
                        queue[tail++] = index;
                    }
                }

                var componentWidth = maxX - minX + 1;
                var componentHeight = maxY - minY + 1;
                if (tail < minimumArea ||
                    componentWidth < minimumHorizontal + 12 ||
                    componentHeight < minimumVertical + 12 ||
                    minY <= 4 || minX <= 4)
                    continue;

                // A continuous label component has a long, near-horizontal
                // TOP transition and a long, near-vertical LEFT transition.
                // Random glare/text can be bright but seldom satisfies both.
                var topPerColumn = Enumerable.Repeat(int.MaxValue, componentWidth).ToArray();
                var leftPerRow = Enumerable.Repeat(int.MaxValue, componentHeight).ToArray();
                for (var n = 0; n < tail; n++)
                {
                    var index = queue[n];
                    var y = index / roiWidth;
                    var x = index - y * roiWidth;
                    topPerColumn[x - minX] = Math.Min(topPerColumn[x - minX], y);
                    leftPerRow[y - minY] = Math.Min(leftPerRow[y - minY], x);
                }

                var inset = Math.Max(12,
                    Math.Min(20, Math.Min(componentWidth, componentHeight) / 6));
                var topPoints = new List<(double Along, double Across)>();
                var leftPoints = new List<(double Along, double Across)>();
                for (var x = minX + inset; x < maxX - 2; x++)
                {
                    var boundary = topPerColumn[x - minX];
                    if (boundary <= minY + 8)
                        topPoints.Add((left + x, top + boundary));
                }
                for (var y = minY + inset; y < maxY - 2; y++)
                {
                    var boundary = leftPerRow[y - minY];
                    if (boundary <= minX + 8)
                        leftPoints.Add((top + y, left + boundary));
                }

                var topFit = Fit(topPoints, minimumHorizontal);
                var leftFit = Fit(leftPoints, minimumVertical);
                if (topFit == null || leftFit == null)
                    continue;

                var denominator = 1 - topFit.Slope * leftFit.Slope;
                if (Math.Abs(denominator) < 0.25)
                    continue;

                // top y = a*x+b; left x = c*y+d
                var subpixelX = (leftFit.Slope * topFit.Intercept +
                    leftFit.Intercept) / denominator;
                var subpixelY = topFit.At(subpixelX);
                if (double.IsNaN(subpixelX) || double.IsNaN(subpixelY) ||
                    subpixelX < left + minX - 10 ||
                    subpixelX > left + minX + 18 ||
                    subpixelY < top + minY - 10 ||
                    subpixelY > top + minY + 18)
                    continue;

                var xPixel = (int)Math.Round(subpixelX);
                var yPixel = (int)Math.Round(subpixelY);
                var topStart = new Point2D<double>(topFit.First,
                    topFit.At(topFit.First));
                var topEnd = new Point2D<double>(topFit.Last,
                    topFit.At(topFit.Last));
                var leftStart = new Point2D<double>(leftFit.At(leftFit.First),
                    leftFit.First);
                var leftEnd = new Point2D<double>(leftFit.At(leftFit.Last),
                    leftFit.Last);
                var fitError = Math.Max(topFit.RmsError, leftFit.RmsError);
                var observation = new FeederWhiteCornerObservation(
                    true, xPixel, yPixel, width, height, tail,
                    $"WHITE CORNER ({xPixel},{yPixel}) fitted " +
                        $"TOP {topFit.Count}px LEFT {leftFit.Count}px " +
                        $"RMS {fitError:F2}px",
                    topEdgeStart: topStart, topEdgeEnd: topEnd,
                    leftEdgeStart: leftStart, leftEdgeEnd: leftEnd,
                    topEdgeSamples: topFit.Count, leftEdgeSamples: leftFit.Count,
                    edgeFitErrorPixels: fitError);

                var score = topFit.Count * leftFit.Count /
                    (1 + topFit.RmsError + leftFit.RmsError);
                candidates.Add(new CornerCandidate { Corner = observation, Score = score });
            }

            if (candidates.Count == 0)
                return new FeederWhiteCornerObservation(false, 0, 0, width, height, 0,
                    "WHITE CORNER NOT FOUND: need long straight top and left WHITE edges inside search circle.");

            candidates.Sort((a, b) => b.Score.CompareTo(a.Score));
            // Multiple plausible long L-shapes are not safe to jog to.
            if (candidates.Count > 1 && candidates[1].Score >= candidates[0].Score * 0.70)
                return new FeederWhiteCornerObservation(false, 0, 0, width, height, 0,
                    "WHITE CORNER AMBIGUOUS: two strong L-shaped candidates; narrow the Feeder Origin search radius.");

            return candidates[0].Corner;
        }
    }
}
