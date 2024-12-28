using HelixToolkit.Wpf;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.App.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LagoVista.PickAndPlace.App.Controls
{
    /// <summary>
    /// Interaction logic for MachineRender.xaml
    /// </summary>
    public partial class MachineRender : UserControl
    {
        public MachineRender()
        {
            InitializeComponent();
            DataContextChanged += MachineRender_DataContextChanged;
        }

        Material copperMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(0xb8, 0x73, 0x33));
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
            var vm = DataContext as PnPJobViewModel;
            Machine = vm.Machine.Settings;

            var maxX = Machine.FrameWidth;
            var maxY = Machine.FrameHeight;
            var factor = Machine.FrameHeight / 10;

            var max = Math.Max(maxX, maxY);
            var x = (max / 2) + 0;
            var y = -12 * factor;
            var z = 7 * factor;

            Camera.Position = new Point3D(x, y, z);
            var dir = Camera.LookDirection;
        }

        LagoVista.Manufacturing.Models.Machine _machine;
        public LagoVista.Manufacturing.Models.Machine Machine 
        {   get => _machine;
            set {
                _machine = value;
                if (_machine != null)
                {
                    RenderStagingPlates(value);
                    RenderFrame(value);
                }
            }
        }

        List<LagoVista.Manufacturing.Models.StripFeeder> _stripFeeders;
        public List<LagoVista.Manufacturing.Models.StripFeeder> StripFeeders
        {
            get => _stripFeeders;
            set
            {
                _stripFeeders = value;
                if (_stripFeeders != null)
                    RenderStripFeeders(value);
            }
        }

        public void RenderStripFeeders(List<LagoVista.Manufacturing.Models.StripFeeder> feeders)
        {

        }

        List<LagoVista.Manufacturing.Models.AutoFeeder> _autoFeeders;
        public List<LagoVista.Manufacturing.Models.AutoFeeder> AutoFeeders
        {
            get => _autoFeeders;
            set
            {
                _autoFeeders = value;
                if (_autoFeeders != null)
                    RenderAutoFeeders(value);
            }
        }


        public void RenderAutoFeeders(List<LagoVista.Manufacturing.Models.AutoFeeder> feeders)
        {

        }

        public void RenderFrame(LagoVista.Manufacturing.Models.Machine machine)
        {

            var modelGroup = new Model3DGroup();

            var topWireMeshBuilder = new MeshBuilder(false, false);
            topWireMeshBuilder.AddBox(new Rect3D(0, 0, -23, 20, machine.FrameHeight, 20));
            topWireMeshBuilder.AddBox(new Rect3D(machine.FrameWidth-20, 0, -23, 20, machine.FrameHeight, 20));

            topWireMeshBuilder.AddBox(new Rect3D(0, 0, 50, 20, machine.FrameHeight, 20));
            topWireMeshBuilder.AddBox(new Rect3D(machine.FrameWidth - 20, 0, 50, 20, machine.FrameHeight, 20));

            var boxModel = new GeometryModel3D() { Geometry = topWireMeshBuilder.ToMesh(true), Material = grayMaterial };
            modelGroup.Children.Add(boxModel);

            var railMeshBuilder = new MeshBuilder(false, false);
            railMeshBuilder.AddBox(new Rect3D(3, 0, 70,14, machine.FrameHeight, 5));
            railMeshBuilder.AddBox(new Rect3D(machine.FrameWidth - 17, 0, 70, 14, machine.FrameHeight, 5));
            var railModel = new GeometryModel3D() { Geometry = railMeshBuilder.ToMesh(true), Material = whiteMaterial };
            modelGroup.Children.Add(railModel);
            

            foreach (var feederRail in machine.FeederRails)
            {
                var feederMeshBuilder = new MeshBuilder(false, false);
                var rect = new Rect3D(0, feederRail.Origin.Y, -43, machine.FrameWidth, 20, 20);
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
                var boxRect = new Rect3D(plate.Origin.X, plate.Origin.Y, 0 , plate.Size.X, plate.Size.Y, 3);
                topWireMeshBuilder.AddBox(boxRect);

                var r = Convert.ToByte(plate.Color.Substring(1, 2), 16);
                var g = Convert.ToByte(plate.Color.Substring(3, 2), 16);
                var b = Convert.ToByte(plate.Color.Substring(5, 2), 16);
                //                plate.Color = 

                var material = MaterialHelper.CreateMaterial(Color.FromRgb(r,g,b));
                var boxModel = new GeometryModel3D() { Geometry = topWireMeshBuilder.ToMesh(true), Material = material };

                var holeMeshBuilder = new MeshBuilder(false, false);
                var maskMeshBuilder = new MeshBuilder(false, false);


                for (double x = plate.HoleSpacing; x < plate.Size.X ; x += (plate.HoleSpacing / 2))
                {
                    for (double y = plate.HoleSpacing; y < plate.Size.Y; y += (plate.HoleSpacing / 2))
                    {
                        maskMeshBuilder.AddCylinder(new Point3D(x, y + plate.Origin.Y, -0.025), new Point3D(x, y + plate.Origin.Y, 3.05), 4);
                        holeMeshBuilder.AddCylinder(new Point3D(x, y + plate.Origin.Y, -0.05), new Point3D(x, y + plate.Origin.Y, 3.1), 3);
                    }
                }             

                modelGroup.Children.Add(new GeometryModel3D() { Geometry = holeMeshBuilder.ToMesh(true), Material = whiteMaterial });
                modelGroup.Children.Add(new GeometryModel3D() { Geometry = maskMeshBuilder.ToMesh(true), Material = copperMaterial });
                modelGroup.Children.Add(boxModel);

            }

            StagingPlatesLayer.Content = modelGroup;
            //StagingPlatesLayer.Transform = new TranslateTransform3D(scrapX, scrapY, 0);            
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            if(Machine != null)
            {
                RenderFrame(Machine);
                RenderStagingPlates(Machine);
            }
        }       

        private void ResetZoom_Click(object sender, RoutedEventArgs e)
        {
            var maxX = Machine.FrameWidth;
            var maxY = Machine.FrameHeight;
            var factor = Machine.FrameHeight / 10;

            var max = Math.Max(maxX, maxY);
            var x = (max / 2) + 0;
            var y = -12 * factor;
            var z = 7 * factor;

            Camera.Position = new Point3D(x, y, z);
            var dir = Camera.LookDirection;         

        }
    }
}
