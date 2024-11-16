using System.Globalization;

namespace LagoVista.GCode.Sender
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
        public static string FileFilterGCode = "GCode|*.g;*.tap;*.nc;*.ngc|All Files|*.*";
        public static string FileFilterHeightMap = "Height Maps|*.hmap|All Files|*.*";

        public static string FilePathErrors = "Resources\\GrblErrors.txt";
        public static string FilePathWebsite = "Resources\\index.html";

        public static char[] NewLines = new char[] { '\n', '\r' };

        static Constants()
        {

        }
    }
}
