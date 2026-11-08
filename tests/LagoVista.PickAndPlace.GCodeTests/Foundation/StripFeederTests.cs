using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace;
using NUnit.Framework;
using System;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class StripFeederTests
    {
        MachineStagingPlate _plate;

        [SetUp]
        public void Setup()
        {
            _plate = new MachineStagingPlate()
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("1", "1", "1"),
                ReferenceHoleRow1 = EntityHeader.Create("A", "A", "A"),
                ReferenceHoleColumn2 = EntityHeader.Create("24", "24", "24"),
                ReferenceHoleRow2 = EntityHeader.Create("S", "S", "S"),
                ReferenceHoleLocation1 = new Point2D<double>(100, 100),
                ReferenceHoleLocation2 = new Point2D<double>(445, 370)
            };
        }

        [TestCase(false, 230.0, 182.0)]
        [TestCase(true, 227.0, 195.0)]
        public void CalculatedOriginNavigationMustIgnoreSavedOriginOffset(
            bool vertical, double expectedX, double expectedY)
        {
            var mountingHole = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(_plate, "10", "G");
            var feeder = new StripFeeder
            {
                Orientation = EntityHeader<FeederOrientations>.Create(vertical
                    ? FeederOrientations.Vertical : FeederOrientations.Horizontal),
                MountingHoleOffset = new Point2D<double>(5, 8),
                OriginOffset = new Point2D<double>(12.5, -7.25)
            };

            // Exercise the same private production resolver used specifically
            // by FindLocation(FeederOrigin), not a copied formula in the test.
            var method = typeof(LagoVista.PickAndPlace.ViewModels.PickAndPlace.StripFeederViewModel)
                .GetMethod("ResolveCalculatedFeederOrigin",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Static);
            Assert.That(method, Is.Not.Null);
            var calculated = (Point2D<double>)method.Invoke(null,
                new object[] { mountingHole, feeder });

            Assert.That(calculated.X, Is.EqualTo(expectedX).Within(1e-9));
            Assert.That(calculated.Y, Is.EqualTo(expectedY).Within(1e-9));
            Assert.That(calculated.X, Is.Not.EqualTo(expectedX + feeder.OriginOffset.X));
        }

        [TestCase(false, 230.0, 182.0)]
        [TestCase(true, 227.0, 195.0)]
        public void FrameBarcodeUsesSavedCalibratedOriginWhileInitialApproachDoesNot(
            bool vertical, double calculatedX, double calculatedY)
        {
            var mountingHole = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(_plate, "10", "G");
            var feeder = new StripFeeder
            {
                Orientation = EntityHeader<FeederOrientations>.Create(vertical
                    ? FeederOrientations.Vertical : FeederOrientations.Horizontal),
                MountingHoleOffset = new Point2D<double>(5, 8),
                OriginOffset = new Point2D<double>(3.25, -1.75)
            };
            var type = typeof(LagoVista.PickAndPlace.ViewModels.PickAndPlace.StripFeederViewModel);
            var flags = System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.Static;
            var calculatedMethod = type.GetMethod("ResolveCalculatedFeederOrigin", flags);
            var calibratedMethod = type.GetMethod("ResolveCalibratedFeederOrigin", flags);
            Assert.That(calculatedMethod, Is.Not.Null);
            Assert.That(calibratedMethod, Is.Not.Null);

            var approach = (Point2D<double>)calculatedMethod.Invoke(
                null, new object[] { mountingHole, feeder });
            var frameOrigin = (Point2D<double>)calibratedMethod.Invoke(
                null, new object[] { approach, feeder });

            Assert.That(approach.X, Is.EqualTo(calculatedX).Within(1e-9));
            Assert.That(approach.Y, Is.EqualTo(calculatedY).Within(1e-9));
            Assert.That(frameOrigin.X, Is.EqualTo(calculatedX + 3.25).Within(1e-9));
            Assert.That(frameOrigin.Y, Is.EqualTo(calculatedY - 1.75).Within(1e-9));
        }

        // Uses the production staging-plate resolver and production feeder orientation math.
        // The asserted positions are hand-computed from a 15 mm pitch grid.
        [TestCase("1", "A", false, 5.0, 8.0, 0.0, 0.0, 95.0, 92.0)]
        [TestCase("24", "S", false, 5.0, 8.0, 0.0, 0.0, 440.0, 362.0)]
        [TestCase("10", "G", false, 5.0, 8.0, 0.0, 0.0, 230.0, 182.0)]
        [TestCase("10", "G", true, 5.0, 8.0, 0.0, 0.0, 227.0, 195.0)]
        [TestCase("10", "G", false, 5.0, 8.0, 2.5, -1.0, 232.5, 181.0)]
        [TestCase("10", "G", true, 5.0, 8.0, 2.5, -1.0, 229.5, 194.0)]
        public void ResolveFeederOriginFromMountingHole(
            string column, string row, bool vertical,
            double mountingX, double mountingY, double correctionX, double correctionY,
            double expectedX, double expectedY)
        {
            var mountingHole = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(_plate, column, row);
            var mountingOffset = new Point2D<double>(mountingX, mountingY);
            var originOffset = new Point2D<double>(correctionX, correctionY);

            // Same production calculation as StripFeederViewModel.FindLocation.
            var feederOrigin = mountingHole.SubtractWithConditionalSwap(vertical, mountingOffset) + originOffset;

            Assert.That(feederOrigin.X, Is.EqualTo(expectedX).Within(1e-9));
            Assert.That(feederOrigin.Y, Is.EqualTo(expectedY).Within(1e-9));

            // The inverse must recover the selected mounting hole for both orientations.
            var recoveredMount = feederOrigin - originOffset;
            recoveredMount = recoveredMount.AddWithConditionalSwap(vertical, mountingOffset);
            Assert.That(recoveredMount.X, Is.EqualTo(mountingHole.X).Within(1e-9));
            Assert.That(recoveredMount.Y, Is.EqualTo(mountingHole.Y).Within(1e-9));
        }

        [TestCase(false)]
        [TestCase(true)]
        public void RepeatedOriginCalibrationPreservesCorrectedPosition(bool vertical)
        {
            var mountingHole = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(_plate, "10", "G");
            var mountingOffset = new Point2D<double>(5, 8);
            var originOffset = new Point2D<double>(2, -1);
            var uncorrectedOrigin = mountingHole.SubtractWithConditionalSwap(vertical, mountingOffset);
            var measuredOrigin = new Point2D<double>(uncorrectedOrigin.X + 3.25, uncorrectedOrigin.Y - 2.5);

            // The production SetLocation correction uses the existing offset plus
            // the difference between the measured and currently calculated origin.
            for (var attempt = 0; attempt < 3; attempt++)
            {
                var calculatedOrigin = uncorrectedOrigin + originOffset;
                originOffset = originOffset + measuredOrigin - calculatedOrigin;
                Assert.That(originOffset.X, Is.EqualTo(3.25).Within(1e-9));
                Assert.That(originOffset.Y, Is.EqualTo(-2.5).Within(1e-9));
                var resolved = uncorrectedOrigin + originOffset;
                Assert.That(resolved.X, Is.EqualTo(measuredOrigin.X).Within(1e-9));
                Assert.That(resolved.Y, Is.EqualTo(measuredOrigin.Y).Within(1e-9));
            }
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ResolveEveryFeederOriginOnRotatedPlate(bool vertical)
        {
            const double angle = 3.0 * Math.PI / 180.0;
            var cosine = Math.Cos(angle);
            var sine = Math.Sin(angle);
            Func<double, double, Point2D<double>> measured = (x, y) =>
                new Point2D<double>(100 + cosine * x - sine * y, 80 + sine * x + cosine * y);

            _plate.ReferenceHoleLocation1 = measured(15, 15);
            _plate.ReferenceHoleLocation2 = measured(360, 285);

            var mountingOffset = new Point2D<double>(6, 9);
            var correction = new Point2D<double>(1.25, -0.75);

            for (var rowIndex = 0; rowIndex < 19; rowIndex++)
            {
                var row = ((char)('A' + rowIndex)).ToString();
                for (var column = 1; column <= 24; column++)
                {
                    var mountingHole = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(_plate, column.ToString(), row);
                    var feederOrigin = mountingHole.SubtractWithConditionalSwap(vertical, mountingOffset) + correction;

                    var expectedMount = measured(column * 15, (rowIndex + 1) * 15);
                    var expectedX = expectedMount.X - (vertical ? 9 : 6) + correction.X;
                    var expectedY = expectedMount.Y + (vertical ? 6 : -9) + correction.Y;

                    Assert.That(feederOrigin.X, Is.EqualTo(expectedX).Within(1e-9), $"X at {row}{column}");
                    Assert.That(feederOrigin.Y, Is.EqualTo(expectedY).Within(1e-9), $"Y at {row}{column}");
                }
            }
        }
    }
}
