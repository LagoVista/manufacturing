using DirectShowLib;
using LagoVista.Core.IOC;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.App.Views;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.XPlat;
using System.Linq;
using System.Windows;

namespace LagoVista.PickAndPlace.App.Controls
{
    public partial class ImageSensorControl : VMBoundUserControl<IImageCaptureService>
    {
        public ImageSensorControl()
        {
            InitializeComponent();
            DataContextChanged += ImageSensorControl_DataContextChanged;
        }

        private void ImageSensorControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if(ViewModel != null)
                ViewModel.CameraType = CameraType;
        }

        CameraTypes _cameraType;
        public CameraTypes CameraType
        {
            get => _cameraType;
            set
            {
                _cameraType = value;
                if(ViewModel != null)
                    ViewModel.CameraType = value;
            }
        }        
               
        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var vm = SLWIOC.Create<IVisionProfileViewModel>();
            vm.Camera = ViewModel.Camera;
            vm.SelectedCameraDevicePath = ViewModel.Camera.CameraDevice?.Id ?? "-1";
            var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
            var cameraList = cameras.Select(cm => EntityHeader.Create(cm.DevicePath, cm.Name)).ToList();
            cameraList.Insert(0, EntityHeader.Create("-1","-select camera-"));
            vm.CameraList =  new System.Collections.ObjectModel.ObservableCollection<EntityHeader>( cameraList);            

            var mvDialog = new VisionSettingsView();
            mvDialog.Owner = Window.GetWindow(this); 
            mvDialog.DataContext = vm;
            mvDialog.Show();
        }
    }
}