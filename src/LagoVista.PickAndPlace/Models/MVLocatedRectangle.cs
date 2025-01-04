using Emgu.CV.Structure;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using System.Globalization;

namespace LagoVista.PickAndPlace.Models
{
    public class MVLocatedRectangle
    {
        public MVLocatedRectangle()
        {

        }

        /// <summary>
        /// Offset in MM from the center of the image
        /// </summary>
        public Point2D<double> Offset { get; }

        /// <summary>
        /// Position of the center of the center of the object in the work space.
        /// </summary>
        public Point2D<double> Position { get; set; }

        /// <summary>
        /// Bounding rectangle including angle of the object.
        /// </summary>
        public RotatedRect RotatedRect { get; set; }

        /// <summary>
        /// Angle of the object in degrees.
        /// </summary>
        public double Angle { get; set; }

        /// <summary>
        /// Camera that was used to identify the object.
        /// </summary>
        public CameraTypes Camera { get; set; }

        /// <summary>
        /// True if the object was cenered in the image with respect to the error toolerance of the of vision profile.
        /// </summary>
        public bool Centered { get; set; }

        /// <summary>
        /// Size of the object in MM
        /// </summary>
        public Point2D<double> Size { get; set; }

        /// <summary>
        /// Number of times this object was found in the image within the error tolerance of the vision profile.
        /// </summary>
        public int FoundCount { get; set; }
    }
}
