using System;
using System.Collections.Generic;
using System.Linq;
using LagoVista.Manufacturing.Models;

namespace LagoVista.PickAndPlace.Models
{
    public static class FeederLandmarkGeometry
    {
        public sealed class Dot
        {
            public Dot(double x, double y, double radius) { X=x; Y=y; Radius=radius; }
            public double X { get; }
            public double Y { get; }
            public double Radius { get; }
        }
        // Initial search parameters, not measured physical marker dimensions.
        public static void InitializeProfile(VisionProfile p)
        {
            p.FindCircles=true; p.PerformShapeDetection=true;
            p.FindRectangles=false; p.FindCorners=false; p.FindLines=false;
            p.AutoThreshold=true; p.ApplyThreshold=true; p.Invert=false;
            p.ShowOriginalImage=true; p.LightOn=true; p.TargetImageRadius=4;
            p.HoughCirclesMinRadius=0.2; p.HoughCirclesMaxRadius=3;
            p.StabilizationCount=5; p.ErrorToleranceMM=0.15; p.PixelsPerMM=null;
        }
        private static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
        private static bool Positive(double v) => Finite(v) && v>0;
        private static double Distance(Dot a, Dot b) => Math.Sqrt((a.X-b.X)*(a.X-b.X)+(a.Y-b.Y)*(a.Y-b.Y));
        private static bool Dimension(double value,double? expected,double scale) =>
            !expected.HasValue || (Positive(expected.Value) && Math.Abs(value/scale-expected.Value)<=expected.Value*0.25);

        public static Dot Find(IReadOnlyList<Dot> dots,double x,double y,double radius,double scale,
            double minimumCenterRadiusMm,double maximumCenterRadiusMm,double? centerDiameterMm=null,
            double? sideDiameterMm=null,double? centerToSideSpacingMm=null)
        {
            if (dots==null || dots.Count<3 || dots.Count>64 || !Finite(x) || !Finite(y) ||
                !Positive(radius) || !Positive(scale) || !Positive(minimumCenterRadiusMm) ||
                !Positive(maximumCenterRadiusMm) || maximumCenterRadiusMm<=minimumCenterRadiusMm) return null;
            var valid=dots.Where(p=>p!=null && Finite(p.X) && Finite(p.Y) && Positive(p.Radius)).ToArray();
            Dot result=null;
            foreach(var center in valid)
            {
                var dx=center.X-x; var dy=center.Y-y;
                if(dx*dx+dy*dy>radius*radius || center.Radius/scale<minimumCenterRadiusMm ||
                    center.Radius/scale>maximumCenterRadiusMm || !Dimension(center.Radius*2,centerDiameterMm,scale)) continue;
                var sides=valid.Where(p=>!ReferenceEquals(p,center) && center.Radius/p.Radius>=1.45 &&
                    center.Radius/p.Radius<=2.7 && Dimension(p.Radius*2,sideDiameterMm,scale)).ToArray();
                for(int a=0;a<sides.Length;a++)
                for(int b=a+1;b<sides.Length;b++)
                {
                    var left=sides[a]; var right=sides[b];
                    if(Math.Max(left.Radius,right.Radius)/Math.Min(left.Radius,right.Radius)>1.35) continue;
                    var da=Distance(left,center); var db=Distance(right,center);
                    if(Math.Min(da,db)<center.Radius*3 || Math.Max(da,db)>center.Radius*8 ||
                        Math.Max(da,db)/Math.Min(da,db)>1.20 ||
                        !Dimension(da,centerToSideSpacingMm,scale) || !Dimension(db,centerToSideSpacingMm,scale)) continue;
                    var mx=(left.X+right.X)/2-center.X; var my=(left.Y+right.Y)/2-center.Y;
                    if(Math.Sqrt(mx*mx+my*my)>0.12*(da+db)/2) continue;
                    if(result!=null) return null;
                    result=center;
                }
            }
            return result;
        }
    }
}
