using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace
{
    public static class PointMathExtensions
    {
        // This is useful for feeders that can be installed horizontally or vertically.
        public static Point2D<double> SubtractWithConditionalSwap(this Point2D<double> p1, bool swap, Point2D<double> p2)
        {
            if (!swap)
                return p1 - p2;

            return new Point2D<double>(p1.X - p2.Y, p1.Y + p2.X);
        }

        public static Point2D<double> AddWithConditionalSwap(this Point2D<double> p1, bool swap, Point2D<double> p2)
        {
            if (!swap)
                return p1 + p2;

            return new Point2D<double>(p1.X + p2.Y, p1.Y - p2.X);
        }

    }
}
