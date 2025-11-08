// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c961c2af4d6f27b8a3856b93f43fe52321b0ca5429dccebbc774d974929199b4
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Globalization;

namespace LagoVista
{
    public static class MathHelpers
    {
        public static double ToDegrees(this double radians)
        {
            return radians * (180 / Math.PI);
        }

        public static double ToRadians(this double degrees)
        {
            return degrees * (Math.PI / 180);
        }

        public static string ToDim(this double value)
        {
            /* First remove any trailing zeros */
            var dim = value.ToString("0.0000", new NumberFormatInfo() { NumberDecimalSeparator = "." }).TrimEnd('0');

            /* If at this point we have a whole number, remove the . */
            return dim.TrimEnd('.');
        }

    }
}
