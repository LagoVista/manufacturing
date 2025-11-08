// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fa847157b70e9f3aff5ff9fd7ef600be95531320380b328dcd3a639710b019d4
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.MachineVision
{
    public static class ShapeHelpers
    {
        public static PointF[] ToPointArray(this RotatedRect rect)
        {
            var p1 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));
            var p2 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X - (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
            var p3 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y + (rect.Size.Height / 2)));
            var p4 = new LagoVista.Core.Models.Drawing.Point2D<float>(Convert.ToInt32(rect.Center.X + (rect.Size.Width / 2)), Convert.ToInt32(rect.Center.Y - (rect.Size.Height / 2)));

            var p = new PointF[]
            {
                new PointF(p1.X, p1.Y),
                new PointF(p2.X, p2.Y),
                new PointF(p3.X, p3.Y),
                new PointF(p4.X, p4.Y),
            };

            return p;
        }
    }
}
