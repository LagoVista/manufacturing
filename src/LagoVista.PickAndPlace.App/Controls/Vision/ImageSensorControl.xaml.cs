using DirectShowLib.Dvd;
using Emgu.CV;
using Emgu.CV.Structure;
using LagoVista.Core.IOC;
using LagoVista.PickAndPlace.App.Views;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.ViewModels;
using LagoVista.XPlat;
using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.Controls
{
    public partial class ImageSensorControl : VMBoundUserControl<IImageCaptureService>
    {
        [DllImport("gdi32")]
        private static extern int DeleteObject(IntPtr o);

        
        private void ImageSensorControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(ViewModel != null)
                ViewModel.ActiveCamera = Camera;
        }

        LocatedByCamera _camera;
        public LocatedByCamera Camera
        {
            get => _camera;
            set
            {
                _camera = value;
                if(ViewModel != null)
                    ViewModel.ActiveCamera = value;
            }
        }        

        VideoCapture _videoCapture;
        Object _videoCaptureLocker = new object();

        public ImageSensorControl()
        {
            InitializeComponent();
            Stop.Visibility = Visibility.Collapsed;
            DataContextChanged += ImageSensorControl_DataContextChanged;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var vm = SLWIOC.Create<IVisionProfileViewModel>();
            vm.Profile = ViewModel.Profile;
            vm.Camera = ViewModel.Camera;

            var mvDialog = new VisionSettingsView();

            mvDialog.DataContext = vm;
            mvDialog.Show();
        }
    }
}