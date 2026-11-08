using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederBarcodeFixtureTests
    {
        [Test]
        public void RealVerticalCameraImageReadsOnlyExpectedCenterBand()
        {
            var path = Path.Combine(AppContext.BaseDirectory, "TestImages",
                "FeederBarcode", "vertical_barcode_01.png");
            Assert.That(File.Exists(path), Is.True, "Missing committed camera test image: " + path);

            using (var image = Image.Load<Rgba32>(path))
            {
                var start = image.Width / 2;
                // Deliberately throw if a future implementation tries to hunt
                // elsewhere in this frame: there are bright distractors on the right.
                var signal = FeederBarcodeSignal.SampleCenterBand(
                    image.Width, image.Height, true, (x, y) =>
                    {
                        if (x < start || x >= start + 10)
                            throw new InvalidOperationException("Barcode sampled outside the expected center band.");
                        var pixel = image[x, y];
                        return (byte)((pixel.R * 299 + pixel.G * 587 + pixel.B * 114) / 1000);
                    });

                Assert.That(signal.Length, Is.EqualTo(image.Height));
                Assert.That(signal.All(value => value == 0 || value == 255), Is.True);

                // Validate raw transitions on the actual camera capture, not an
                // invented all-white synthetic label. Clip-edge runs are ignored.
                // Feeder-origin profile uses roughly 50 px/mm. The first
                // six fully bounded bars in the committed camera image are
                // 01, 10, 00, 11, 10, 01 (four distinguishable widths).
                var bars = FeederBarcodeSignal.FindBars(signal, 50);
                Assert.That(bars.Count, Is.GreaterThanOrEqualTo(6),
                    "Real vertical barcode should contain six fully visible data bars.");
                Assert.That(bars.Take(6).Select(bar => bar.Value).ToArray(),
                    Is.EqualTo(new int?[] { 1, 2, 0, 3, 2, 1 }),
                    "The actual camera pattern must decode deterministically.");
                Assert.That(bars.All(bar => bar.StartPixel > 0 &&
                    bar.EndPixel < image.Height), Is.True);

                // This capture was made from the original prototype sticker:
                // its first bar is not the dedicated 5x start. Do not treat
                // it as the scale reference for a new-format marker.
                var calibrated = FeederBarcodeSignal.ReadFromStart(signal, 50);
                Assert.That(calibrated.HasVerifiedStart, Is.False,
                    "A legacy barcode must not be silently accepted as a verified 5x start.");
            }
        }
    }
}
