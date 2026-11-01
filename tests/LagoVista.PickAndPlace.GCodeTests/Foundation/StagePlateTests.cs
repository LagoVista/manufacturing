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
