using Emgu.CV.Structure;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using System.Globalization;

namespace LagoVista.PickAndPlace.Models
{
    public class MVLocatedRectangle
    {
        int _head;
        const int FILTER_SIZE = 9;
        const int THROW_AWAY = 3;

        public RotatedRect[] _rects = new RotatedRect[FILTER_SIZE];
        private double _pixelsPerMM;
        private double _errorMargin;
        private int _stabilizationCount;
        private Point2D<float> _viewCenter;

        public MVLocatedRectangle(CameraTypes cameraType, Point2D<int> viewCenter, double pixelsPerMM, double erorMargin, int stabilizationCount)
        {
            _viewCenter = new Point2D<float>(viewCenter.X, viewCenter.Y);
            _pixelsPerMM = pixelsPerMM;
            _errorMargin = erorMargin;
            _stabilizationCount = stabilizationCount;
            CameraType = cameraType;
        }

        public void Add(RotatedRect rect)
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
        /// CameraType that was used to identify the object.
        /// </summary>
        public CameraTypes CameraType { get;  }

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
