// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 14a048b6a09013ecbedaa2b4b486af3c858ab52b4f992d5b298111f420c415cb
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV.Structure;
using LagoVista.Core.Models.Drawing;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LagoVista.PickAndPlace
{
    public static class SizeExtensions
    {
        public static Point2D<int> ToPoint2D(this System.Drawing.Size size)
        {
            return new Point2D<int>()
            {
                X = size.Width,
                Y = size.Height
            };
        }

        public static Point2D<float> ToPoint2D(this System.Drawing.PointF size)
        {
            return new Point2D<float>()
            {
                X = size.X,
                Y = size.Y
            };
        }

        public static Point2D<int> Center(this System.Drawing.Size size)
        {
            return new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };
        }

        public static bool IsSimilar(this MVLocatedCircle original, CircleF candidate, int deltaPixel)
        {
            var deltaX = Math.Abs(original.CenterPixels.X - candidate.Center.X);
            if (deltaX > deltaPixel)
                return false;

            var deltaY = Math.Abs(original.CenterPixels.Y - candidate.Center.Y);
            if (deltaY > deltaPixel)
                return false;

            var deltaRadius = Math.Abs(original.RadiusPixels - candidate.Radius);
            if (deltaRadius > deltaPixel)
                return false;

            return true;
        }

        public static MVLocatedCircle FindPrevious(this List<MVLocatedCircle> foundCircles, CircleF circle, int deltaPixel)
        {
            return foundCircles.FirstOrDefault(cir => cir.IsSimilar(circle, deltaPixel));
        }

        public static MVLocatedCircle FindPrevious(this ObservableCollection<MVLocatedCircle> foundCircles, CircleF circle, int deltaPixel)
        {
            return foundCircles.FirstOrDefault(cir => cir.IsSimilar(circle, deltaPixel));
        }

        public static float GetDeltaAngle(this float a1, float a2)
        {
            var delta = Math.Abs(a1 - a2);
            if (delta > 180)
                delta = 360 - delta;
            return delta;
        }

        public static bool IsSimilar(this RotatedRect r1, RotatedRect r2, int deltaPixel)
        {
            var deltaX = Math.Abs(r1.Center.X - r2.Center.X);
            if (deltaX > deltaPixel)
                return false;

            var deltaY = Math.Abs(r1.Center.Y - r2.Center.Y);
            if(deltaY > deltaPixel) 
                return false;

            var deltaLen = Math.Abs(r1.Size.Width - r2.Size.Width);
            if(deltaLen > deltaPixel)
                return false;

            var deltaHeight = Math.Abs(r1.Size.Height - r2.Size.Height);
            if (deltaHeight > deltaPixel)
                return false;

            var deltaAngle = r1.Angle.GetDeltaAngle(r2.Angle);
            if (deltaAngle > 5)
                return false;

            return true;
        }

        public static MVLocatedRectangle FindPrevious(this List<MVLocatedRectangle> foundRects, RotatedRect rect, int deltaPixel)
        {
            return foundRects.FirstOrDefault(cir => cir.RotatedRect.IsSimilar(rect, deltaPixel));
        }

        public static MVLocatedRectangle FindPrevious(this ObservableCollection<MVLocatedRectangle> foundRects, RotatedRect rect, int deltaPixel)
        {
            return foundRects.FirstOrDefault(cir => cir.RotatedRect.IsSimilar(rect, deltaPixel));
        }

        public static bool WithinRadius(this CircleF bounds, CircleF test)
        {
            var distance = Math.Sqrt(Math.Pow(bounds.Center.X - test.Center.X, 2) + Math.Pow(bounds.Center.Y - test.Center.Y, 2));
            return distance + test.Radius < bounds.Radius;
        }

        public static bool WithinBounds(this PointF point, CircleF bounds)
        {
            return point.X >= bounds.Center.X - bounds.Radius && 
                   point.X <= bounds.Center.X + bounds.Radius &&
                   point.Y >= bounds.Center.Y - bounds.Radius &&
                   point.Y <= bounds.Center.Y + bounds.Radius;
        }
        public static bool WithinRadius(this CircleF bounds, RotatedRect rect)
        {
            return rect.GetVertices()[0].WithinBounds(bounds) &&
                   rect.GetVertices()[1].WithinBounds(bounds) &&
                   rect.GetVertices()[2].WithinBounds(bounds) &&
                   rect.GetVertices()[3].WithinBounds(bounds);
        }
    }
}
