using HelixToolkit.Wpf;
using LagoVista.Core.Models.Drawing;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.ThreeD
{
    public class StripFeeder : DrawingBase
    {
        public void Render3DModel(Model3DGroup parentGroup, LagoVista.Manufacturing.Models.StripFeeder feeder, Point2D<double> origin = null, double? rotate = null)
        {
            System.Windows.Media.Color color = (System.Windows.Media.Color)ColorConverter.ConvertFromString(feeder.Color);
            var feederColor = MaterialHelper.CreateMaterial(color);

            var feederModelGroup = new Model3DGroup();
            var baseMeshBuilder = new MeshBuilder(false, false);
            var baseRect = new Rect3D(0, 0, 0, feeder.Length, feeder.Width, 2);
            baseMeshBuilder.AddBox(baseRect);
            var boxModel = new GeometryModel3D() { Geometry = baseMeshBuilder.ToMesh(true), Material = feederColor };
            feederModelGroup.Children.Add(boxModel);

            for (var idx = 0; idx < feeder.RowCount; ++idx)
            {
                var row = feeder.Rows[idx];
                
                var rowBottomY = (double)((idx * feeder.RowWidth) + feeder.TapeReferenceHoleOffset.Y + (idx > 0 ? (double)feeder.RowSpacing : 0));

                var rowMeshBuilder = new MeshBuilder(false, false);
                var bottom = idx * (double)(feeder.RowWidth + feeder.BottomLeftRow1Margin.Y + (idx > 0 ? (double)feeder.RowSpacing : 0));
                var side1Rect = new Rect3D(0, bottom,                         1.5,   feeder.Length, 1.5, feeder.Height - 2);
                var side2Rect = new Rect3D(0, bottom + feeder.RowWidth - 1.5, 1.5, feeder.Length, 1.5, feeder.Height - 2);
                
                rowMeshBuilder.AddBox(side1Rect);
                rowMeshBuilder.AddBox(side2Rect);
               
                var rowModel = new GeometryModel3D() { Geometry = rowMeshBuilder.ToMesh(true), Material = feederColor  };
                feederModelGroup.Children.Add(rowModel);

                if (row.Component != null && row.Component.Value?.ComponentPackage != null)
                {
                    var package = row.Component.Value.ComponentPackage.Value;
                    var tapeY = rowBottomY;// + (feeder.RowWidth / 2) - (package.TapeWidth.Value / 2);

                    var tapeMeshBuilder = new MeshBuilder(false, false);
                    var tapeWidth = row.Component.Value.ComponentPackage.Value.TapeWidth;
                    var tapeRect = new Rect3D(0, tapeY, feeder.Height - 1, feeder.Length, tapeWidth.Value, 0.2);
                    tapeMeshBuilder.AddBox(tapeRect);
                    Material tapeMaterial = WhiteMaterial;
                    Material holeColor = WhiteMaterial;
                    var tapeColor = row.Component.Value.TapeColor != null ? row.Component.Value.TapeColor.Value : row.Component.Value.ComponentPackage.Value.TapeColor.Value;

                    switch (tapeColor)
                    {
                        case Manufacturing.Models.TapeColors.Black:
                            tapeMaterial = BlackMaterial;
                            holeColor = WhiteMaterial;
                            break;
                        case Manufacturing.Models.TapeColors.White:
                            tapeMaterial = WhiteMaterial;
                            holeColor = BlackMaterial;
                            break;
                        case Manufacturing.Models.TapeColors.Clear:
                            tapeMaterial = MaterialHelper.CreateMaterial(Colors.Silver, 0.25);
                            holeColor = BlackMaterial;
                            break;
                    }

                    var tapeModel = new GeometryModel3D() { Geometry = tapeMeshBuilder.ToMesh(true), Material = tapeMaterial };
                    feederModelGroup.Children.Add(tapeModel);
                    var componentCount = feeder.Length / package.SpacingX.Value;
                    
                    
                    for (var cmptIdx = 0; cmptIdx < componentCount - 1; ++cmptIdx)
                    {
                        var yOffset = tapeY + (package.HolesOnBothSideOfTape ? 0.0 : 1);// - package.Width / 2).Value;
                        var xOffset = 4.0;
                        yOffset -= 1;
                        var grp = new Transform3DGroup();



                        switch (row.Component.Value.ComponentPackage.Value.TapeRotation.Value)
                        {
                            case Manufacturing.Models.TapeRotations.Zero:
                                grp.Children.Add(new TranslateTransform3D(feeder.RowWidth / 2, 0, 0));
                                grp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -90), new Point3D(0, 0, 0)));
                                xOffset += package.Width / 2;
                                break;
                            case Manufacturing.Models.TapeRotations.OneEighty:
                                grp.Children.Add(new TranslateTransform3D(feeder.RowWidth / 2, 0, 0));
                                grp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 90), new Point3D(0, 0, 0)));
                                xOffset += package.Width / 2;
                                break;
                            case Manufacturing.Models.TapeRotations.Ninety:
                                grp.Children.Add(new TranslateTransform3D(0, feeder.RowWidth / 2, 0));
                                grp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), 0), new Point3D(0, 0, 0)));
                                xOffset += package.Length / 2;
                                break;
                            case Manufacturing.Models.TapeRotations.MinusNinety:
                                grp.Children.Add(new TranslateTransform3D(0, feeder.RowWidth / 2, 0));
                                grp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), -180), new Point3D(0, feeder.RowWidth / 2, 0)));
                                xOffset += package.Length / 2;
                                break;

                        }

                        grp.Children.Add(new TranslateTransform3D(cmptIdx * package.SpacingX.Value + xOffset, yOffset, ((feeder.Height - 1) - package.Height) + 0.3));

                        var componentModel = Component.Render3DModel(row.Component.Value.ComponentPackage.Value);
                        componentModel.Transform = grp;
                        feederModelGroup.Children.Add(componentModel);
                    }

                    for (var holeIdx = 2; holeIdx < feeder.Length; holeIdx += 4)
                    {
                        var holeMeshBuilder = new MeshBuilder(false, false);
                        if(!feeder.TapeHolesOnTop || row.Component.Value.ComponentPackage.Value.HolesOnBothSideOfTape)
                            holeMeshBuilder.AddCylinder(new Point3D(holeIdx, tapeY + 1.75, feeder.Height - 0.8), new Point3D(holeIdx, tapeY + 1.75, feeder.Height - 0.65), 0.5);

                        if (feeder.TapeHolesOnTop || row.Component.Value.ComponentPackage.Value.HolesOnBothSideOfTape)
                            holeMeshBuilder.AddCylinder(new Point3D(holeIdx, tapeY + (tapeWidth.Value - 1.75), feeder.Height - 0.8), new Point3D(holeIdx, tapeY + (tapeWidth.Value - 1.75), feeder.Height - 0.65), 0.5);

                        feederModelGroup.Children.Add(new GeometryModel3D() { Geometry = holeMeshBuilder.ToMesh(true), Material = holeColor });
                    }
                }
            }

            if (origin != null)
            {
                var grp = new Transform3DGroup();

                if (rotate.HasValue)
                    grp.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), rotate.Value),new Point3D(feeder.MountingHoleOffset.X,feeder.MountingHoleOffset.Y,0)));

                grp.Children.Add(new TranslateTransform3D(origin.X, origin.Y, 2));


                feederModelGroup.Transform = grp;
            }
         
            parentGroup.Children.Add(feederModelGroup);
        }
    }
}
