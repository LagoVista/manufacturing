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
        public string Status { get; }

        public FeederWhiteCornerObservation(bool found, int x, int y,
            int frameWidth, int frameHeight, int componentAreaPixels, string status)
        {
            Found = found;
            X = x;
            Y = y;
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
            ComponentAreaPixels = componentAreaPixels;
            Status = status;
        }
    }

    /// <summary>
    /// Finds the first high-contrast white cut edge in a bounded region around
    /// the calculated feeder origin. Uses binary pixels and 4-connected runs,
    /// not a general Harris/contour search or barcode symbol recognition.
    /// </summary>
    public static class FeederWhiteCornerFinder
    {
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
                var rowMinimumX = new int[roiHeight];
                for (var i = 0; i < rowMinimumX.Length; i++)
                    rowMinimumX[i] = int.MaxValue;

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
                    rowMinimumX[y] = Math.Min(rowMinimumX[y], x);

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

                var topXs = new List<int>();
                for (var y = minY; y < Math.Min(roiHeight, minY + 6); y++)
                    if (rowMinimumX[y] != int.MaxValue)
                        topXs.Add(rowMinimumX[y]);
                if (topXs.Count < 3)
                    continue;

                topXs.Sort();
                var xPixel = left + topXs[topXs.Count / 2];
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
