using LagoVista.PickAndPlace.Models;
using NUnit.Framework;
using System.Linq;

namespace LagoVista.PickAndPlace.GCodeTests.Foundation
{
    public class FeederLandmarkGeometryTests
    {
        private static FeederLandmarkGeometry.Dot D(double x,double y,double r) => new FeederLandmarkGeometry.Dot(x,y,r);
        [Test] public void FindsCenterWithDistractorAndSlightRotation()
        {
            var mid=D(100,100,10);
            Assert.That(FeederLandmarkGeometry.Find(new[]{D(50,98,5),mid,D(150,102,5),D(120,150,6)},100,100,80,20,.2,3),Is.SameAs(mid));
        }
        [Test] public void RejectsMissingSideAndEqualDiameters()
        {
            Assert.That(FeederLandmarkGeometry.Find(new[]{D(50,100,5),D(100,100,10)},100,100,80,20,.2,3),Is.Null);
            Assert.That(FeederLandmarkGeometry.Find(new[]{D(50,100,5),D(100,100,5),D(150,100,5)},100,100,80,20,.2,3),Is.Null);
        }
        [Test] public void RejectsAmbiguousTriplets()
        {
            var dots=new[]{D(50,100,5),D(100,100,10),D(150,100,5),D(50,200,5),D(100,200,10),D(150,200,5)};
            Assert.That(FeederLandmarkGeometry.Find(dots,100,150,100,20,.2,3),Is.Null);
        }
        [Test] public void RejectsBadSpacingOrPhysicalDimensions()
        {
            Assert.That(FeederLandmarkGeometry.Find(new[]{D(50,100,5),D(100,100,10),D(170,100,5)},100,100,80,20,.2,3),Is.Null);
            Assert.That(FeederLandmarkGeometry.Find(new[]{D(50,100,5),D(100,100,10),D(150,100,5)},100,100,80,20,.2,3,2),Is.Null);
        }
    }
}
