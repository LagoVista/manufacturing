using Emgu.CV.Structure;
using Emgu.CV;
using System.Linq;
using System.Drawing;
using Emgu.CV.CvEnum;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    internal class ImageHelper
    {
        public void Circle(IInputOutputArray img, FoundCircle circle, Size size, int thickness = 1)
        {
            var color = circle.Centered ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            Line(img, 0, circle.CenterPixels.Y, size.Width, circle.CenterPixels.Y, color);
            Line(img, circle.CenterPixels.X, 0, circle.CenterPixels.X, size.Height, color);

            CvInvoke.Circle(img, new System.Drawing.Point(circle.CenterPixels.X, circle.CenterPixels.Y), circle.RadiusPixels, new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
            CvInvoke.PutText(img, $"Radius {circle.RadiusMM}mm", new Point(size.Width - 300, size.Height - 100), FontFace.HersheyPlain, 1, new Bgr(System.Drawing.Color.White).MCvScalar);
            CvInvoke.PutText(img, $"Found Count: {circle.FoundCount}", new Point(size.Width - 300, size.Height - 70), FontFace.HersheyPlain, 1, new Bgr(System.Drawing.Color.White).MCvScalar);
            CvInvoke.PutText(img, $"Error (mm) {circle.OffsetMM}", new Point(size.Width - 300, size.Height - 40), FontFace.HersheyPlain, 1, new Bgr(System.Drawing.Color.White).MCvScalar);
        }

        public void Circle(IInputOutputArray img, int x, int y, int radius, System.Drawing.Color color, int thickness = 1)
        {
            CvInvoke.Circle(img, new System.Drawing.Point(x, y), radius, new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
        }

        public void Line(IInputOutputArray img, int x1, int y1, int x2, int y2, System.Drawing.Color color, int thickness = 1)
        {
            //if (!ShowOriginalImage)
            //{
            //    color = System.Drawing.Color.White;
            //}

            CvInvoke.Line(img, new System.Drawing.Point(x1, y1),
                new System.Drawing.Point(x2, y2),
                new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
        }

        public void DrawRect(IInputOutputArray img, PointF[] p, string msg, System.Drawing.Color color)
        {
            Line(img, (int)p[0].X, (int)p[0].Y, (int)p[1].X, (int)p[1].Y, color);
            Line(img, (int)p[1].X, (int)p[1].Y, (int)p[2].X, (int)p[2].Y, color);
            Line(img, (int)p[2].X, (int)p[2].Y, (int)p[3].X, (int)p[3].Y, color);
            Line(img, (int)p[3].X, (int)p[3].Y, (int)p[0].X, (int)p[0].Y, color);

            var center = new Point()
            {
                X = (int)p.Max(p=>p.X),
                Y = (int)p.Min(p=>p.Y),
            };

            CvInvoke.PutText(img, msg, center, FontFace.HersheyPlain, 1, new Bgr(System.Drawing.Color.Red).MCvScalar);


        }

        public bool ShowOriginalImage { get; set; }
    }
}
