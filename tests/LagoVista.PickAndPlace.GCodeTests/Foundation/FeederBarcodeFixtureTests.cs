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
                var bars = FeederBarcodeSignal.FindBars(signal, 95.2);
                Assert.That(bars.Count, Is.GreaterThanOrEqualTo(4),
                    "Real vertical barcode should contain multiple fully visible bars.");
                Assert.That(bars.All(bar => bar.StartPixel > 0 &&
                    bar.EndPixel < image.Height), Is.True);
            }
        }
    }
}
