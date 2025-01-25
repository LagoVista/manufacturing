using HelixToolkit.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;
using System.Windows.Media;

namespace LagoVista.PickAndPlace.App.ThreeD
{
    public class DrawingBase
    {
        readonly Material CopperMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(0xb8, 0x73, 0x33));
        protected readonly Material RedMaterial = MaterialHelper.CreateMaterial(Colors.Red);
        protected readonly Material GreenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
        protected readonly Material BlackMaterial = MaterialHelper.CreateMaterial(Colors.Black);
        protected readonly Material WhiteMaterial = MaterialHelper.CreateMaterial(Colors.White);
        protected readonly Material BlueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
        protected readonly Material GrayMaterial = MaterialHelper.CreateMaterial(Colors.Gray);

        public DrawingBase() {
        }
    }
}
