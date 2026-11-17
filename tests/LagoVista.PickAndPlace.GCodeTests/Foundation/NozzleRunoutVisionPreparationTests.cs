using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Drawing;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class NozzleRunoutVisionPreparationTests
    {
        [Test]
        public void ConfiguresDisabledNozzleCircleDetectorWithoutOperatorSetup()
        {
            var profile = new VisionProfile
            {
                Key = VisionProfile.VisionProfile_Nozzle,
                FindCircles = false,
                PerformShapeDetection = false
            };
            Assert.That(NozzleRunoutVisionPreparation.TryEnableCircleDetection(
                profile, out var reason), Is.True, reason);
            Assert.That(profile.FindCircles, Is.True);
            Assert.That(profile.PerformShapeDetection, Is.True);
            Assert.That(profile.LightOn, Is.True);
        }

        [Test]
        public void DoesNotModifyUnrelatedVisionProfile()
        {
            var profile = new VisionProfile
            { Key=VisionProfile.VisionProfile_Defauilt };
            Assert.That(NozzleRunoutVisionPreparation.TryEnableCircleDetection(
                profile, out _),Is.False);
            Assert.That(profile.FindCircles,Is.False);
        }

        [Test]
        public void LockingOneHeadRequiresVerifiedLiveOpticalTune()
        {
            var profile = new VisionProfile { Key=VisionProfile.VisionProfile_Nozzle };
            var tuned = new DarkCircularOpeningLocator.TuneResult
            {
                DiameterMm=0.56, MedianContrast=85, MaxCenterDeviationMm=0.04,
                GoodFrames=23, TotalFrames=24
            };
            Assert.That(NozzleRunoutVisionPreparation.TryLockAdaptiveOpening(
                profile, 12.5, tuned, out var reason),Is.True,reason);
            Assert.That(profile.UseAdaptiveDarkOpening,Is.True);
            Assert.That(profile.ExpectedDarkOpeningDiameterMm,Is.EqualTo(0.56));
            Assert.That(profile.DarkOpeningTunedPixelsPerMM,Is.EqualTo(12.5));

            var circle = new MVLocatedCircle(CameraTypes.PartInspection,
                new Point2D<int>(100,100),12.5,0.1,5,true);
            for(var i=0;i<9;i++)
                circle.Add(new PointF(103,102),3.5f);
            Assert.That(NozzleRunoutVisionPreparation.MatchesLockedOpening(
                profile,12.5,circle),Is.True);

            Assert.That(NozzleRunoutVisionPreparation.MatchesLockedOpening(
                profile,50,circle),Is.False,"Changed camera scale invalidates runout optical lock.");

            var wrongDiameter = new MVLocatedCircle(CameraTypes.PartInspection,
                new Point2D<int>(100,100),12.5,0.1,5,true);
            for(var i=0;i<9;i++)
                wrongDiameter.Add(new PointF(103,102),5.8f);
            Assert.That(NozzleRunoutVisionPreparation.MatchesLockedOpening(
                profile,12.5,wrongDiameter),Is.False,
                "Neighboring concentric ring must not be measured as nozzle runout.");
        }

        [Test]
        public void LargerRightHeadOpeningCanBeLockedForRunout()
        {
            var profile = new VisionProfile { Key = VisionProfile.VisionProfile_Nozzle };
            var tuned = new DarkCircularOpeningLocator.TuneResult
            {
                DiameterMm = 1.376, MedianContrast = 118.5,
                MaxCenterDeviationMm = 0.020, GoodFrames = 24, TotalFrames = 24
            };

            Assert.That(NozzleRunoutVisionPreparation.TryLockAdaptiveOpening(
                profile, 12.5, tuned, out var reason), Is.True, reason);
            Assert.That(profile.ExpectedDarkOpeningDiameterMm, Is.EqualTo(1.376));
            Assert.That(profile.DarkOpeningTunedPixelsPerMM, Is.EqualTo(12.5));
        }

        [Test]
        public void MeasuredThreePointFiveMmOpeningCanBeLockedForHeadTwoRunout()
        {
            var profile = new VisionProfile { Key = VisionProfile.VisionProfile_Nozzle };
            var tuned = new DarkCircularOpeningLocator.TuneResult
            {
                DiameterMm = 3.52, MedianContrast = 92,
                MaxCenterDeviationMm = 0.04, GoodFrames = 24, TotalFrames = 24
            };
            Assert.That(NozzleRunoutVisionPreparation.TryLockAdaptiveOpening(
                profile, 12.5, tuned, out var reason), Is.True, reason);

            var circle = new MVLocatedCircle(CameraTypes.PartInspection,
                new Point2D<int>(100, 100), 12.5, 0.1, 5, true);
            for (var i = 0; i < 9; i++)
                circle.Add(new PointF(100, 100), 22f);
            Assert.That(NozzleRunoutVisionPreparation.MatchesLockedOpening(
                profile, 12.5, circle), Is.True);
        }

        [Test]
        public void RejectedAutoTuneDoesNotEnableAdaptiveDetector()
        {
            var profile = new VisionProfile { Key=VisionProfile.VisionProfile_Nozzle };
            var invalid = new DarkCircularOpeningLocator.TuneResult
            {
                DiameterMm=0.56, MedianContrast=85, MaxCenterDeviationMm=0.3,
                GoodFrames=24, TotalFrames=24
            };
            Assert.That(NozzleRunoutVisionPreparation.TryLockAdaptiveOpening(
                profile,12.5,invalid,out _),Is.False);
            Assert.That(profile.UseAdaptiveDarkOpening,Is.False);
            Assert.That(profile.DarkOpeningTunedPixelsPerMM,Is.Null);
        }

        [Test]
        public void FailsForMissingNozzleProfile()
        {
            Assert.That(NozzleRunoutVisionPreparation.TryEnableCircleDetection(
                null, out _),Is.False);
        }
    }
}
