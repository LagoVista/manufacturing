// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0c8444eb4739cc2074164ed1703c076a6ce58a17b8b23275e80c6aecd7f96e9a
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using NUnit.Framework;
using System;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class StagePlateTests
    {
        MachineStagingPlate _plate;

        [SetUp]
        public void Setup()
        {
            _plate = new Manufacturing.Models.MachineStagingPlate()
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("B", "B", "B"),
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(100, 100),
            };
        }

        [Test]
        public void ResolveRow()
        {
            var y = StagingPlateUtils.RowToY(_plate, "G");
            Console.WriteLine(y);
            Assert.That(y.Equals(7 * 30 / 2));

            y = StagingPlateUtils.RowToY(_plate, "A");
            Console.WriteLine(y);
            Assert.That(y.Equals(1 * 30 / 2));
        }

        [Test]
        public void ResolveCols()
        {
            var x = StagingPlateUtils.ColToX(_plate, "1");
            Console.WriteLine(x);
            Assert.That(x.Equals(1 * 30 / 2));

            x = StagingPlateUtils.ColToX(_plate, "10");
            Console.WriteLine(x);
            Assert.That(x.Equals(10 * 30 / 2));
        }


        [Test]
        public void ResolveXY() {
            var location = StagingPlateUtils.ResolveStagePlateLocation(_plate, EntityHeader.Create("10", "10", "10"), EntityHeader.Create("B", "B", "B"));
            Console.WriteLine(location); 
            Assert.That(location.X.Equals(150));
            Assert.That(location.Y.Equals(30));
        }

        [TestCase(0.0, 1.0)]
        [TestCase(3.0, 1.0)]
        [TestCase(-4.5, 1.0)]
        [TestCase(12.0, 1.002)]
        [TestCase(-7.0, 0.998)]
        public void ResolveAllMountingHolesFromTwoReferences(double rotationDegrees, double scale)
        {
            // A1 and S24 are 23 columns and 18 rows apart on the 15 mm grid.
            _plate.ReferenceHoleColumn1 = EntityHeader.Create("1", "1", "1");
            _plate.ReferenceHoleRow1 = EntityHeader.Create("A", "A", "A");
            _plate.ReferenceHoleColumn2 = EntityHeader.Create("24", "24", "24");
            _plate.ReferenceHoleRow2 = EntityHeader.Create("S", "S", "S");

            var radians = rotationDegrees * Math.PI / 180.0;
            var cosine = Math.Cos(radians) * scale;
            var sine = Math.Sin(radians) * scale;

            // Independent known physical transform used only to generate test measurements.
            Func<double, double, Core.Models.Drawing.Point2D<double>> measured = (x, y) =>
                new Core.Models.Drawing.Point2D<double>(
                    100 + cosine * x - sine * y,
                    80 + sine * x + cosine * y);

            _plate.ReferenceHoleLocation1 = measured(15, 15);
            _plate.ReferenceHoleLocation2 = measured(360, 285);

            double maximumError = 0;
            for (var rowIndex = 0; rowIndex < 19; rowIndex++)
            {
                var row = ((char)('A' + rowIndex)).ToString();
                for (var column = 1; column <= 24; column++)
                {
                    var expected = measured(column * 15, (rowIndex + 1) * 15);
                    var actual = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(
                        _plate, column.ToString(), row);

                    var error = Math.Sqrt(Math.Pow(actual.X - expected.X, 2) +
                                          Math.Pow(actual.Y - expected.Y, 2));
                    maximumError = Math.Max(maximumError, error);

                    Assert.That(error, Is.LessThan(0.00000001),
                        $"Hole {row}{column} at {rotationDegrees} degrees, scale {scale}");
                }
            }

            TestContext.WriteLine($"Verified 456 holes at {rotationDegrees} degrees, scale {scale}. Maximum error: {maximumError} mm.");
        }

        [Test]
        public void ChangingReferenceFromA3ToC3KeepsPreviousAlignment()
        {
            var previous = new MachineStagingPlate
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("A", "A", "A"),
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(13.8, 118),
                ReferenceHoleColumn2 = EntityHeader.Create("23", "23", "23"),
                ReferenceHoleRow2 = EntityHeader.Create("Q", "Q", "Q"),
                ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(316.5, 354.3)
            };
            var expectedC3 = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(previous, "3", "C");
            var expectedO3 = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(previous, "3", "O");

            var updated = new MachineStagingPlate
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("C", "C", "C"),
                // Form still has the old A3 XY when only its row changes.
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(13.8, 118),
                ReferenceHoleColumn2 = EntityHeader.Create("23", "23", "23"),
                ReferenceHoleRow2 = EntityHeader.Create("Q", "Q", "Q"),
                ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(316.5, 354.3)
            };

            StagingPlateUtils.RebaseChangedReferenceHoles(previous, updated);

            Assert.That(updated.ReferenceHoleLocation1.X, Is.EqualTo(expectedC3.X).Within(0.000001));
            Assert.That(updated.ReferenceHoleLocation1.Y, Is.EqualTo(expectedC3.Y).Within(0.000001));
            Assert.That(updated.ReferenceHoleLocation2.X, Is.EqualTo(316.5));
            Assert.That(updated.ReferenceHoleLocation2.Y, Is.EqualTo(354.3));

            // Switching the reference is a reparameterization, not a recalibration.
            var actualO3 = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(updated, "3", "O");
            Assert.That(actualO3.X, Is.EqualTo(expectedO3.X).Within(0.000001));
            Assert.That(actualO3.Y, Is.EqualTo(expectedO3.Y).Within(0.000001));
        }

        [Test]
        public void ChangingBothReferencesProjectsBothFromOriginalAlignment()
        {
            var previous = new MachineStagingPlate
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("A", "A", "A"),
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(13.8, 118),
                ReferenceHoleColumn2 = EntityHeader.Create("23", "23", "23"),
                ReferenceHoleRow2 = EntityHeader.Create("Q", "Q", "Q"),
                ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(316.5, 354.3)
            };
            var expectedC3 = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(previous, "3", "C");
            var expectedP23 = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(previous, "23", "P");

            var updated = new MachineStagingPlate
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("C", "C", "C"),
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(13.8, 118),
                ReferenceHoleColumn2 = EntityHeader.Create("23", "23", "23"),
                ReferenceHoleRow2 = EntityHeader.Create("P", "P", "P"),
                ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(316.5, 354.3)
            };
            StagingPlateUtils.RebaseChangedReferenceHoles(previous, updated);

            Assert.That(updated.ReferenceHoleLocation1.X, Is.EqualTo(expectedC3.X).Within(0.000001));
            Assert.That(updated.ReferenceHoleLocation1.Y, Is.EqualTo(expectedC3.Y).Within(0.000001));
            Assert.That(updated.ReferenceHoleLocation2.X, Is.EqualTo(expectedP23.X).Within(0.000001));
            Assert.That(updated.ReferenceHoleLocation2.Y, Is.EqualTo(expectedP23.Y).Within(0.000001));
        }

        [Test]
        public void ExplicitlyMeasuredReferenceCoordinatesAreNotOverwritten()
        {
            var previous = new MachineStagingPlate
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("A", "A", "A"),
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(13.8, 118),
                ReferenceHoleColumn2 = EntityHeader.Create("23", "23", "23"),
                ReferenceHoleRow2 = EntityHeader.Create("Q", "Q", "Q"),
                ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(316.5, 354.3)
            };
            var updated = new MachineStagingPlate
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("C", "C", "C"),
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(14.06, 148.28),
                ReferenceHoleColumn2 = EntityHeader.Create("23", "23", "23"),
                ReferenceHoleRow2 = EntityHeader.Create("Q", "Q", "Q"),
                ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(316.5, 354.3)
            };
            StagingPlateUtils.RebaseChangedReferenceHoles(previous, updated);

            Assert.That(updated.ReferenceHoleLocation1.X, Is.EqualTo(14.06));
            Assert.That(updated.ReferenceHoleLocation1.Y, Is.EqualTo(148.28));
        }

        [Test]
        public void RecalibrationOfUnchangedReferencesKeepsNewMeasurements()
        {
            var previous = new MachineStagingPlate
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("C", "C", "C"),
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(14.06, 148.28),
                ReferenceHoleColumn2 = EntityHeader.Create("23", "23", "23"),
                ReferenceHoleRow2 = EntityHeader.Create("Q", "Q", "Q"),
                ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(316.5, 353.9)
            };
            var updated = new MachineStagingPlate
            {
                HoleSpacing = 30,
                ReferenceHoleColumn1 = EntityHeader.Create("3", "3", "3"),
                ReferenceHoleRow1 = EntityHeader.Create("C", "C", "C"),
                ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(14.16, 148.18),
                ReferenceHoleColumn2 = EntityHeader.Create("23", "23", "23"),
                ReferenceHoleRow2 = EntityHeader.Create("Q", "Q", "Q"),
                ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(316.4, 353.9)
            };
            StagingPlateUtils.RebaseChangedReferenceHoles(previous, updated);

            Assert.That(updated.ReferenceHoleLocation1.X, Is.EqualTo(14.16));
            Assert.That(updated.ReferenceHoleLocation1.Y, Is.EqualTo(148.18));
            Assert.That(updated.ReferenceHoleLocation2.X, Is.EqualTo(316.4));
        }

        [Test]
        public void DefaultMachineFiducialGridPositionIsF13()
        {
            Assert.That(_plate.FiducialRow.Id, Is.EqualTo("F"));
            Assert.That(_plate.FiducialColumn.Id, Is.EqualTo("13"));
        }

        [Test]
        public void FirstReferenceApproachUsesFiducialNotPreviousReferenceCoordinates()
        {
            // At 15 mm pitch, C3 is 10 columns left and 3 rows below F13.
            var fiducial = new Core.Models.Drawing.Point2D<double>(300, 200);
            _plate.ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(100, 100);
            _plate.ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(400, 400);

            var target = StagingPlateUtils.ResolveStagePlateApproximateWorkSpaceLocation(
                _plate, fiducial, _plate.FiducialColumn.Id, _plate.FiducialRow.Id, "3", "C");
            Assert.That(target.X, Is.EqualTo(150).Within(0.000001));
            Assert.That(target.Y, Is.EqualTo(155).Within(0.000001));

            // A stale or invalid previous alignment must not move the approach target.
            _plate.ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(999, 999);
            var unchanged = StagingPlateUtils.ResolveStagePlateApproximateWorkSpaceLocation(
                _plate, fiducial, _plate.FiducialColumn.Id, _plate.FiducialRow.Id, "3", "C");
            Assert.That(unchanged.X, Is.EqualTo(target.X));
            Assert.That(unchanged.Y, Is.EqualTo(target.Y));
        }

        [Test]
        public void SecondReferenceApproachUsesTheNewlyMeasuredFirstHole()
        {
            var measuredC3 = new Core.Models.Drawing.Point2D<double>(151, 156);
            var target = StagingPlateUtils.ResolveStagePlateApproximateWorkSpaceLocation(
                _plate, measuredC3, "3", "C", "23", "O");

            Assert.That(target.X, Is.EqualTo(451).Within(0.000001));
            Assert.That(target.Y, Is.EqualTo(336).Within(0.000001));
        }

        [Test]
        public void ReferenceHoleOrderDoesNotMatter()
        {
            _plate.ReferenceHoleColumn1 = EntityHeader.Create("24", "24", "24");
            _plate.ReferenceHoleRow1 = EntityHeader.Create("S", "S", "S");
            _plate.ReferenceHoleColumn2 = EntityHeader.Create("1", "1", "1");
            _plate.ReferenceHoleRow2 = EntityHeader.Create("A", "A", "A");

            _plate.ReferenceHoleLocation1 = new Core.Models.Drawing.Point2D<double>(460, 365);
            _plate.ReferenceHoleLocation2 = new Core.Models.Drawing.Point2D<double>(115, 95);

            var location = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(_plate, "10", "G");
            Assert.That(location.X, Is.EqualTo(250).Within(0.00000001));
            Assert.That(location.Y, Is.EqualTo(185).Within(0.00000001));
        }
    }
}
