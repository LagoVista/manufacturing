// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 27746c74c8e24e3c349dc0df970c7d9e8fac13a4f96eb7cd816fbb5c018e75f9
// IndexVersion: 2
// --- END CODE INDEX META ---
using System.Globalization;

namespace LagoVista.PickAndPlace
{
 


    public class Constants
    {
        public static NumberFormatInfo DecimalParseFormat = new NumberFormatInfo() { NumberDecimalSeparator = "." };

        public static NumberFormatInfo DecimalOutputFormat
        {
            get
            {
                return new NumberFormatInfo() { NumberDecimalSeparator = "." };
            }
        }

        public const double PixelToleranceEpsilon = 1.5;
        public const int PixelStabilizationToleranceCount = 10;

        public static string PickAndPlaceProject = "Pick and Place (*.pnpjob)|*.pnpjob";
        public static string PCBProject = "PCB Projec (*.pcbproj)|*.pcbproj|All Files|*.*";
        public static string FileFilterPCB = "Eagle|*.brd|All Files|*.*";
        public static string PartsPackages = "Package Library(*.pckgs)|*.pckgs";
        public static string PnPMachine = "PNP Machine(*.pnp)|*.pnp";
        public static string FileFilterGCode = "GCode|*.g;*.gcode;*.tap;*.nc;*.ngc|All Files|*.*";
        public static string FileFilterHeightMap = "Height Maps|*.hmap|All Files|*.*";

        public static string FilePathErrors = "Resources\\GrblErrors.txt";
        public static string FilePathAlarmCodes = "Resources\\GrblAlarmCodes.txt";
        public static string FilePathWebsite = "Resources\\index.html";

        public static char[] NewLines = new char[] { '\n', '\r' };

        static Constants()
        {

        }
    }
}
