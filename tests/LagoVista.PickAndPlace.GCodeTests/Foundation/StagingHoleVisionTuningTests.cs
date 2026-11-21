using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class StagingHoleVisionTuningTests
    {
        private static VisionProfile ValidProfile() => new VisionProfile
        {
            Key = VisionProfile.VisionProfile_StagingPlateHole,
            TargetImageRadius = 2.5,
            HoughCirclesMinRadius = 0.2,
            HoughCirclesMaxRadius = 4.0,
            ErrorToleranceMM = 0.1,
            StabilizationCount = 5,
            GaussianSigma = 2,
            GausianKernellSize = 3,
            ZoomLevel = 1,
            PerformShapeDetection = true,
            FindCircles = true,
            HoughCirclesParam2 = 550
        };

        [Test]
        public void DefaultOffsetIsOriginalOpticalCenter()
        {
            var pt = StagingHoleSearchRegion.Center(480, 480, 100, 0, 0);
            Assert.Multiple(() => {
                Assert.That(pt.X, Is.EqualTo(240));
                Assert.That(pt.Y, Is.EqualTo(240));
            });
        }

        [Test]
        public void OffsetMovesAcquisitionRegionNotMachineCenter()
        {
            var pt = StagingHoleSearchRegion.Center(480, 480, 100, 0.5, -0.25);
            Assert.Multiple(() => {
                Assert.That(pt.X, Is.EqualTo(290));
                Assert.That(pt.Y, Is.EqualTo(215));
            });
        }

        [Test]
        public void DraftIncludesStagingHoleFieldsAndDoesNotChangeProfileBeforeApply()
        {
            var p = ValidProfile();
            var d = StagingHoleVisionTuning.FromProfile(p);
            d.SearchOffsetXmm = 0.5;
            d.MaximumRadiusMm = 3.5;
            Assert.That(p.StagingSearchOffsetXmm, Is.Zero);
            Assert.That(d.Matches(p), Is.False);
            d.ApplyTo(p);
            Assert.That(p.StagingSearchOffsetXmm, Is.EqualTo(0.5));
            Assert.That(p.HoughCirclesMaxRadius, Is.EqualTo(3.5));
            Assert.That(p.HoughCirclesParam2, Is.EqualTo(550),
                "Hough accumulator is irrelevant to staging centroid and must be untouched.");
        }

        [Test]
        public void SearchMaskMustFitFrameWhenApplied()
        {
            var d = StagingHoleVisionTuning.FromProfile(ValidProfile());
            d.ApplyMask = true;
            d.SearchRadiusMm = 2.5;
            Assert.That(d.FitsFrame(640, 480, 100, out _), Is.False);
            d.SearchRadiusMm = 1;
            Assert.That(d.FitsFrame(640, 480, 100, out _), Is.True);
        }

        [Test]
        public void BlankCameraOverridesInheritFromCamera()
        {
            var p = ValidProfile();
            var d = StagingHoleVisionTuning.FromProfile(p);
            Assert.That(d.Exposure, Is.Null);
            d.Exposure = -9.4;
            d.LightPower = 180;
            d.ApplyTo(p);
            Assert.That(p.Exposure, Is.EqualTo(-9.4));
            Assert.That(p.LightPower, Is.EqualTo(180));
            d.Exposure = null;
            d.LightPower = null;
            d.ApplyTo(p);
            Assert.That(p.Exposure, Is.Null);
            Assert.That(p.LightPower, Is.Null);
        }

        [Test]
        public void InapplicableHoughAndNozzleParametersRemainUnmodified()
        {
            var p = ValidProfile();
            p.HoughCirclesParam1 = 30;
            p.HoughCirclesParam2 = 550;
            p.UseAdaptiveDarkOpening = true;
            var d = StagingHoleVisionTuning.FromProfile(p);
            d.SearchOffsetYmm = 0.3;
            d.ApplyTo(p);
            Assert.Multiple(() => {
                Assert.That(p.HoughCirclesParam1, Is.EqualTo(30));
                Assert.That(p.HoughCirclesParam2, Is.EqualTo(550));
                Assert.That(p.UseAdaptiveDarkOpening, Is.True);
            });
        }

        [Test]
        public void InvalidMaskAndBlurSettingsCannotBeApplied()
        {
            var d = StagingHoleVisionTuning.FromProfile(ValidProfile());
            d.UseBlurredImage = true;
            d.BlurKernelSize = 4;
            Assert.That(d.Validate(out _), Is.False);
            d.BlurKernelSize = 3;
            Assert.That(d.Validate(out _), Is.True);
        }
    }
}
