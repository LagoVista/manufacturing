// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9199e809e18990996ddddfe69e8a9cc1fc3d897a39d9830e0b635e13afdba919
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;

namespace LagoVista.GCode.Parser
{
    public class ParserState
    {
        public Vector3 Position;
        public ArcPlane Plane;
        public double? Feed;
        public double? SpindleRPM;
        public double? RotateAngle;
        public ParseDistanceMode DistanceMode;
        public ParseDistanceMode ArcDistanceMode;
        public ParseUnit Unit;
        public double LastMotionMode;

        public ParserState()
        {
            Position = new Vector3();
            Plane = ArcPlane.XY;
            DistanceMode = ParseDistanceMode.Absolute;
            ArcDistanceMode = ParseDistanceMode.Relative;
            Unit = ParseUnit.Metric;
            LastMotionMode = -1;
        }
    }

}
