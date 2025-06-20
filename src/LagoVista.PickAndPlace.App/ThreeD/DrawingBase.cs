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
        protected readonly static Material CopperMaterial = MaterialHelper.CreateMaterial(Color.FromRgb(0xb8, 0x73, 0x33));
        protected readonly static Material RedMaterial = MaterialHelper.CreateMaterial(Colors.Red);
        protected readonly static Material GreenMaterial = MaterialHelper.CreateMaterial(Colors.Green);
        protected readonly static Material BlackMaterial = MaterialHelper.CreateMaterial(Colors.Black);
        protected readonly static Material WhiteMaterial = MaterialHelper.CreateMaterial(Colors.White);
        protected readonly static Material BlueMaterial = MaterialHelper.CreateMaterial(Colors.Blue);
        protected readonly static Material GrayMaterial = MaterialHelper.CreateMaterial(Colors.Gray);
        protected readonly static Material SilverMaterial = MaterialHelper.CreateMaterial(Colors.Silver);

        public DrawingBase() {
        }
    }
}
