// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fbb7f27d889ccd7f6d0c83cedb776df155e42c087e02046796ab79447a70b42d
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace LagoVista.Manufacturing.Utils
{
    public enum AngleType
    {
        Radians, 
        Degrees,
    }

    public class MathUtils
    {
        public static double CalculateAngle(Point2D<double> p1, Point2D<double> p2, AngleType angleType = AngleType.Degrees)
        {
            var theta = Math.Atan((p2.Y - p1.Y) / (p2.X - p1.X));
            if (angleType == AngleType.Radians)
                return theta;
            
            return theta * (180.0 / Math.PI);
        }

        /// <summary>
        /// This will correct for any errors by feeders or boards not being installed perfectly in the horizontal or vertical access. 
        /// These coordinates will always assume that the XY location is at the lower left, calcualte the angle offset based on that origin to point 2 and apply the difference.
        /// </summary>
        /// <param name="origin">Known Origin point of position on the machine.</param>
        /// <param name="knownPoint2">Second point on the machine that is known.</param>
        /// <param name="point">Point to be adjusted for any errors in those two points.</param>
        /// <returns>Adjusted point based on rotation around the angle</returns>
        public static Point2D<double> CorrectForError(Point2D<double> origin, Point2D<double> knownPoint2, Point2D<double> point)
        {
            var delta = knownPoint2 - origin;


            var theta = 90 + CalculateAngle(origin, knownPoint2);
            Debug.WriteLine($"Delta: {delta}");
            Debug.WriteLine($"Point 1: {origin}");
            Debug.WriteLine($"Point 2: {knownPoint2}");
            Debug.WriteLine($"Target : {point}");
            Debug.WriteLine($"Angle  : {theta}");

            var result = CorrectForError(knownPoint2, theta, point);

            Debug.WriteLine($"compensated : {result}");
            return result;
        }

        /// <summary>
        /// Adjust the point for error rotation based on the angle (theta) and target point.
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="theta"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static Point2D<double> CorrectForError(Point2D<double> origin, double theta, Point2D<double> point, AngleType angleType = AngleType.Degrees)
        {
            if(angleType == AngleType.Degrees) 
                theta = theta * (Math.PI / 180.0);

            var deltaY = point.Y - origin.Y;
            var deltaX = point.X - origin.X;

            var newX = deltaX * Math.Cos(theta) - deltaY * Math.Sin(theta);
            var newY = deltaY * Math.Cos(theta) + deltaX * Math.Sin(theta);
            
            return new Point2D<double>(Math.Round(origin.X + newX, 3), Math.Round(origin.Y + newY, 3));
        }


    }
}
