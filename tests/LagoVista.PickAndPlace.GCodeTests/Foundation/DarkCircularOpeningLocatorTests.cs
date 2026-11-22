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
        public void RecalibratingPixelsPerMmDoesNotLoseAnOffCenterOpening()
        {
            // A 2 mm ROI shrinks from 100 px to 25 px when camera scale is
            // corrected from 50 to 12.5. The nozzle may still be 39 pixels
            // away during coarse positioning; it must remain discoverable
            // before the optical centering step can move it.
            var scene = MakeScene(x:117, y:94);
            Assert.That(DarkCircularOpeningLocator.EffectiveAcquisitionRadiusPixels(
                160, 160, 12.5, 2.0), Is.GreaterThan(39));
            var calibrated = DarkCircularOpeningLocator.Find(
                scene, 160, 160, 12.5, 2.0);
            Assert.That(calibrated, Is.Not.Null);
            Assert.That(calibrated.X, Is.EqualTo(117).Within(1.5));
            Assert.That(calibrated.Y, Is.EqualTo(94).Within(1.5));
            Assert.That(calibrated.DiameterMm, Is.InRange(0.5, 1.0));
            Assert.That(DarkCircularOpeningLocator.Find(
                scene, 160, 160, 50, 2.0), Is.Not.Null);
        }

        [Test]
        public void TinyFramesStillBoundTheAcquisitionToTheCameraImage()
        {
            Assert.That(DarkCircularOpeningLocator.EffectiveAcquisitionRadiusPixels(
                160, 160, 12.5, 2), Is.LessThan(80));
            Assert.That(DarkCircularOpeningLocator.EffectiveAcquisitionRadiusPixels(
                480, 480, 12.5, 2), Is.EqualTo(72));
            Assert.That(DarkCircularOpeningLocator.EffectiveAcquisitionRadiusPixels(
                480, 480, 50, 2), Is.EqualTo(100));
        }

        // Reproduces the 12.5 px/mm regression: the dark feature covers well
        // under 1% of the ROI and the background is almost a single level.
        // The old 1st-percentile threshold returned no candidates.
        private static byte[,,] MakeSparseNozzleScene(int x = 119, int y = 93,
            int coreRadius = 4, int surrounding = 127)
        {
            var frame = new byte[200, 200, 1];
            for (var iy = 0; iy < 200; iy++)
                for (var ix = 0; ix < 200; ix++)
                {
                    var d = Math.Sqrt((ix - x) * (ix - x) + (iy - y) * (iy - y));
                    frame[iy, ix, 0] = d <= coreRadius ? (byte)27
                        : d <= coreRadius * 2.0 ? (byte)220
                        : (byte)surrounding;
                }
            return frame;
        }

        [Test]
        public void SmallDarkOpeningSurvivesFlatBackgroundAtCalibratedScale()
        {
            var frame = MakeSparseNozzleScene();
            var circle = DarkCircularOpeningLocator.Find(
                frame, 200, 200, 12.5, 2.0, 0.59);
            Assert.That(circle, Is.Not.Null);
            Assert.That(circle.X, Is.EqualTo(119).Within(1));
            Assert.That(circle.Y, Is.EqualTo(93).Within(1));
            Assert.That(circle.DiameterMm, Is.InRange(0.48, 0.78));
            Assert.That(circle.Contrast, Is.GreaterThan(35));
        }

        [Test]
        public void SlightLightingAndPositionChangesStillFindSameSmallOpening()
        {
            var observations = new List<DarkCircularOpeningLocator.Observation>();
            for (var i = 0; i < 24; i++)
            {
                var frame = MakeSparseNozzleScene(
                    x:119 + (i % 2), y:93, surrounding:120 + (i % 5));
                observations.Add(DarkCircularOpeningLocator.Find(
                    frame, 200, 200, 12.5, 2.0, 0.59));
            }
            Assert.That(DarkCircularOpeningLocator.TryValidate(
                observations, 12.5, out var result, out var reason), Is.True, reason);
            Assert.That(result.GoodFrames, Is.EqualTo(24));
        }

        private static byte[,,] MakeLargeNozzleScene(int x, int y, int coreRadius)
        {
            var frame = new byte[200, 200, 1];
            for (var iy = 0; iy < 200; iy++)
                for (var ix = 0; ix < 200; ix++)
                {
                    var dx = ix - x;
                    var dy = iy - y;
                    var distance = Math.Sqrt(dx * dx + dy * dy);
                    frame[iy, ix, 0] = distance <= coreRadius ? (byte)24
                        : distance <= coreRadius * 2.0 ? (byte)224
                        : (byte)128;
                }
            return frame;
        }

        [Test]
        public void LargerRightHeadOpeningSurvivesOnePixelDiameterVariation()
        {
            var observations = new List<DarkCircularOpeningLocator.Observation>();
            for (var i = 0; i < 24; i++)
            {
                // At 12.5 px/mm, a one-pixel radius change shifts measured
                // diameter by ~0.16 mm. The prior 1.40 mm ceiling rejected
                // most frames around Head 2's 1.376 mm tuned opening.
                var radius = i % 2 == 0 ? 9 : 10;
                var frame = MakeLargeNozzleScene(100 + (i % 2), 100, radius);
                var found = DarkCircularOpeningLocator.Find(
                    frame, 200, 200, 12.5, 2.0, 1.376);
                Assert.That(found, Is.Not.Null, $"Frame {i} should retain the larger opening.");
                Assert.That(found.DiameterMm, Is.GreaterThan(1.40).And.LessThanOrEqualTo(1.70));
                observations.Add(found);
            }

            Assert.That(DarkCircularOpeningLocator.TryValidate(
                observations, 12.5, out var tune, out var reason), Is.True, reason);
            Assert.That(tune.GoodFrames, Is.EqualTo(24));
            Assert.That(tune.DiameterMm, Is.GreaterThan(1.40).And.LessThanOrEqualTo(1.70));
        }

        [Test]
        public void RejectsOpeningAboveTheSupervisedFourMmAcquisitionCeiling()
        {
            // A 26 px radius at 12.5 px/mm is greater than 4.0 mm.
            var frame = MakeLargeNozzleScene(100, 100, 26);
            Assert.That(DarkCircularOpeningLocator.Find(
                frame, 200, 200, 12.5, 2.0), Is.Null);
        }

        [Test]
        public void AcquiresHeadTwoOpeningAroundThreePointFiveMm()
        {
            // Reproduce the 3.52 mm candidate reported by actual Head 2
            // diagnostics. Real camera acceptance still requires hardware.
            var observations = new List<DarkCircularOpeningLocator.Observation>();
            for (var i = 0; i < 24; i++)
            {
                var frame = MakeLargeNozzleScene(112 + i % 2, 94, 22);
                var diagnostic = new DarkCircularOpeningLocator.RejectionDiagnostics();
                var found = DarkCircularOpeningLocator.Find(
                    frame, 200, 200, 12.5, 2.0, 0, diagnostic);
                Assert.That(found, Is.Not.Null, diagnostic.PrimaryReason);
                Assert.That(found.X, Is.EqualTo(112 + i % 2).Within(1));
                Assert.That(found.Y, Is.EqualTo(94).Within(1));
                Assert.That(found.DiameterMm, Is.InRange(3.3, 3.8));
                observations.Add(found);
            }
            Assert.That(DarkCircularOpeningLocator.TryValidate(
                observations, 12.5, out var tune, out var reason), Is.True, reason);
            Assert.That(tune.GoodFrames, Is.EqualTo(24));
        }

        private static byte[,,] MakeReflectiveWideNozzleScene()
        {
            // Two circular dark objects, but only the larger nozzle opening
            // has a bright annulus over most angles. A small reflective speck
            // has a very bright, ONE-SIDED arc that used to dominate the mean
            // ring-contrast score even though it was not the nozzle opening.
            var frame = new byte[200, 200, 1];
            for (var y = 0; y < 200; y++)
                for (var x = 0; x < 200; x++)
                {
                    var nx = x - 121;
                    var ny = y - 88;
                    var nd = Math.Sqrt(nx * nx + ny * ny);
                    var na = Math.Atan2(ny, nx);
                    if (na < 0) na += 2 * Math.PI;
                    var sx = x - 90;
                    var sy = y - 112;
                    var sd = Math.Sqrt(sx * sx + sy * sy);
                    var sa = Math.Atan2(sy, sx);
                    if (sa < 0) sa += 2 * Math.PI;
                    frame[y, x, 0] = nd <= 9 || sd <= 4 ? (byte)10
                        : nd <= 18 && na < 2 * Math.PI * 0.67 ? (byte)145
                        : sd <= 8 && sa < 2 * Math.PI * 0.42 ? (byte)255
                        : (byte)40;
                }
            return frame;
        }

        [Test]
        public void WideNozzleWinsAgainstOneSidedReflectiveDecoy()
        {
            var found = DarkCircularOpeningLocator.Find(
                MakeReflectiveWideNozzleScene(), 200, 200, 12.5, 2.0);
            Assert.That(found, Is.Not.Null);
            Assert.That(found.X, Is.EqualTo(121).Within(1));
            Assert.That(found.Y, Is.EqualTo(88).Within(1));
            Assert.That(found.DiameterMm, Is.InRange(1.3, 1.7));
        }

        [Test]
        public void SparseBrightRingOnDarkFieldIsNotLostAt80thPercentile()
        {
            // Less than 20% of the search region is bright. Previously the
            // 80th-percentile "high" matched the dark background, leaving no
            // thresholds despite an unmistakable central dark opening.
            var frame = new byte[200, 200, 1];
            for (var y = 0; y < 200; y++)
                for (var x = 0; x < 200; x++)
                {
                    var dx = x - 113;
                    var dy = y - 96;
                    var d = Math.Sqrt(dx * dx + dy * dy);
                    frame[y, x, 0] = d <= 9 ? (byte)8
                        : d <= 18 ? (byte)180 : (byte)18;
                }
            var found = DarkCircularOpeningLocator.Find(
                frame, 200, 200, 12.5, 2.0);
            Assert.That(found, Is.Not.Null);
            Assert.That(found.X, Is.EqualTo(113).Within(1));
            Assert.That(found.Y, Is.EqualTo(96).Within(1));
            Assert.That(found.DiameterMm, Is.InRange(1.3, 1.7));
        }

        [Test]
        public void SwitchingHeadsDoesNotInheritOtherNozzlesSavedOpeningDiameter()
        {
            var profile = new VisionProfile {
                ExpectedDarkOpeningDiameterMm = 0.36,
                DarkOpeningTunedPixelsPerMM = 12.5
            };
            var state = new NozzleOpeningHeadContext();

            // The shared saved 0.36 mm has no known head ownership yet.
            Assert.That(state.Resolve("1:nozzle-small", profile, 12.5,
                out var switched), Is.Zero);
            Assert.That(switched, Is.True);
            Assert.That(state.Resolve("1:nozzle-small", profile, 12.5,
                out switched), Is.Zero);
            Assert.That(switched, Is.False);

            // Explicit fresh tuning belongs to the selected head.
            profile.ExpectedDarkOpeningDiameterMm = 0.37;
            Assert.That(state.Resolve("1:nozzle-small", profile, 12.5,
                out switched), Is.EqualTo(0.37).Within(0.001));

            // Switching to Head 2 must not apply Head 1's 0.37 mm gate.
            Assert.That(state.Resolve("2:nozzle-wide", profile, 12.5,
                out switched), Is.Zero);
            Assert.That(switched, Is.True);
            profile.ExpectedDarkOpeningDiameterMm = 1.376;
            Assert.That(state.Resolve("2:nozzle-wide", profile, 12.5,
                out switched), Is.EqualTo(1.376).Within(0.001));

            // Returning to either nozzle uses its own in-session tuning,
            // without rewriting the camera-owned vision profile.
            Assert.That(state.Resolve("1:nozzle-small", profile, 12.5,
                out switched), Is.EqualTo(0.37).Within(0.001));
            Assert.That(state.Resolve("2:nozzle-wide", profile, 12.5,
                out switched), Is.EqualTo(1.376).Within(0.001));
            Assert.That(profile.ExpectedDarkOpeningDiameterMm,
                Is.EqualTo(1.376).Within(0.001));
        }

        [Test]
        public void InvalidCameraScaleOrProfileReplacementCannotClaimHeadTuning()
        {
            var profile = new VisionProfile {
                ExpectedDarkOpeningDiameterMm = 0.36,
                DarkOpeningTunedPixelsPerMM = 12.5
            };
            var state = new NozzleOpeningHeadContext();
            state.Resolve("1:nozzle-small", profile, 12.5, out _);
            profile.ExpectedDarkOpeningDiameterMm = 0.37;
            Assert.That(state.Resolve("1:nozzle-small", profile, 12.5,
                out _), Is.EqualTo(0.37).Within(0.001));

            // Loading a different camera-owned Nozzle profile is not proof
            // of a new nozzle tune.
            var replacement = new VisionProfile {
                ExpectedDarkOpeningDiameterMm = 1.376,
                DarkOpeningTunedPixelsPerMM = 12.5
            };
            Assert.That(state.Resolve("1:nozzle-small", replacement, 12.5,
                out var changed), Is.EqualTo(0.37).Within(0.001));
            Assert.That(changed, Is.False);
            Assert.That(state.Resolve("2:nozzle-wide", replacement, 12.5,
                out changed), Is.Zero);
            Assert.That(changed, Is.True);

            replacement.ExpectedDarkOpeningDiameterMm = 1.38;
            Assert.That(state.Resolve("2:nozzle-wide", replacement, 12.5,
                out _), Is.EqualTo(1.38).Within(0.001));

            // Camera scale changes invalidate physical diameter associations.
            Assert.That(state.Resolve("1:nozzle-small", replacement, 15,
                out _), Is.Zero);
            Assert.That(state.Resolve("2:nozzle-wide", replacement, 15,
                out _), Is.Zero);
        }

        [Test]
        public void OversizeWideNozzleReportsThePhysicalLimitWithoutLocking()
        {
            // This verifies that diagnostics explain a genuinely annular
            // circular opening beyond the supervised 4 mm ceiling.
            var diagnostic = new DarkCircularOpeningLocator.RejectionDiagnostics();
            var frame = MakeLargeNozzleScene(100, 100, 26);
            var circle = DarkCircularOpeningLocator.Find(
                frame, 200, 200, 12.5, 2.0, 0, diagnostic);
            Assert.That(circle, Is.Null);
            Assert.That(diagnostic.RejectedTooLarge, Is.GreaterThan(0));
            Assert.That(diagnostic.LargestOversizeDiameterMm,
                Is.GreaterThan(DarkCircularOpeningLocator.MaximumOpeningDiameterMm));
            Assert.That(diagnostic.PrimaryReason, Does.Contain("OVERSIZE"));
        }

        // The N24 real camera can show a narrow black seam through the
        // reflective white metal ring. Without bounded repair, the dark core
        // floods into the background and the locator returns ROI EDGE instead
        // of the correct, previously measured 1.525 mm opening.
        private static byte[,,] MakeSeamedReflectiveNozzle(int seamHalfWidth)
        {
            var pixels = new byte[200, 200, 1];
            for (var y = 0; y < 200; y++)
                for (var x = 0; x < 200; x++)
                {
                    var dx = x - 105;
                    var dy = y - 96;
                    var d = Math.Sqrt(dx * dx + dy * dy);
                    var seam = dx >= 0 && Math.Abs(dy) <= seamHalfWidth;
                    pixels[y, x, 0] = d <= 9 ? (byte)8
                        : d <= 21 && !seam ? (byte)205 : (byte)18;
                }
            return pixels;
        }

        [Test]
        public void LockedNozzleMayBridgeOnePixelReflectiveRingSeamButKeepsDiameterProof()
        {
            var frame = MakeSeamedReflectiveNozzle(0);
            var raw = DarkCircularOpeningLocator.Find(frame, 200, 200,
                12.5, 8.2, expectedDiameterMm:1.525,
                allowTinyRingGapRepair:false);
            Assert.That(raw, Is.Null,
                "The small seam connects the dark core to the ROI exterior.");
            var diagnostics = new DarkCircularOpeningLocator.RejectionDiagnostics();
            var repaired = DarkCircularOpeningLocator.Find(frame, 200, 200,
                12.5, 8.2, expectedDiameterMm:1.525, diagnostics:diagnostics);
            Assert.That(repaired, Is.Not.Null, diagnostics.PrimaryReason);
            Assert.That(diagnostics.TinyRingGapRepairUsed, Is.True);
            Assert.That(repaired.X, Is.EqualTo(105).Within(1));
            Assert.That(repaired.Y, Is.EqualTo(96).Within(1));
            Assert.That(repaired.DiameterMm, Is.InRange(1.12, 1.98));
            Assert.That(DarkCircularOpeningLocator.Find(frame, 200, 200,
                12.5, 8.2, expectedDiameterMm:0.55), Is.Null,
                "The repair cannot substitute an incorrectly sized opening.");
        }

        [Test]
        public void LargeMissingRingArcRemainsUnqualifiedEvenWithTinyGapRecovery()
        {
            var damaged = MakeSeamedReflectiveNozzle(6);
            var diagnostics = new DarkCircularOpeningLocator.RejectionDiagnostics();
            Assert.That(DarkCircularOpeningLocator.Find(damaged,
                200, 200, 12.5, 8.2, 1.525, diagnostics), Is.Null,
                "A 13-pixel missing ring band must not be reconstructed.");
            Assert.That(diagnostics.TinyRingGapRepairUsed, Is.False);
        }

        [Test]
        public void ExplicitHeadTuneDoesNotRequireSharedProfileDiameterToChange()
        {
            // A repeat tune can report the same physical diameter as last
            // time; it must still bind explicitly to the selected head.
            var profile = new VisionProfile {
                ExpectedDarkOpeningDiameterMm = 0.36,
                DarkOpeningTunedPixelsPerMM = 12.5
            };
            var context = new NozzleOpeningHeadContext();
            Assert.That(context.Resolve("2:nozzle-wide", profile, 12.5,
                out _), Is.Zero);

            context.RegisterVerifiedTune("2:nozzle-wide", 1.376, 12.5);
            Assert.That(context.Resolve("2:nozzle-wide", profile, 12.5,
                out _), Is.EqualTo(1.376).Within(0.001));
            Assert.That(context.Resolve("1:nozzle-small", profile, 12.5,
                out _), Is.Zero);
            Assert.That(profile.ExpectedDarkOpeningDiameterMm,
                Is.EqualTo(0.36).Within(0.001));
        }

        [Test]
        public void NoisyUniformImageDoesNotPassAsSmallNozzle()
        {
            var frame = new byte[200, 200, 1];
            for (var y = 0; y < 200; y++)
                for (var x = 0; x < 200; x++)
                    frame[y,x,0] = ((x * 73 + y * 101) % 467 == 0)
                        ? (byte)7 : (byte)127;
            Assert.That(DarkCircularOpeningLocator.Find(
                frame, 200, 200, 12.5, 2.0, 0.59), Is.Null);
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
