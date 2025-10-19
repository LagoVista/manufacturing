// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5c2a9be9eb4c5b259b55030071cdb163f1a64d5d76bf0cf599c1332a2deabfcd
// IndexVersion: 0
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
