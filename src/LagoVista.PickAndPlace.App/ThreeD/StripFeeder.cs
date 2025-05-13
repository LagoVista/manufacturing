using HelixToolkit.Wpf;
using LagoVista.Manufacturing.Models;
using MSDMarkwort.Kicad.Parser.PcbNew;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.ThreeD
{
    public class StripFeeder : DrawingBase
    {
        public void Render3DModel(Model3DGroup modelGroup, LagoVista.Manufacturing.Models.StripFeeder model)
        {
            var baseMeshBuilder = new MeshBuilder(false, false);
            var baseRect = new Rect3D(0, 0, 0, model.Length, model.Width, 4);
            baseMeshBuilder.AddBox(baseRect);
            var boxModel = new GeometryModel3D() { Geometry = baseMeshBuilder.ToMesh(true), Material = BlackMaterial };
            modelGroup.Children.Add(boxModel);

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
                modelGroup.Children.Add(rowModel);

                var row = model.Rows[idx];
                if(row.Component != null && row.Component.Value?.ComponentPackage != null)
                {
                    var package = row.Component.Value.ComponentPackage.Value;
                    var tapeY = rowBottomY + (model.RowWidth / 2) - (package.TapeWidth.Value / 2) - 1;

                    var tapeMeshBuilder = new MeshBuilder(false, false);
                    var tapeWidth = row.Component.Value.ComponentPackage.Value.TapeWidth;
                    var tapeRect = new Rect3D(0, tapeY, model.Height - 1, model.Length, tapeWidth.Value, 0.2);
                    tapeMeshBuilder.AddBox(tapeRect);
                    var tapeModel = new GeometryModel3D() { Geometry = tapeMeshBuilder.ToMesh(true), Material = WhiteMaterial };
                    modelGroup.Children.Add(tapeModel);
                    var componentCount = model.Length / package.SpacingX.Value;
                    for (var cmptIdx = 0; cmptIdx < componentCount - 1; ++cmptIdx)
                    {
                        var y = (package.CenterY - package.Width / 2).Value;

                        var componentModel = Component.Render3DModel(row.Component.Value.ComponentPackage.Value);
                        componentModel.Transform = new TranslateTransform3D(cmptIdx * package.SpacingX.Value + 3 , tapeY + y , ((model.Height - 1) - package.Height) + 0.3);
                        modelGroup.Children.Add(componentModel);
                    }

                    for(var holeIdx = 2; holeIdx < model.Length; holeIdx += 4)
                    {
                        var holeMeshBuilder = new MeshBuilder(false, false);
                       holeMeshBuilder.AddCylinder(new Point3D(holeIdx, tapeY + 1.75 , model.Height - 0.8), new Point3D(holeIdx, tapeY + 1.75, model.Height - 0.7));
                       modelGroup.Children.Add(new GeometryModel3D() { Geometry = holeMeshBuilder.ToMesh(true), Material = BlackMaterial });
                    }
                }
            }            
        }
    }
}
