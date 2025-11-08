// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b3b2ff249f6814ec512407a3eb84adc47dd819131390170a8ecebe6c9fdce873
// IndexVersion: 2
// --- END CODE INDEX META ---
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
        public static Model3DGroup Render3DModel(LagoVista.Manufacturing.Models.ComponentPackage package)
        {
            var componentModelGroup = new Model3DGroup();
            var baseMeshBuilder = new MeshBuilder(false, false);
            var boxRect = new Rect3D(-package.Length / 2, -package.Width / 2, 0, package.Length, package.Width, package.Height);
            baseMeshBuilder.AddBox(boxRect);

            var padMeshBuilder = new MeshBuilder(false, false);

            foreach(var pad in package.Layout.SmdPads)
            {
                var padRect = new Rect3D(pad.X1, pad.Y1, 0.1, pad.DX, pad.DY, package.Height);
                padMeshBuilder.AddBox(padRect);
            }

            var padsGeometry = new GeometryModel3D() { Geometry = padMeshBuilder.ToMesh(true), Material = CopperMaterial };
            padsGeometry.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90), new Point3D(0, 0, 0));
            componentModelGroup.Children.Add(padsGeometry);
            componentModelGroup.Children.Add(new GeometryModel3D() { Geometry = baseMeshBuilder.ToMesh(true), Material = GrayMaterial});

            return componentModelGroup;
        }
    }
}
