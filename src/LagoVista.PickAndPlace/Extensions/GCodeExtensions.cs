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
            return $"{command} X{point.X} Y{point.Y}";
        }
    }
}
