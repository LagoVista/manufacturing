using Emgu.CV.Structure;
using Emgu.CV;
using System.Linq;
using System.Drawing;
using Emgu.CV.CvEnum;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;
using LagoVista.PickAndPlace.Interfaces.Services;
using System.Drawing.Drawing2D;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    internal class ImageHelper : IImageHelper<IInputOutputArray>
    {
        public void Circle(IMVImage<IInputOutputArray> img, MVLocatedCircle circle, Size size, int thickness = 1)
        {
            var color = circle.Centered ? System.Drawing.Color.Green : System.Drawing.Color.Red;

            Line(img, 0, (int)circle.CenterPixels.Y, size.Width, (int)circle.CenterPixels.Y, color);
            Line(img, (int)circle.CenterPixels.X, 0, (int)circle.CenterPixels.X, size.Height, color);

            CvInvoke.Circle(img.Image, new System.Drawing.Point((int)circle.CenterPixels.X, (int)circle.CenterPixels.Y), (int)circle.RadiusPixels, new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
            CvInvoke.PutText(img.Image, $"Diameter {circle.RadiusMM * 2}mm", new Point(size.Width - 300, size.Height - 100), FontFace.HersheyPlain, 1, new Bgr(System.Drawing.Color.White).MCvScalar);
            CvInvoke.PutText(img.Image, $"Found Count: {circle.FoundCount}", new Point(size.Width - 300, size.Height - 70), FontFace.HersheyPlain, 1, new Bgr(System.Drawing.Color.White).MCvScalar);
            CvInvoke.PutText(img.Image, $"Error (mm) {circle.OffsetMM}", new Point(size.Width - 300, size.Height - 40), FontFace.HersheyPlain, 1, new Bgr(System.Drawing.Color.White).MCvScalar);
        }

        public void Circle(IMVImage<IInputOutputArray> img, int x, int y, int radius, System.Drawing.Color color, int thickness = 1)
        {
            CvInvoke.Circle(img.Image, new System.Drawing.Point(x, y), radius, new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
        }

        public void Line(IMVImage<IInputOutputArray> img, int x1, int y1, int x2, int y2, System.Drawing.Color color, int thickness = 1)
        {
            CvInvoke.Line(img.Image, new System.Drawing.Point(x1, y1),
               new System.Drawing.Point(x2, y2),
               new Bgr(color).MCvScalar, thickness, Emgu.CV.CvEnum.LineType.AntiAlias);
        }

        public void DrawRect(IMVImage<IInputOutputArray> img, MVLocatedRectangle rect, System.Drawing.Color color)
        {
            var p =  rect.RotatedRect.ToPointArray();
            var matrix = new Matrix();
            matrix.RotateAt(rect.RotatedRect.Angle, rect.RotatedRect.Center);
            matrix.TransformPoints(p);

            Line(img, (int)p[0].X, (int)p[0].Y, (int)p[1].X, (int)p[1].Y, color);
            Line(img, (int)p[1].X, (int)p[1].Y, (int)p[2].X, (int)p[2].Y, color);
            Line(img, (int)p[2].X, (int)p[2].Y, (int)p[3].X, (int)p[3].Y, color);
            Line(img, (int)p[3].X, (int)p[3].Y, (int)p[0].X, (int)p[0].Y, color);


//            CvInvoke.PutText(img.Image, msg, center, FontFace.HersheyPlain, 1, new Bgr(System.Drawing.Color.Red).MCvScalar);
        }

        public void ShowCalibrationSquare(IMVImage<IInputOutputArray> destImage, Size size)
        {
            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            CvInvoke.Rectangle(destImage.Image, new System.Drawing.Rectangle(center.X - 100, center.Y - 100, 200, 200),
            new Bgr(System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0xFF)).MCvScalar);
        }

        public void DrawCrossHairs(IMVImage<IInputOutputArray> destImage, MachineCamera camera, Size size)
        {
            var profile = camera.CurrentVisionProfile;

            var center = new Point2D<int>()
            {
                X = size.Width / 2,
                Y = size.Height / 2
            };

            var scaledRadius = profile.TargetImageRadius * camera.CurrentVisionProfile.PixelsPerMM;

            Line(destImage, 0, center.Y, center.X - (int)scaledRadius, center.Y, System.Drawing.Color.Yellow);
            Line(destImage, center.X + (int)scaledRadius, center.Y, size.Width, center.Y, System.Drawing.Color.Yellow);

            Line(destImage, center.X, 0, center.X, center.Y - (int)scaledRadius, System.Drawing.Color.Yellow);
            Line(destImage, center.X, center.Y + (int)scaledRadius, center.X, size.Height, System.Drawing.Color.Yellow);

            Line(destImage, center.X - (int)scaledRadius, center.Y, center.X + (int)scaledRadius, center.Y, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));
            Line(destImage, center.X, center.Y - (int)scaledRadius, center.X, center.Y + (int)scaledRadius, System.Drawing.Color.FromArgb(0x7f, 0xFF, 0xFF, 0XFF));

            //if (_locatorViewModel.LocatorState == MVLocatorState.PartInTape)
            //{
            //    _imageHelper.Line(destImage, center.X - PartSizeWidth, center.Y - PartSizeHeight, center.X - PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);
            //    _imageHelper.Line(destImage, center.X + PartSizeWidth, center.Y - PartSizeHeight, center.X + PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);

            //    _imageHelper.Line(destImage, center.X - PartSizeWidth, center.Y + PartSizeHeight, center.X + PartSizeWidth, center.Y + PartSizeHeight, System.Drawing.Color.Yellow);
            //    _imageHelper.Line(destImage, center.X - PartSizeWidth, center.Y - PartSizeHeight, center.X + PartSizeWidth, center.Y - PartSizeHeight, System.Drawing.Color.Yellow);
            //}
            //else
            //{
            Circle(destImage, center.X, center.Y, (int)scaledRadius, System.Drawing.Color.Yellow);
            //}

        }
    }
}
