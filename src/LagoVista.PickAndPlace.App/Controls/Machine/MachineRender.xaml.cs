using HelixToolkit.Wpf;
using LagoVista.Core.Models.Drawing;
using LagoVista.PickAndPlace.App.Controls.PcbFab;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.XPlat;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Numerics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for MachineRender.xaml
    /// </summary>
    public partial class MachineRender : VMBoundUserControl<IPartsViewModel>
    {
        public MachineRender()
        {
            InitializeComponent();
            DataContextChanged += MachineRender_DataContextChanged;
            this.Loaded += MachineRender_Loaded;
        }

        private void MachineRender_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                ViewModel.AutoFeederViewModel.PropertyChanged += ViewModel_PropertyChanged;
                ViewModel.StripFeederViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        private void ViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(IPartsViewModel.Machine))
            {
                if (ViewModel.Machine != null)
                    Machine = ViewModel.Machine.Settings;
            }

            if (e.PropertyName == nameof(IStripFeederViewModel.Feeders))
            {
                StripFeeders = ViewModel.StripFeederViewModel.Feeders;
            }

            if (e.PropertyName == nameof(IAutoFeederViewModel.Feeders))
            {
                AutoFeeders = ViewModel.AutoFeederViewModel.Feeders;
            }
        }

        Material copperMaterial = MaterialHelper.CreateMaterial(System.Windows.Media.Color.FromRgb(0xb8, 0x73, 0x33));
        Material whiteMaterial = MaterialHelper.CreateMaterial(Colors.White);
        Material redMaterial = MaterialHelper.CreateMaterial(Colors.Red);
        Material brownMaterial = MaterialHelper.CreateMaterial(Colors.Brown);
        Material greenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
        Material goldMaterial = MaterialHelper.CreateMaterial(Colors.Gold);
        Material blueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
        Material blackMaterial = MaterialHelper.CreateMaterial(Colors.Black);
        Material grayMaterial = MaterialHelper.CreateMaterial(Colors.DarkGray);
        Material silverMaterial = MaterialHelper.CreateMaterial(Colors.Silver);


        private void MachineRender_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (ViewModel != null)
            {
                ViewModel.PropertyChanged += ViewModel_PropertyChanged;
                ViewModel.AutoFeederViewModel.PropertyChanged += ViewModel_PropertyChanged;
                ViewModel.StripFeederViewModel.PropertyChanged += ViewModel_PropertyChanged;
            }
        }

        LagoVista.Manufacturing.Models.Machine _machine;
        public LagoVista.Manufacturing.Models.Machine Machine
        {
            get => _machine;
            set
            {
                _machine = value;
                if (_machine != null)
                {
                    var maxX = Machine.FrameSize.X;
                    var maxY = Machine.FrameSize.Y;
                    var factor = Machine.FrameSize.Y / 10;

                    var max = Math.Max(maxX, maxY);
                    var x = (max / 2) + 0;
                    var y = -12 * factor;
                    var z = 7 * factor;

                    Camera.Position = new Point3D(x, y, z);
                    var dir = Camera.LookDirection;
                    RenderStagingPlates(value);
                    RenderFrame(value);
                }
            }
        }

        ObservableCollection<LagoVista.Manufacturing.Models.StripFeeder> _stripFeeders;
        public ObservableCollection<LagoVista.Manufacturing.Models.StripFeeder> StripFeeders
        {
            get => _stripFeeders;
            set
            {
                _stripFeeders = value;
                if (_stripFeeders != null)
                    RenderStripFeeders(value);
            }
        }

        public void RenderStripFeeders(ObservableCollection<LagoVista.Manufacturing.Models.StripFeeder> feeders)
        {

        }

        ObservableCollection<LagoVista.Manufacturing.Models.AutoFeeder> _autoFeeders;
        public ObservableCollection<LagoVista.Manufacturing.Models.AutoFeeder> AutoFeeders
        {
            get => _autoFeeders;
            set
            {
                _autoFeeders = value;
                if (_autoFeeders != null)
                    RenderAutoFeeders(value);
            }
        }


        public void RenderAutoFeeders(ObservableCollection<LagoVista.Manufacturing.Models.AutoFeeder> feeders)
        {

        }

        public void RenderFrame(LagoVista.Manufacturing.Models.Machine machine)
        {
            var modelGroup = new Model3DGroup();

            var topWireMeshBuilder = new MeshBuilder(false, false);
            topWireMeshBuilder.AddBox(new Rect3D(0, 0, -23, 20, machine.FrameSize.Y, 20));
            topWireMeshBuilder.AddBox(new Rect3D(machine.FrameSize.X - 20, 0, -23, 20, machine.FrameSize.Y, 20));

            topWireMeshBuilder.AddBox(new Rect3D(0, 0, 50, 20, machine.FrameSize.Y, 20));
            topWireMeshBuilder.AddBox(new Rect3D(machine.FrameSize.X - 20, 0, 50, 20, machine.FrameSize.Y, 20));

            var boxModel = new GeometryModel3D() { Geometry = topWireMeshBuilder.ToMesh(true), Material = grayMaterial };
            modelGroup.Children.Add(boxModel);

            var railMeshBuilder = new MeshBuilder(false, false);
            railMeshBuilder.AddBox(new Rect3D(3, 0, 70, 14, machine.FrameSize.Y, 5));
            railMeshBuilder.AddBox(new Rect3D(machine.FrameSize.X - 17, 0, 70, 14, machine.FrameSize.Y, 5));
            var railModel = new GeometryModel3D() { Geometry = railMeshBuilder.ToMesh(true), Material = whiteMaterial };
            modelGroup.Children.Add(railModel);


            foreach (var feederRail in machine.FeederRails)
            {
                var feederMeshBuilder = new MeshBuilder(false, false);
                var rect = new Rect3D(0, feederRail.FirstFeederOrigin.Y, -43, machine.FrameSize.X, 20, 20);
                feederMeshBuilder.AddBox(rect);
                var feederRailModel = new GeometryModel3D() { Geometry = feederMeshBuilder.ToMesh(true), Material = grayMaterial };
                modelGroup.Children.Add(feederRailModel);
            }

            FrameLayer.Content = modelGroup;
        }

        public void RenderStagingPlates(LagoVista.Manufacturing.Models.Machine machine)
        {
            var modelGroup = new Model3DGroup();
            foreach (var plate in machine.StagingPlates)
            {
                var topWireMeshBuilder = new MeshBuilder(false, false);
                var boxRect = new Rect3D(plate.Origin.X, plate.Origin.Y, 0, plate.Size.X, plate.Size.Y, 3);
                topWireMeshBuilder.AddBox(boxRect);

                var r = Convert.ToByte(plate.Color.Substring(1, 2), 16);
                var g = Convert.ToByte(plate.Color.Substring(3, 2), 16);
                var b = Convert.ToByte(plate.Color.Substring(5, 2), 16);
                //                plate.Color = 

                var material = MaterialHelper.CreateMaterial(System.Windows.Media.Color.FromRgb(r, g, b));
                var boxModel = new GeometryModel3D() { Geometry = topWireMeshBuilder.ToMesh(true), Material = material };

                var holeMeshBuilder = new MeshBuilder(false, false);
                var maskMeshBuilder = new MeshBuilder(false, false);

                for (double plateX = 0; plateX <= plate.HoleSpacing * (plate.LastUsableColumn ) / 2; plateX += (plate.HoleSpacing))
                {

                    var idx = 0;
                    for (double plateY = 0; plateY < ((plate.Size.Y - 10) - plate.FirstHole.Y) ; plateY += (plate.HoleSpacing / 2))
                    {
                        var xOFfset = (idx++ % 2 == 0) ? plateX : plateX + 15;
                        var x = xOFfset + plate.FirstHole.X;
                        var y = plateY + plate.FirstHole.Y + plate.Origin.Y;

                        maskMeshBuilder.AddCylinder(new Point3D(x, y, -0.025), new Point3D(x, y, 3.05), 3.3);
                        holeMeshBuilder.AddCylinder(new Point3D(x, y, -0.05), new Point3D(x, y, 3.1), 1.5);
                    }
                }

                for (double plateX = 0; plateX <= plate.HoleSpacing * ((plate.LastUsableColumn + 1) / 2); plateX += (plate.HoleSpacing / 2))
                {
                    var geometry = TextCreator.CreateTextLabelModel3D($"{(plateX / 15) + 1}", Brushes.White, true, 3, new Point3D(plateX + plate.FirstHole.X, plate.Origin.Y + 5, 3.2), new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                    geometry.BackMaterial = material;
                    modelGroup.Children.Add(geometry);

                    geometry = TextCreator.CreateTextLabelModel3D($"{(plateX / 15) + 1}", Brushes.White, true, 3, new Point3D(plateX + plate.FirstHole.X, plate.Origin.Y + plate.Size.Y  - 5, 3.2), new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                    geometry.BackMaterial = material;
                    modelGroup.Children.Add(geometry);

                }

                for (double platey = 0; platey < ((plate.Size.Y - 10) - plate.FirstHole.Y); platey += (plate.HoleSpacing / 2))
                {
                    var letter = (char)('A' + (platey / 15));
                    var geometry = TextCreator.CreateTextLabelModel3D($"{letter}", Brushes.White, true, 3, new Point3D( plate.FirstHole.X - 10, plate.Origin.Y + plate.FirstHole.Y + platey, 3.2), new Vector3D(1, 0, 0), new Vector3D(0, 1, 0));
                    geometry.BackMaterial = material;
                    modelGroup.Children.Add(geometry);
                }

                modelGroup.Children.Add(new GeometryModel3D() { Geometry = holeMeshBuilder.ToMesh(true), Material = whiteMaterial });
                modelGroup.Children.Add(new GeometryModel3D() { Geometry = maskMeshBuilder.ToMesh(true), Material = copperMaterial });
                modelGroup.Children.Add(boxModel);

                var feederRender = new ThreeD.StripFeeder();

                var screwMeshBuilder = new MeshBuilder(false, false);

                if (ViewModel.StripFeederViewModel.Feeders != null)
                {
                    foreach (var feeder in ViewModel.StripFeederViewModel.Feeders)
                    {
                        var keyRow = feeder.ReferenceHoleRow.Key;
                        var keyCol = feeder.ReferenceHoleColumn.Key;
                        var colIndex = int.Parse(keyCol) - 1;
                        var rowIndex = keyRow[0] - 'A' ;

                        var mountHoleX = ((colIndex * (plate.HoleSpacing / 2)) + plate.Origin.X + plate.FirstHole.X);
                        var mountHoleY = ((rowIndex * (plate.HoleSpacing / 2)) + plate.Origin.Y + plate.FirstHole.Y);

                        var xOffset = mountHoleX - feeder.MountingHoleOffset.X;
                        var yOffset =  mountHoleY - feeder.MountingHoleOffset.Y;

                        screwMeshBuilder.AddCylinder(new Point3D(mountHoleX, mountHoleY, -0.025), new Point3D(mountHoleX, mountHoleY, 10.05),1.5);
                        feederRender.Render3DModel(modelGroup, feeder, new Point2D<double>(xOffset, yOffset), feeder.Orientation.Key == "horizontal" ? 0 : -90);
                    }

                    modelGroup.Children.Add(new GeometryModel3D() { Geometry = screwMeshBuilder.ToMesh(true), Material = grayMaterial });

                }
            }

            StagingPlatesLayer.Content = modelGroup;
        }

        private async void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if (Machine != null)
            {
                await ViewModel.ReloadedAsync();
                RenderFrame(Machine);
                RenderStagingPlates(Machine);
            }
        }

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            var maxX = Machine.FrameSize.X;
            var maxY = Machine.FrameSize.Y;
            var factor = Machine.FrameSize.Y / 10;

            var max = Math.Max(maxX, maxY);
            var x = (max / 2) + 0;
            var y = -12 * factor;
            var z = 7 * factor;

            Camera.Position = new Point3D(x, y, z);
            var dir = Camera.LookDirection;

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
            }
        }

        private void ShowLeftView()
        {
            //double min = ViewModel.GCodeFileManager.HasValidFile ? ViewModel.GCodeFileManager.Min.Y : 0;
            //double max = ViewModel.Machine.Settings.WorkAreaSize.Y;

            //if (ViewModel.PCBManager.HasBoard)
            //    max = ViewModel.PCBManager.Board.Height;

            //if (ViewModel.GCodeFileManager.HasValidFile)
            //    max = ViewModel.GCodeFileManager.Max.Y;

            //var factor = 18.0;

            //var delta = (max - min);
            //var y = (delta / 2) + min;
            //var x = -12 * factor;
            //var z = 7 * factor;

            //Camera.Position = new Point3D(x, y, z);
            //Camera.LookDirection = new Vector3D(4, 0.0001, -1.7);

            //CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";

        }

        private void ShowTopView()
        {
            double minX = -20;
            double maxX = _machine.FrameSize.X;

            var minY = -20;
            var maxY = _machine.FrameSize.Y;


            var deltaX = maxX - minX;
            var deltaY = maxY - minY;

            var x = deltaX / 2 + minX;
            var y = deltaY / 2 + minY;

            Camera.Position = new Point3D(x, y, 400);
            Camera.LookDirection = new Vector3D(0, 0.0001, -1);

            _imageMode = ImageModes.Top;
        }

        private void ShowFrontView()
        {
            //double min = ViewModel.GCodeFileManager.HasValidFile ? ViewModel.GCodeFileManager.Min.X : 0;
            //double max = ViewModel.Machine.Settings.WorkAreaSize.X;

            //if (ViewModel.PCBManager.HasBoard)
            //    max = ViewModel.PCBManager.Board.Width;

            //if (ViewModel.GCodeFileManager.HasValidFile)
            //    max = ViewModel.GCodeFileManager.Max.X;

            //var factor = 18.0;

            //var delta = (max - min);
            //var x = (delta / 2) + min;
            //var y = -12 * factor;
            //var z = 7 * factor;

            //Camera.Position = new Point3D(x, y, z);
            //Camera.LookDirection = new Vector3D(0.0001, 4, -1.7);

            //CameraPosition.Text = $"{Camera.Position.X},{Camera.Position.Y},{Camera.Position.Z}";

        }
    }
}
