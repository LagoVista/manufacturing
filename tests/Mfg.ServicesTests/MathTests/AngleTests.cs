// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: cc8b773209754ae93f1688438ef96b1081de8d313dfebe1f4cd2c294b951ae39
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Utils;
using System;

namespace Mfg.ServicesTests.MathTests
{

    public class AngleTests
    {
        [SetUp]
        public void Init()
        {

        }

        [Test]
        public void HorizontalAlignTest()
        {
            var pStart = new Point2D<double>(5, 5);
            var pEnd = new Point2D<double>(10, 5);
            var p1 = new Point2D<double>(7.5, 5);

            var end = MathUtils.CorrectForError(pStart, pEnd, p1);
            Console.WriteLine(end);
            Assert.That(end.X.Equals(7.5));
            Assert.That(end.Y.Equals(5));
        }

        [Test]
        public void ThrirtyDegreeTest()
        {
            var pStart = new Point2D<double>(5, 5);
            var pEnd = new Point2D<double>(10, 5);
            var p1 = new Point2D<double>(7.5, 5);

            var end = MathUtils.CorrectForError(pStart, 30, pEnd);
            Console.WriteLine(end);
            Assert.That(end.X.Equals(9.33));
            Assert.That(end.Y.Equals(7.5));
        }


        [Test]
        public void MinusThrirtyDegreeTest()
        {
            var pStart = new Point2D<double>(5, 5);
            var pEnd = new Point2D<double>(10, 5);
            var p1 = new Point2D<double>(7.5, 5);

            var end = MathUtils.CorrectForError(pStart, -30, pEnd);
            Console.WriteLine(end);
            Assert.That(end.X.Equals(9.33));
            Assert.That(end.Y.Equals(2.5));
        }

        [Test]
        public void FortyFiveDegreeTest()
        {
            var pStart = new Point2D<double>(5, 5);
            var pEnd = new Point2D<double>(10, 5);
            var p1 = new Point2D<double>(7.5, 5);

            var end = MathUtils.CorrectForError(pStart, 45, pEnd);
            Console.WriteLine(end);
            Assert.That(end.X.Equals(8.536));
            Assert.That(end.Y.Equals(8.536));
        }

        [Test]
        public void HorizontalAngleTest()
        {
            var p1 = new Point2D<double>(5, 5);
            var p2 = new Point2D<double>(10, 5);

            var theta = MathUtils.CalculateAngle(p1, p2);
            Assert.That(theta.Equals(0));
        }

        [Test]
        public void VerticalAngleTest()
        {
            var p1 = new Point2D<double>(5, 5);
            var p2 = new Point2D<double>(5, 10);

            var theta = MathUtils.CalculateAngle(p1, p2);
            Assert.That(theta.Equals(90));
        }


        [Test]
        public void FortyFiveAngleTest()
        {
            var p1 = new Point2D<double>(5, 5);
            var p2 = new Point2D<double>(10, 10);

            var theta = MathUtils.CalculateAngle(p1, p2);
            Console.WriteLine(theta);
            Assert.That(theta.Equals(45));
        }
    }
}
