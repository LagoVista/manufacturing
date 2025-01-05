using Emgu.CV.Cuda;
using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.Util;

namespace LagoVista.PickAndPlace.Models
{
    public class MVLocatedCircle
    {
        DoubleMedianFilter _filter = new DoubleMedianFilter(5, 2);

        public void AddPoint(Point2D<float> point)
        {
            _filter.Add(point);
        }

        public bool Centered { get; set; }

        public Point2D<int> CenterPixels 
        { 
            get; set;         
        }

        public Point2D<double> Position { get; set; }

        public Point2D<int> OffsetPixels { get; set; }
        public Point2D<double> OffsetMM { get; set; }
        public int RadiusPixels { get; set; }
        public double RadiusMM { get; set; }
       
        public Point2D<double> StandardDeviation { get; set; }

        public int FoundCount { get; set; }
    }
}
