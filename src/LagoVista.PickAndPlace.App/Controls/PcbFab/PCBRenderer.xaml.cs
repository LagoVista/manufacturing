using HelixToolkit.Wpf;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.App.Controls.UI;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for PCBRenderer.xaml
    /// </summary>
    public partial class PCBRenderer : UserControl
    {
        public PCBRenderer()
        {
            InitializeComponent();
        }

        public void RenderRevision(CircuitBoardRevision board)
        {
            var linePoints = new Point3DCollection();

            var modelGroup = new Model3DGroup();
            var copperMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(0xb8, 0x73, 0x33));
            var redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
            var greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
            var blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
            var blackMaterial = MaterialHelper.CreateMaterial(Colors.Black);
            var grayMaterial = MaterialHelper.CreateMaterial(Colors.DarkGray);

            var boardThickness = 1.60;


            if (TopWiresVisible)
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

            if (BottomWiresVisible)
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

            foreach (var element in board.PcbComponents)
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

                //if (PCBVisible)
                //{
                //    var billBoard = new BillboardTextVisual3D() { Foreground = Brushes.White, Text = element.N, Position = new Point3D(element.X.V + scrapX, element.Y.V + scrapY, 4), FontSize = 14 };
                //    viewport.Children.Add(billBoard);
                //}
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

            if (PCBVisible)
            {
                foreach (var circle in board.Holes)
                {
                    var circleMeshBuilder = new MeshBuilder(false, false);
                    circleMeshBuilder.AddCylinder(new Point3D(circle.X, circle.Y, 0), new Point3D(circle.X, circle.Y, 0.01), circle.D / 2);
                    modelGroup.Children.Add(new GeometryModel3D() { Geometry = circleMeshBuilder.ToMesh(true), Material = blackMaterial });
                }

                #region Hold your nose to discover why irregular boards don't render as expected... 
                /* gonna cheat here in next chunk of code...need to make progress, assume all corners are
                 * either square or round.  If rounded, same radius...WILL revisit this at some point, KDW 2/24/2017
                 * FWIW - feel so dirty doing this, but need to move on :*( 
                 * very happy to accept a PR to fix this!  Proper mechanism is to create a polygon and likely subdivide the curve into smaller polygon edges
                 * more work than it's worth right now....sorry again :(
                 */
                //TODO: Render proper edge of board.

                var boardEdgeMeshBuilder = new MeshBuilder(false, false);

                //var cornerWires = board.Layers.Where(layer => layer.Layer == PCBLayers.BoardOutline).FirstOrDefault().Wires.Where(wire => wire.Crv.HasValue == true);
                var cornerWires = board.Outline.Where(lne=> lne.Crv > 0);
                var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().X1 - cornerWires.First().X2) : 0;
                if (radius == 0)
                {
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width.Value / 2, board.Height.Value / 2, -boardThickness / 2), board.Width.Value, board.Height.Value, boardThickness);
                }
                else
                {
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width.Value / 2, board.Height.Value / 2, -boardThickness / 2), board.Width.Value - (radius * 2), board.Height.Value - (radius * 2), boardThickness);
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width.Value / 2, radius / 2, -boardThickness / 2), board.Width.Value - (radius * 2), radius, boardThickness);
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width.Value / 2, board.Height.Value - radius / 2, -boardThickness / 2), board.Width.Value - (radius * 2), radius, boardThickness);
                    boardEdgeMeshBuilder.AddBox(new Point3D(radius / 2, board.Height.Value / 2, -boardThickness / 2), radius, board.Height.Value - (radius * 2), boardThickness);
                    boardEdgeMeshBuilder.AddBox(new Point3D(board.Width.Value - radius / 2, board.Height.Value / 2, -boardThickness / 2), radius, board.Height.Value - (radius * 2), boardThickness);
                    boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, radius, -boardThickness), new Point3D(radius, radius, 0), radius, 50, true, true);
                    boardEdgeMeshBuilder.AddCylinder(new Point3D(radius, board.Height.Value - radius, -boardThickness), new Point3D(radius, board.Height.Value - radius, 0), radius, 50, true, true);
                    boardEdgeMeshBuilder.AddCylinder(new Point3D(board.Width.Value - radius, radius, -boardThickness), new Point3D(board.Width.Value - radius, radius, 0), radius, 50, true, true);
                    boardEdgeMeshBuilder.AddCylinder(new Point3D(board.Width.Value - radius, board.Height.Value - radius, -boardThickness), new Point3D(board.Width.Value - radius, board.Height.Value - radius, 0), radius, 50, true, true);
                }
                modelGroup.Children.Add(new GeometryModel3D() { Geometry = boardEdgeMeshBuilder.ToMesh(true), Material = greenMaterial });

                #endregion
            }

            PCBLayer.Content = modelGroup;

            if (CenterBoard)
            {
                PCBLayer.Transform = new TranslateTransform3D(-board.Width.Value / 2, - board.Height.Value / 2, 0);
                Camera.LookAt(new Point3D(0, 0, 0), 2);
            }
            else
            {
                Camera.LookAt(new Point3D(RenderOriginX + board.Width.Value / 2, RenderOriginY - board.Width.Value, 0), 0);
                PCBLayer.Transform = new TranslateTransform3D(RenderOriginX, RenderOriginY, 2);
            }
        }

        private static void OnBoardChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var host = dependencyObject as PCBRenderer;
            var newRevision = e.NewValue as CircuitBoardRevision;
            var oldRevision = e.OldValue as CircuitBoardRevision;

            if(newRevision != null)
            {
                host.RenderRevision(newRevision);
            }
        }

        public static readonly DependencyProperty CircuitBoardRevisionProperty = DependencyProperty.RegisterAttached(nameof(CircuitBoard), typeof(CircuitBoardRevision), typeof(PCBRenderer), new FrameworkPropertyMetadata(OnBoardChanged)); 
        public CircuitBoardRevision CircuitBoard
        {
            get => (CircuitBoardRevision)GetValue(CircuitBoardRevisionProperty);
            set => SetValue(CircuitBoardRevisionProperty, value);
        }


        private static void OnPropChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            var host = dependencyObject as PCBRenderer;
            if (host.CircuitBoard != null)
                host.RenderRevision(host.CircuitBoard);
        }

        public static readonly DependencyProperty TopWiresVisibleProperty = DependencyProperty.RegisterAttached(nameof(TopWiresVisible), typeof(bool), typeof(PCBRenderer), new FrameworkPropertyMetadata(OnPropChanged));
        public bool TopWiresVisible
        {
            get => (bool)GetValue(TopWiresVisibleProperty);
            set => SetValue(TopWiresVisibleProperty, value);
        }

        public static readonly DependencyProperty BottomWiresVisibleProperty = DependencyProperty.RegisterAttached(nameof(BottomWiresVisible), typeof(bool), typeof(PCBRenderer), new FrameworkPropertyMetadata(OnPropChanged));
        public bool BottomWiresVisible
        {
            get => (bool)GetValue(BottomWiresVisibleProperty);
            set => SetValue(BottomWiresVisibleProperty, value);
        }

        public static readonly DependencyProperty PCBVisibleProperty = DependencyProperty.RegisterAttached(nameof(PCBVisible), typeof(bool), typeof(PCBRenderer), new FrameworkPropertyMetadata(OnPropChanged));
        public bool PCBVisible
        {
            get => (bool)GetValue(PCBVisibleProperty);
            set => SetValue(PCBVisibleProperty, value);
        }

        public static readonly DependencyProperty RenderOriginXProperty = DependencyProperty.RegisterAttached(nameof(RenderOriginX), typeof(double), typeof(PCBRenderer), new FrameworkPropertyMetadata(OnPropChanged));
        public double RenderOriginX
        {
            get => (double)GetValue(RenderOriginXProperty);
            set => SetValue(RenderOriginXProperty, value);
        }

        public static readonly DependencyProperty RenderOriginYProperty = DependencyProperty.RegisterAttached(nameof(RenderOriginY), typeof(double), typeof(PCBRenderer), new FrameworkPropertyMetadata(OnPropChanged));
        public double RenderOriginY
        {
            get => (double)GetValue(RenderOriginYProperty);
            set => SetValue(RenderOriginYProperty, value);
        }

        public static readonly DependencyProperty CenterBoardProperty = DependencyProperty.RegisterAttached(nameof(CenterBoard), typeof(bool), typeof(PCBRenderer), new FrameworkPropertyMetadata(OnPropChanged));
        public bool CenterBoard
        {
            get => (bool)GetValue(CenterBoardProperty);
            set => SetValue(CenterBoardProperty, value);
        }      
    }
}
