using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederWhiteCornerFinderTests
    {
        private const int Size = 640;

        private static byte[] PrintedWhiteLabel(bool includeCorner = true)
        {
            var pixels = new byte[Size * Size]; // Black feeder/background.

            if (!includeCorner)
                return pixels;

            // Rectangular white label with a rough/slanted leading cut:
            // top begins at (288, 280), while its far-left edge below the
            // header begins at x=280. We want the VISIBLE white top-left,
            // not the globally leftmost pixel farther down the label.
            for (var y = 280; y < Size; y++)
            {
                var left = y < 292 ? 288 : 280;
                for (var x = left; x < 440; x++)
                    pixels[y * Size + x] = 255;
            }

            // Tall black 5x start bar just inside the white margins.
            for (var y = 333; y < 475; y++)
                for (var x = 308; x < 428; x++)
                    pixels[y * Size + x] = 0;

            // A smaller data bar to ensure the white region stays L-shaped.
            for (var y = 500; y < 535; y++)
                for (var x = 308; x < 428; x++)
                    pixels[y * Size + x] = 0;

            return pixels;
        }

        private static FeederWhiteCornerObservation Find(byte[] pixels,
            int radius = 240, byte threshold = 128)
        {
            return FeederWhiteCornerFinder.Find(Size, Size,
                (x, y) => pixels[y * Size + x], threshold, radius);
        }

        [Test]
        public void FindsPrintedWhiteTopLeftNotTheFirstBlackBarcodeBlock()
        {
            var pixels = PrintedWhiteLabel();
            var result = Find(pixels);

            Assert.That(result.Found, Is.True, result.Status);
            Assert.That(result.X, Is.EqualTo(288));
            Assert.That(result.Y, Is.EqualTo(280));
            Assert.That(result.ComponentAreaPixels, Is.GreaterThan(1000));
            Assert.That(result.X, Is.Not.EqualTo(308));
            Assert.That(result.Y, Is.Not.EqualTo(333));
        }

        [Test]
        public void OnePixelThresholdNoiseMustNotDisplaceTheCutCorner()
        {
            var pixels = PrintedWhiteLabel();
            // Isolated bright speckles on the black feeder must not be
            // mistaken for the label's white L-shaped quiet zone.
            for (var y = 175; y < 270; y += 11)
                for (var x = 145; x < 370; x += 17)
                    pixels[y * Size + x] = 255;

            var result = Find(pixels);
            Assert.That(result.Found, Is.True, result.Status);
            Assert.That(result.X, Is.EqualTo(288));
            Assert.That(result.Y, Is.EqualTo(280));
        }

        [Test]
        public void NoWhiteCornerCannotAuthorizeJog()
        {
            var result = Find(PrintedWhiteLabel(false));
            Assert.That(result.Found, Is.False);
            Assert.That(result.Status, Does.Contain("NOT FOUND"));
        }

        [Test]
        public void ClippedUpperBoundaryIsNotAcceptedAsACompleteCorner()
        {
            var pixels = new byte[Size * Size];
            // Bright sheet starts above the search region. A visible
            // interior must not be interpreted as its true cut corner.
            for (var y = 40; y < Size; y++)
                for (var x = 290; x < 435; x++)
                    pixels[y * Size + x] = 255;

            var result = Find(pixels, radius: 190);
            Assert.That(result.Found, Is.False);
        }

        [Test]
        public void CameraThresholdIsAppliedWithoutChangingRawPixels()
        {
            var pixels = PrintedWhiteLabel();
            for (var i = 0; i < pixels.Length; i++)
                if (pixels[i] == 255)
                    pixels[i] = 206;

            var result = Find(pixels, threshold: 180);
            Assert.That(result.Found, Is.True, result.Status);
            Assert.That(result.X, Is.EqualTo(288));
            Assert.That(result.Y, Is.EqualTo(280));
            Assert.That(pixels[280 * Size + 288], Is.EqualTo(206));
        }
    }
}
