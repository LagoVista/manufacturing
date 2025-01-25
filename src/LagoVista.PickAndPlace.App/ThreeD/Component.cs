using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.ThreeD
{
    public class Component : DrawingBase
    {
        public static Model3D Render3DModel(LagoVista.Manufacturing.Models.ComponentPackage package)
        {
            var baseMeshBuilder = new MeshBuilder(false, false);
            var boxRect = new Rect3D(0, 0, 0, package.Length, package.Width, package.Height);
            baseMeshBuilder.AddBox(boxRect);
            return new GeometryModel3D() { Geometry = baseMeshBuilder.ToMesh(true), Material = RedMaterial };
        }
    }
}
