﻿using DirectShowLib;
using Emgu.CV;
using Emgu.CV.Structure;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public abstract partial class MachineVisionViewModelBase
    {
        VideoCapture _topCameraCapture;
        VideoCapture _bottomCameraCapture;

        Object _videoCaptureLocker = new object();
       
        private string _status = "Idle";
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        private VideoCapture InitCapture(string cameraName)
        {
            try
            {
                var cameras = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);
                //    var camera = cameras.FirstOrDefault(cam => cam.N == cameraName);
                var camera = cameras.Select((cam, idx) => new { cam = cam, index = idx }).FirstOrDefault(cam => cam.cam.Name == cameraName);
                if (camera != null)
                    return new VideoCapture(camera.index);
                else
                    return null;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not open camera: " + ex.Message);
                return null;
            }
        }

       

    }
}
