// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5cd6c07311aea68bae4adfe724b61eb0b8852b331bafef6eb68961276efd814f
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace
{
    public static class GCodeExtensions
    {
        public static string ToGCode(this Point2D<double> point, string command = "G0")
        {
            if (point == null)
                return String.Empty;

            return $"{command} X{point.X} Y{point.Y}";
        }

        public static string ToGCode(this Point3D<double> point, string command = "G0")
        {
            return $"{command} X{point.X} Y{point.Y}";
        }
    }
}
