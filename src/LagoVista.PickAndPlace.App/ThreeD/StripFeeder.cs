using HelixToolkit.Wpf;
using LagoVista.Core.Models.Drawing;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.ThreeD
{
    public class StripFeeder : DrawingBase
    {
        public void Render3DModel(Model3DGroup parentGroup, LagoVista.Manufacturing.Models.StripFeeder model, Point2D<double> origin = null, double? rotate = null)
        {
            var feederModelGroup = new Model3DGroup();
            var baseMeshBuilder = new MeshBuilder(false, false);
            var baseRect = new Rect3D(0, 0, 0, model.Length, model.Width, 4);
            baseMeshBuilder.AddBox(baseRect);
            var boxModel = new GeometryModel3D() { Geometry = baseMeshBuilder.ToMesh(true), Material = BlackMaterial };
            feederModelGroup.Children.Add(boxModel);

            for (var idx = 0; idx < model.RowCount; ++idx)
            {
                var rowBottomY = (idx * model.RowWidth) + model.BottomLeftRow1Margin.Y;

                var rowMeshBuilder = new MeshBuilder(false, false);
                var bottom = idx * model.RowWidth + model.BottomLeftRow1Margin.Y;
                var side1Rect = new Rect3D(0, bottom, 4, model.Length, 2, model.Height - 4);
                var side2Rect = new Rect3D(0, bottom + model.RowWidth - 4, 4, model.Length, 2, model.Height - 4);
                rowMeshBuilder.AddBox(side1Rect);
                rowMeshBuilder.AddBox(side2Rect);
                var rowModel = new GeometryModel3D() { Geometry = rowMeshBuilder.ToMesh(true), Material = BlueMaterial };
                feederModelGroup.Children.Add(rowModel);

                var row = model.Rows[idx];
                if (row.Component != null && row.Component.Value?.ComponentPackage != null)
                {
                    var package = row.Component.Value.ComponentPackage.Value;
                    var tapeY = rowBottomY + (model.RowWidth / 2) - (package.TapeWidth.Value / 2) - 1;

                    var tapeMeshBuilder = new MeshBuilder(false, false);
                    var tapeWidth = row.Component.Value.ComponentPackage.Value.TapeWidth;
                    var tapeRect = new Rect3D(0, tapeY, model.Height - 1, model.Length, tapeWidth.Value, 0.2);
                    tapeMeshBuilder.AddBox(tapeRect);
                    var tapeModel = new GeometryModel3D() { Geometry = tapeMeshBuilder.ToMesh(true), Material = WhiteMaterial };
                    feederModelGroup.Children.Add(tapeModel);
                    var componentCount = model.Length / package.SpacingX.Value;
                    for (var cmptIdx = 0; cmptIdx < componentCount - 1; ++cmptIdx)
                    {
                        var y = (package.CenterY - package.Width / 2).Value;

                        var componentModel = Component.Render3DModel(row.Component.Value.ComponentPackage.Value);
                        componentModel.Transform = new TranslateTransform3D(cmptIdx * package.SpacingX.Value + 3, tapeY + y, ((model.Height - 1) - package.Height) + 0.3);
                        feederModelGroup.Children.Add(componentModel);
                    }

                    for (var holeIdx = 2; holeIdx < model.Length; holeIdx += 4)
                    {
                        var holeMeshBuilder = new MeshBuilder(false, false);
                        holeMeshBuilder.AddCylinder(new Point3D(holeIdx, tapeY + 1.75, model.Height - 0.8), new Point3D(holeIdx, tapeY + 1.75, model.Height - 0.7));
                        feederModelGroup.Children.Add(new GeometryModel3D() { Geometry = holeMeshBuilder.ToMesh(true), Material = BlackMaterial });
                    }
                }
            }

            if (origin != null)
            {
                var grp = new Transform3DGroup();

                if (rotate.HasValue)
                    grp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotate.Value),new Point3D(0,0,0)));

                grp.Children.Add(new TranslateTransform3D(origin.X, origin.Y, 0));


                feederModelGroup.Transform = grp;
            }
         
            parentGroup.Children.Add(feederModelGroup);
        }
    }
}
