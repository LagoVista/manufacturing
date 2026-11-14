using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class DarkCircularOpeningLocatorTests
    {
        private static byte[,,] MakeScene(int w = 160, int h = 160,
            int x = 89, int y = 74, int radius = 5, bool addOuterRing = true)
        {
            var frame = new byte[h, w, 1];
            for (var iy = 0; iy < h; iy++)
                for (var ix = 0; ix < w; ix++)
                {
                    var d = Math.Sqrt((ix - x) * (ix - x) + (iy - y) * (iy - y));
                    frame[iy, ix, 0] = d <= radius ? (byte)22
                        : d <= radius * 2.0 ? (byte)226
                        : addOuterRing && d >= 20 && d <= 25 ? (byte)130
                        : (byte)182;
                }
            return frame;
        }

        [Test]
        public void FindsDarkNozzleOpeningNotItsLargerConcentricRing()
        {
            var found = DarkCircularOpeningLocator.Find(
                MakeScene(), 160, 160, 50, 1.5);
            Assert.That(found, Is.Not.Null);
            Assert.That(found.X, Is.EqualTo(89).Within(1.5));
            Assert.That(found.Y, Is.EqualTo(74).Within(1.5));
            Assert.That(found.DiameterMm, Is.InRange(0.15, 0.30));
            Assert.That(found.Contrast, Is.GreaterThan(35));
        }

        [Test]
        public void NoOpeningMeansNoLock()
        {
            var flat = new byte[160,160,1];
            for (var y = 0; y < 160; y++)
                for (var x = 0; x < 160; x++)
                    flat[y,x,0] = 183;
            Assert.That(DarkCircularOpeningLocator.Find(flat,160,160,50,1.5),
                Is.Null);
        }

        [Test]
        public void LockedDiameterRejectsWrongConcentricFeature()
        {
            Assert.That(DarkCircularOpeningLocator.Find(
                MakeScene(),160,160,50,1.5,1.2), Is.Null);
        }

        [Test]
        public void RepeatableFramesEarnLockButJumpingSamplesDoNot()
        {
            var observations = new List<DarkCircularOpeningLocator.Observation>();
            for (var n = 0; n < 24; n++)
            {
                var frame = MakeScene(x:89 + n % 2, y:74);
                observations.Add(DarkCircularOpeningLocator.Find(frame,160,160,50,1.5));
            }
            Assert.That(DarkCircularOpeningLocator.TryValidate(
                observations,50,out var result,out var reason), Is.True, reason);
            Assert.That(result.GoodFrames, Is.EqualTo(24));

            observations[4] = new DarkCircularOpeningLocator.Observation {
                X = 110, Y = 74, DiameterMm = 0.2, Contrast = 140
            };
            Assert.That(DarkCircularOpeningLocator.TryValidate(
                observations,50,out _,out reason), Is.False);
            Assert.That(reason, Does.Contain("moved"));
        }
    }
}
