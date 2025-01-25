using HelixToolkit.Wpf;
using LagoVista.Manufacturing.Models;
using MSDMarkwort.Kicad.Parser.PcbNew;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.ThreeD
{
    public class StripFeeder :  DrawingBase
    {
        public void Render3DModel(Model3DGroup modelGroup, LagoVista.Manufacturing.Models.StripFeeder model)
        {
            var baseMeshBuilder = new MeshBuilder(false, false);
            var boxRect = new Rect3D(0, 0, 0, model.Length, model.Width, model.Height - 8);
            baseMeshBuilder.AddBox(boxRect);
            var boxModel = new GeometryModel3D() { Geometry = baseMeshBuilder.ToMesh(true), Material = GrayMaterial };
            modelGroup.Children.Add(boxModel);

            for (var idx = 0; idx < model.RowCount; ++idx)
            {
                var rowMeshBuilder = new MeshBuilder(false, false);
                var rowRect = new Rect3D(0, idx * (model.RowWidth), model.Height - 8, model.Length, model.RowWidth - 2, model.Height);
                rowMeshBuilder.AddBox(rowRect);
                var rowModel = new GeometryModel3D() { Geometry = rowMeshBuilder.ToMesh(true), Material = BlueMaterial };
                modelGroup.Children.Add(rowModel);

                var row = model.Rows[idx];
                if(row.Component != null && row.Component.Value.ComponentPackage != null)
                {
                    var tapeMeshBuilder = new MeshBuilder(false, false);
                    var tapeWidth = row.Component.Value.ComponentPackage.Value.TapeWidth;
                    var tapeRect = new Rect3D(0, idx * (model.RowWidth), model.Height + 3.5, model.Length, tapeWidth.Value, 1);
                    tapeMeshBuilder.AddBox(tapeRect);
                    var tapeModel = new GeometryModel3D() { Geometry = tapeMeshBuilder.ToMesh(true), Material = WhiteMaterial };
                    modelGroup.Children.Add(tapeModel);
                }
            }            
        }
    }
}
