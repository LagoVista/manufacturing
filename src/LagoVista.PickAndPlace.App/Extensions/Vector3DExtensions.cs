// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f54f3e0ba8f7ecf81307cc8d156a8b60fd50dae935424e26fd08a1d91ca7a98a
// IndexVersion: 2
// --- END CODE INDEX META ---
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App
{
    public static class Vector3DExtensions
    {
        public static Point3D ToMedia3D(this LagoVista.Core.Models.Drawing.Point3D<double> point3d)
        {
            return new Point3D(point3d.X, point3d.Y, point3d.Z);
        }
    }
}
