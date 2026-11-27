using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Models;
using NUnit.Framework;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class CameraToolRepeatabilityStatisticsTests
    {
        [Test]
        public void SeparatesConstantBiasFromScatterAndDrift()
        {
            var p = new Point2D<double>(-0.08, 0.1);
            var result = CameraToolRepeatabilityStatistics.Summarize(new[] { p, p, p });
            Assert.That(result, Does.Contain("mean XY=(-0.080,0.100)"));
            Assert.That(result, Does.Contain("RMS scatter=0.000"));
            Assert.That(result, Does.Contain("first-to-last XY=(0.000,0.000)"));
        }

        [Test]
        public void ReportsSignedDriftAndSkipsMissingSamples()
        {
            var result = CameraToolRepeatabilityStatistics.Summarize(new[] {
                new Point2D<double>(0.02, 0.04), null, new Point2D<double>(-0.02, 0.06) });
            Assert.That(result, Does.Contain("n=2"));
            Assert.That(result, Does.Contain("first-to-last XY=(-0.040,0.020)"));
            Assert.That(result, Does.Contain("RMS scatter=0.022"));
        }
    }
}
