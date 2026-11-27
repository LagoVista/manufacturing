using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class NozzleRunoutMathTests
    {
        private static NozzleRunoutSample[] Samples(int peakAngle=90) =>
            NozzleRunoutMath.SweepAngles().Select(angle => new NozzleRunoutSample {
                CommandedAngle=angle, PixelsPerMm=90.9,
                Xmm=0.17*Math.Sin((angle-peakAngle)*Math.PI/180),
                Ymm=0.17*Math.Cos((angle-peakAngle)*Math.PI/180),
                CapturedAtUtc=DateTimeOffset.UtcNow
            }).ToArray();

        [Test]
        public void TwelveThirtyDegreePositions() =>
            Assert.That(NozzleRunoutMath.SweepAngles(),Is.EqualTo(
                new[]{0,30,60,90,120,150,180,210,240,270,300,330}));

        [Test]
        public void HighestYDefinesLogicalZeroWithoutChangingFirmware()
        {
            var result=NozzleRunoutMath.Analyze(Samples());
            Assert.That(result.LogicalZeroAngle,Is.EqualTo(90));
            Assert.That(result.MaxRadialDeviationMm,Is.EqualTo(0.17).Within(0.0001));
            Assert.That(result.CenterMm.X,Is.EqualTo(0).Within(0.00001));
        }

        [Test]
        public void IncompleteOrDuplicateDataCannotCalibrate()
        {
            Assert.Throws<ArgumentException>(()=>NozzleRunoutMath.Analyze(Samples().Take(11)));
            var samples=Samples();samples[11].CommandedAngle=0;
            Assert.Throws<ArgumentException>(()=>NozzleRunoutMath.Analyze(samples));
        }

        [Test]
        public void RejectsNonFiniteOrZeroCameraScale()
        {
            var samples=Samples(); samples[5].Xmm=Double.NaN;
            Assert.Throws<ArgumentException>(()=>NozzleRunoutMath.Analyze(samples));
            samples=Samples();samples[4].PixelsPerMm=0;
            Assert.Throws<ArgumentException>(()=>NozzleRunoutMath.Analyze(samples));
        }

        [Test]
        public void CorrectionInterpolatesAcrossWrap()
        {
            var result=NozzleRunoutMath.Analyze(Samples());
            var a=NozzleRunoutMath.Correction(result,330);
            var b=NozzleRunoutMath.Correction(result,0);
            var half=NozzleRunoutMath.Correction(result,345);
            Assert.That(half.X,Is.EqualTo((a.X+b.X)/2).Within(0.00001));
            Assert.That(half.Y,Is.EqualTo((a.Y+b.Y)/2).Within(0.00001));
            Assert.That(NozzleRunoutMath.NormalizeAngle(-15),Is.EqualTo(345));
        }

        [Test]
        public void RepeatedHighPointMustBeWithinTolerance()
        {
            var s=Samples()[3];var another=new NozzleRunoutSample
            {CommandedAngle=90,Xmm=s.Xmm+0.2,Ymm=s.Ymm};
            Assert.That(NozzleRunoutMath.IsRepeatable(s,another,0.1),Is.False);
            another.Xmm=s.Xmm+0.05;
            Assert.That(NozzleRunoutMath.IsRepeatable(s,another,0.1),Is.True);
        }
    }
}
