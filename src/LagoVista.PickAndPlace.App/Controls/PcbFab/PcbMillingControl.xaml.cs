// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: adf4c2502653292cf4db35946077dd4d4178d493bab5c9c2eedd11bef4aea176
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.CV.Dnn;
using HelixToolkit.Wpf;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using LagoVista.XPlat;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.Controls.PcbFab
{

    public enum ImageModes
    {
        Top,
        Side,
        Front,
    }


    /// <summary>
    /// Interaction logic for PcbMillingControl.xaml
    /// </summary>
    public partial class PcbMillingControl : VMBoundUserControl<IPcbMillingViewModel>
    {
        public PcbMillingControl()
        {
            InitializeComponent();
        }

        public bool ModelToolVisible
        {
            get { return ModelTool.Visible; }
            set { ModelTool.Visible = value; }
        }

        const int CAMERA_MOVE_DELTA = 10;

        private void RenderBoard(PrintedCircuitBoard board, PcbMillingProject project, bool resetZoomAndView = true)
        {
            var linePoints = new Point3DCollection();

            var modelGroup = new Model3DGroup();
            var copperMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(0xb8, 0x73, 0x33));
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
            var blackMaterial = MaterialHelper.CreateMaterial(Colors.Black);
            var grayMaterial = MaterialHelper.CreateMaterial(Colors.DarkGray);

            var scrapX = project == null ? 0 : project.ScrapSides;
            var scrapY = project == null ? 0 : project.ScrapTopBottom;
            var boardThickness = project == null ? 1.60 : project.StockThickness;


            if (_topWiresVisible)
            {
                foreach (var wireSection in board.TopWires.GroupBy(wre => wre.W))
                {
                    var width = wireSection.First().W;

                    foreach (var wire in wireSection)
                    {
                        var topWireMeshBuilder = new MeshBuilder(false, false);
                        var boxRect = new Rect3D(wire.X1 - (width / 2), wire.Y1, -0.1, width, wire.Length, 0.2);
                        topWireMeshBuilder.AddBox(boxRect);

                        topWireMeshBuilder.AddCylinder(new Point3D(wire.X1, wire.Y1, -0.1), new Point3D(wire.X1, wire.Y1, .1), width / 2, 50, true, true);

                        var boxModel = new GeometryModel3D() { Geometry = topWireMeshBuilder.ToMesh(true), Material = copperMaterial };
                        boxModel.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), wire.Angle), new Point3D(wire.X1, wire.Y1, 0));
                        modelGroup.Children.Add(boxModel);
                    }
                }
            }

            if (_bottomWiresVisible)
            {
                foreach (var wireSection in board.BottomWires.GroupBy(wre => wre.W))
                {
                    var width = wireSection.First().W;

                    foreach (var wire in wireSection)
                    {
                        var topWireMeshBuilder = new MeshBuilder(false, false);
                        var boxRect = new Rect3D(wire.X1 - (width / 2), wire.Y1, -0.105, width, wire.Length, 0.2);
                        topWireMeshBuilder.AddBox(boxRect);

                        topWireMeshBuilder.AddCylinder(new Point3D(wire.X1, wire.Y1, -0.105), new Point3D(wire.X1, wire.Y1, .095), width / 2, 50, true, true);

                        var boxModel = new GeometryModel3D() { Geometry = topWireMeshBuilder.ToMesh(true), Material = grayMaterial };
                        boxModel.Transform = new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), wire.Angle), new Point3D(wire.X1, wire.Y1, 0));
                        modelGroup.Children.Add(boxModel);
                    }
                }
            }

            foreach (var element in board.Components)
            {
                foreach (var pad in element.SMDPads)
                {
                    var padMeshBuilder = new MeshBuilder(false, false);

                    padMeshBuilder.AddBox(new Rect3D(pad.OrgX - (pad.DX / 2), pad.OrgY - (pad.DY / 2), -0.1, (pad.DX), (pad.DY), 0.2));
                    var box = new GeometryModel3D() { Geometry = padMeshBuilder.ToMesh(true), Material = element.Layer == PCBLayers.TopCopper ? copperMaterial : grayMaterial };

                    var transformGroup = new Transform3DGroup();
                    transformGroup.Children.Add(new RotateTransform3D(new AxisAngleRotation3D(new Vector3D(0, 0, 1), element.Rotation)));
                    transformGroup.Children.Add(new TranslateTransform3D(new Vector3D(element.X.Value, element.Y.Value, element.Layer == PCBLayers.TopCopper ? 0 : 0.05)));

                    box.Transform = transformGroup;

                    modelGroup.Children.Add(box);
                }

                foreach (var pad in element.Pads)
                {
                    var padCopperMeshBuilder = new MeshBuilder(false, false);
                    padCopperMeshBuilder.AddCylinder(new Point3D(pad.X, pad.Y, 0), new Point3D(pad.X, pad.Y, 0.1), pad.D * 0.75);
                    var padCopper = new GeometryModel3D() { Geometry = padCopperMeshBuilder.ToMesh(true), Material = copperMaterial };
                    modelGroup.Children.Add(padCopper);

                    var padDrillMeshBuilder = new MeshBuilder(false, false);
                    padDrillMeshBuilder.AddCylinder(new Point3D(pad.X, pad.Y, 0), new Point3D(pad.X, pad.Y, 0.101), pad.D / 2);
                    var padDrill = new GeometryModel3D() { Geometry = padDrillMeshBuilder.ToMesh(true), Material = blackMaterial };
                    modelGroup.Children.Add(padDrill);
                }

                if (_pcbVisible)
                {
                    var billBoard = new BillboardTextVisual3D() { Foreground = Brushes.White, Text = element.Name, Position = new Point3D(element.X.Value + scrapX, element.Y.Value + scrapY, 4), FontSize = 14 };
                    viewport.Children.Add(billBoard);
                }
            }

            foreach (var via in board.Vias)
            {
                var padCopperMeshBuilder = new MeshBuilder(false, false);
                padCopperMeshBuilder.AddCylinder(new Point3D(via.X, via.Y, 0), new Point3D(via.X, via.Y, 0.1), (via.DrillDiameter));
                var padCopper = new GeometryModel3D() { Geometry = padCopperMeshBuilder.ToMesh(true), Material = copperMaterial };
                modelGroup.Children.Add(padCopper);

                var padDrillMeshBuilder = new MeshBuilder(false, false);
                padDrillMeshBuilder.AddCylinder(new Point3D(via.X, via.Y, 0), new Point3D(via.X, via.Y, 0.11), via.DrillDiameter / 2);
                var padDrill = new GeometryModel3D() { Geometry = padDrillMeshBuilder.ToMesh(true), Material = blackMaterial };
                modelGroup.Children.Add(padDrill);
            }

            if (_pcbVisible)
            {
                foreach (var circle in board.Holes)
                {
                    var circleMeshBuilder = new MeshBuilder(false, false);
                    circleMeshBuilder.AddCylinder(new Point3D(circle.X, circle.Y, 0), new Point3D(circle.X, circle.Y, 0.01), circle.D / 2);
                    modelGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });
                }

               
                var boardEdgeMeshBuilderTop = new MeshBuilder(false, false);
                var boardEdgeMeshBuilderBottom = new MeshBuilder(false, false);

                var boardOutline = board.Layers.Where(layer => layer.Layer == PCBLayers.BoardOutline).SingleOrDefault();

                var polygon = boardOutline.Wires.Select(wire => new Point(wire.X1, wire.Y1)).ToList();

                var result = CuttingEarsTriangulator.Triangulate(polygon);

                List<int> tri = new List<int>();
                for (int i = 0; i < result.Count; i++)
                {
                    tri.Add(result[i]);
                    if (tri.Count == 3)
                    {
                        Console.WriteLine("Triangle " + (i / 3).ToString() + " : " + tri[0].ToString() + ", " + tri[1].ToString() + ", " + tri[2].ToString());
                        boardEdgeMeshBuilderTop.AddTriangle(new Point3D(polygon[tri[0]].X, polygon[tri[0]].Y, 0),
                            new Point3D(polygon[tri[1]].X, polygon[tri[1]].Y, 0),
                            new Point3D(polygon[tri[2]].X, polygon[tri[2]].Y, 0));
                        tri.Clear();
                    }

                   
                }

                modelGroup.Children.Add(new GeometryModel3D() { Geometry = boardEdgeMeshBuilderTop.ToMesh(true), Material = greenMaterial });
              

                //   var cornerWires = board.Layers.Where(layer => layer.Layer == PCBLayers.BoardOutline).FirstOrDefault().Wires.Where(wire => wire.Crv.HasValue == true);
                //   var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().X1 - cornerWires.First().X2) : 0;
                //   if (radius == 0)
                //   {
                //       var polyGon = new HelixToolkit.Wpf.Polygon3D(); 

                //           //polyGon.Points.Add(new Point3D(wire.X1, wire.Y1, 0));
                //           //polyGon.Points.Add(new Point3D(wire.X1, wire.Y1, 10));
                //           //polyGon.Points.Add(new Point3D(wire.X2, wire.Y2, 0));

                //           //    polyGon.Points.Add(new Point3D(wire.X1, wire.Y1, 10));
                //           //    polyGon.Points.Add(new Point3D(wire.X2, wire.Y2, 10));
                //           //    polyGon.Points.Add(new Point3D(wire.X2, wire.Y2, 0));
                //           //    polyGon.Points.Add(new Point3D(wire.X1, wire.Y1, 10));

                //       //var last = new Point3D(boardOutline.Wires.Last().X1, boardOutline.Wires.Last().Y1, 0);
                //       //polyGon.Points.Add(last);

                //       //last = new Point3D(boardOutline.Wires.Last().X1, boardOutline.Wires.Last().Y1, 10);
                //       //polyGon.Points.Add(last);


                //       //boardEdgeMeshBuilderTop.AddPolygon(polyGon.Points);

                ////       boardEdgeMeshBuilderTop.AddBox(new Point3D(board.Width / 2, board.Height / 2, -boardThickness / 2), board.Width, board.Height, boardThickness);
                //   }
                //   else
                //   {
                //       boardEdgeMeshBuilderTop.AddBox(new Point3D(board.Width / 2, board.Height / 2, -boardThickness / 2), board.Width - (radius * 2), board.Height - (radius * 2), boardThickness);
                //       boardEdgeMeshBuilderTop.AddBox(new Point3D(board.Width / 2, radius / 2, -boardThickness / 2), board.Width - (radius * 2), radius, boardThickness);
                //       boardEdgeMeshBuilderTop.AddBox(new Point3D(board.Width / 2, board.Height - radius / 2, -boardThickness / 2), board.Width - (radius * 2), radius, boardThickness);
                //       boardEdgeMeshBuilderTop.AddBox(new Point3D(radius / 2, board.Height / 2, -boardThickness / 2), radius, board.Height - (radius * 2), boardThickness);
                //       boardEdgeMeshBuilderTop.AddBox(new Point3D(board.Width - radius / 2, board.Height / 2, -boardThickness / 2), radius, board.Height - (radius * 2), boardThickness);
                //       boardEdgeMeshBuilderTop.AddCylinder(new Point3D(radius, radius, -boardThickness), new Point3D(radius, radius, 0), radius, 50, true, true);
                //       boardEdgeMeshBuilderTop.AddCylinder(new Point3D(radius, board.Height - radius, -boardThickness), new Point3D(radius, board.Height - radius, 0), radius, 50, true, true);
                //       boardEdgeMeshBuilderTop.AddCylinder(new Point3D(board.Width - radius, radius, -boardThickness), new Point3D(board.Width - radius, radius, 0), radius, 50, true, true);
                //       boardEdgeMeshBuilderTop.AddCylinder(new Point3D(board.Width - radius, board.Height - radius, -boardThickness), new Point3D(board.Width - radius, board.Height - radius, 0), radius, 50, true, true);
                //   }

            }

            PCBLayer.Content = modelGroup;
            PCBLayer.Transform = new TranslateTransform3D(scrapX, scrapY, 0);

            if (project != null)
            {
                var stockGroup = new Model3DGroup();

                var circleMeshBuilder = new MeshBuilder(false, false);
                //var holdDownDrills = project.GetHoldDownDrills(board);
                //foreach (var drl in holdDownDrills)
                //{
                //    circleMeshBuilder.AddCylinder(new Point3D(drl.X, drl.Y, -boardThickness), new Point3D(drl.X, drl.Y, 0.01), project.HoldDownDiameter / 2);
                //}

                stockGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });

                if (_stockVisible)
                {
                    var stockMeshBuilder = new MeshBuilder(false, false);
                    stockMeshBuilder.AddBox(new Point3D(project.StockWidth / 2, project.StockHeight / 2, -boardThickness / 2), project.StockWidth, project.StockHeight, boardThickness - 0.05);
                    stockGroup.Children.Add(new GeometryModel3D() { Geometry = stockMeshBuilder.ToMesh(true), Material = copperMaterial });
                }

                StockLayer.Content = stockGroup;
            }
            else
            {
                StockLayer.Content = null;
            }

            if (resetZoomAndView)
            {
                RefreshExtents();
            }
        }

        private void RefreshExtents()
        {
            switch (_imageMode)
            {
                case ImageModes.Front: ShowFrontView(); break;
                case ImageModes.Side: ShowLeftView(); break;
                case ImageModes.Top: ShowTopView(); break;
            }
        }


        private void ShowLeftView()
        {
            double min = ViewModel.GCodeFileManager.HasValidFile ? ViewModel.GCodeFileManager.Min.Y : 0;
            double max = ViewModel.Machine.Settings.WorkAreaSize.Y;

            if (ViewModel.PCBManager.HasBoard)
                max = ViewModel.PCBManager.Board.Height;

            if (ViewModel.GCodeFileManager.HasValidFile)
                max = ViewModel.GCodeFileManager.Max.Y;

            var factor = 18.0;

            var delta = (max - min);
            var y = (delta / 2) + min;
            var x = -12 * factor;
            var z = 7 * factor;

            Camera.Position = new Point3D(x, y, z);
            Camera.LookDirection = new Vector3D(4, 0.0001, -1.7);

            CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";

        }

        private void ShowTopView()
        {
            double minX = ViewModel.GCodeFileManager.HasValidFile ? ViewModel.GCodeFileManager.Min.X : 0;
            double maxX = ViewModel.Machine.Settings.WorkAreaSize.X;

            if (ViewModel.PCBManager.HasBoard)
                maxX = ViewModel.PCBManager.Board.Width;

            if (ViewModel.GCodeFileManager.HasValidFile)
                maxX = ViewModel.GCodeFileManager.Max.X;

            double minY = ViewModel.GCodeFileManager.HasValidFile ? ViewModel.GCodeFileManager.Min.Y : 0;
            double maxY = ViewModel.Machine.Settings.WorkAreaSize.Y;

            if (ViewModel.PCBManager.HasBoard)
                maxY = ViewModel.PCBManager.Board.Height;

            if (ViewModel.GCodeFileManager.HasValidFile)
                maxY = ViewModel.GCodeFileManager.Max.Y;


            var deltaX = maxX - minX;
            var deltaY = maxY - minY;

            var x = deltaX / 2 + minX;
            var y = deltaY / 2 + minY;

            if (ViewModel.PCBManager.HasProject)
                y += ViewModel.PCBManager.Project.ScrapTopBottom;

            Camera.Position = new Point3D(x, y, 400);
            Camera.LookDirection = new Vector3D(0, 0.0001, -1);

            CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";
        }

        private void ShowFrontView()
        {
            double min = ViewModel.GCodeFileManager.HasValidFile ? ViewModel.GCodeFileManager.Min.X : 0;
            double max = ViewModel.Machine.Settings.WorkAreaSize.X;

            if (ViewModel.PCBManager.HasBoard)
                max = ViewModel.PCBManager.Board.Width;

            if (ViewModel.GCodeFileManager.HasValidFile)
                max = ViewModel.GCodeFileManager.Max.X;

            var factor = 18.0;

            var delta = (max - min);
            var x = (delta / 2) + min;
            var y = -12 * factor;
            var z = 7 * factor;

            Camera.Position = new Point3D(x, y, z);
            Camera.LookDirection = new Vector3D(0.0001, 4, -1.7);

            CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";

        }

        public bool _stockVisible = true;
        public bool _pcbVisible = true;
        public bool _topWiresVisible = true;
        public bool _bottomWiresVisible = true;
        public bool _gcodeVisible = true;

        private void ChangeLayer_Handler(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "Stock":
                    {
                        _stockVisible = !_stockVisible;
                    }
                    break;
                case "PCB":
                    {
                        _pcbVisible = !_pcbVisible;
                    }
                    break;
                case "TopWires":
                    {
                        _topWiresVisible = !_topWiresVisible;
                    }
                    break;
                case "BottomWires":
                    {
                        _bottomWiresVisible = !_bottomWiresVisible;
                    }
                    break;
                case "GCode":
                    {
                        _gcodeVisible = !_gcodeVisible;
                    }
                    break;
                case "HeightMap":
                    HeightMap.Visible = !HeightMap.Visible;
                    // ModelHeightMapBoundary.Vis
                    //   HeightMapPoints
                    break;
            }

            if (ViewModel.PCB != null)
                RenderBoard(ViewModel.PCB, ViewModel.Project, false);
        }

        ImageModes _imageMode = ImageModes.Front;

        private void ChangeView_Handler(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            switch (btn.Tag.ToString())
            {
                case "Top":
                    {
                        _imageMode = ImageModes.Top;
                        ShowTopView();
                    }
                    break;
                case "Left":
                    {
                        _imageMode = ImageModes.Side;
                        ShowLeftView();
                    }
                    break;
                case "Front":
                    {
                        _imageMode = ImageModes.Front;
                        ShowFrontView();
                    }
                    break;
                case "ZoomIn":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y * 0.9, Camera.Position.Z * 0.9); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X * 0.9, Camera.Position.Y, Camera.Position.Z * 0.9); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z * .9); break;
                    }


                    break;
                case "ZoomOut":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y * 1.1, Camera.Position.Z * 1.1); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X * 1.1, Camera.Position.Y, Camera.Position.Z * 1.1); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z * 1.1); break;
                    }

                    break;
                case "ShowObject":
                    break;
                case "ShowAll":
                    break;
                case "Up":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "UpLeft":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "UpRight":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z + CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "MoveLeft":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }
                    break;
                case "Center":
                    break;
                case "Right":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }
                    break;
                case "Down":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "DownRight":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }
                    break;
                case "DownLeft":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z - CAMERA_MOVE_DELTA); break;
                        case ImageModes.Top: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                    }

                    break;
                case "Forwards":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y + CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X + CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }

                    break;
                case "Backwards":
                    switch (_imageMode)
                    {
                        case ImageModes.Front: Camera.Position = new Point3D(Camera.Position.X, Camera.Position.Y - CAMERA_MOVE_DELTA, Camera.Position.Z); break;
                        case ImageModes.Side: Camera.Position = new Point3D(Camera.Position.X - CAMERA_MOVE_DELTA, Camera.Position.Y, Camera.Position.Z); break;
                    }

                    break;
            }

            CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";

        }
    }
}