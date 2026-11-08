using System;
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
        public string Status { get; }

        public FeederWhiteCornerObservation(bool found, int x, int y,
            int frameWidth, int frameHeight, int componentAreaPixels, string status,
            double opticalPixelsPerMm = 0)
        {
            Found = found;
            X = x;
            Y = y;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            ComponentAreaPixels = componentAreaPixels;
            Status = status;
            OpticalPixelsPerMm = opticalPixelsPerMm;
        }

        public FeederWhiteCornerObservation WithOpticalScale(double pixelsPerMm) =>
            new FeederWhiteCornerObservation(Found, X, Y, FrameWidth,
                FrameHeight, ComponentAreaPixels, Status, pixelsPerMm);
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

            var minimumArea = Math.Max(120, searchRadiusPixels * searchRadiusPixels / 180);
            var minimumWidth = Math.Max(12, radius / 12);
            var minimumHeight = Math.Max(8, radius / 22);
            var candidates = new List<(int X, int Y, int Area)>();

            for (var seed = 0; seed < count; seed++)
            {
                if (!white[seed] || visited[seed])
                    continue;

                visited[seed] = true;
                var head = 0;
                var tail = 1;
                queue[0] = seed;
                var area = 0;
                var minX = roiWidth;
                var maxX = 0;
                var minY = roiHeight;
                var maxY = 0;
                // Find the local left edge at the TOP, not the leftmost
                // pixel anywhere down the strip (printed cuts may slant).
                while (head < tail)
                {
                    var idx = queue[head++];
                    var y = idx / roiWidth;
                    var x = idx - y * roiWidth;
                    area++;
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
                    if (!white[index] || visited[index])
                        return;
                    visited[index] = true;
                    queue[tail++] = index;
                }

                if (area < minimumArea ||
                    maxX - minX + 1 < minimumWidth ||
                    maxY - minY + 1 < minimumHeight ||
                    minY <= 4 || minX <= 4)
                    continue;

                // Reuse the finished flood-fill queue. Avoid allocating a
                // full-height row table for every isolated noise pixel.
                var topRowMinX = new int[6];
                for (var row = 0; row < topRowMinX.Length; row++)
                    topRowMinX[row] = int.MaxValue;
                for (var i = 0; i < tail; i++)
                {
                    var y = queue[i] / roiWidth;
                    var relativeRow = y - minY;
                    if (relativeRow >= 0 && relativeRow < topRowMinX.Length)
                        topRowMinX[relativeRow] = Math.Min(
                            topRowMinX[relativeRow], queue[i] % roiWidth);
                }

                var topXs = topRowMinX
                    .Where(value => value != int.MaxValue)
                    .OrderBy(value => value)
                    .ToArray();
                if (topXs.Length < 3)
                    continue;
                var xPixel = left + topXs[topXs.Length / 2];
                var yPixel = top + minY;
                // Both boundaries must be a BLACK -> WHITE transition.
                // This avoids treating a white interior patch as an origin.
                if (xPixel < 4 || yPixel < 4 ||
                    sampleGray(xPixel, yPixel - 3) >= whiteThreshold ||
                    sampleGray(xPixel - 3, yPixel + 3) >= whiteThreshold)
                    continue;

                candidates.Add((xPixel, yPixel, area));
            }

            if (candidates.Count == 0)
                return new FeederWhiteCornerObservation(false, 0, 0, width, height, 0,
                    "WHITE CORNER NOT FOUND: no complete high-contrast top-left white edge near the calculated feeder origin.");

            // Choose the upper-left valid label corner, restricting the
            // selection to the top group to ignore lower white gaps between bars.
            var firstTop = candidates.Min(c => c.Y);
            var corner = candidates
                .Where(c => c.Y <= firstTop + Math.Max(5, radius / 16))
                .OrderBy(c => c.X)
                .ThenBy(c => c.Y)
                .First();
            return new FeederWhiteCornerObservation(true, corner.X, corner.Y,
                width, height, corner.Area,
                $"WHITE CORNER: pixel ({corner.X}, {corner.Y}), white region {corner.Area} px.");
        }
    }
}
