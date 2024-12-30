﻿using Emgu.CV;
using Emgu.CV.Structure;
using LagoVista.PickAndPlace.App.Views;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
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
                ViewModel.Camera = Camera;
        }

        LocatedByCamera _camera;
        public LocatedByCamera Camera
        {
            get => _camera;
            set
            {
                _camera = value;
                if(ViewModel != null)
                    ViewModel.Camera = value;
            }
        }        

        VideoCapture _videoCapture;
        Object _videoCaptureLocker = new object();

        public ImageSensorControl()
        {
            InitializeComponent();
            Stop.Visibility = Visibility.Collapsed;
            DataContextChanged += ImageSensorControl_DataContextChanged;
            //            DataContextChanged += ImageSensorControl_DataContextChanged;
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            var mvDialog = new VisionSettingsView();
            mvDialog.DataContext = new VisionProfileViewViewModel(ViewModel.MachineRepo, ViewModel.Profile);
            mvDialog.Show();
        }



        //private void ImageSensorControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        //{
        //    if (ViewModel != null && ViewModel.Machine.IsInitialized)
        //    {
        //        ViewModel.Machine.Settings.PropertyChanged += Settings_PropertyChanged;
        //        ViewModel.Machine.PropertyChanged += (sndr, args) =>
        //        {
        //            if (args.PropertyName == nameof(ViewModel.Machine.Settings))
        //            {
        //                ViewModel.Machine.Settings.PropertyChanged += Settings_PropertyChanged;
        //            }
        //        };
        //    }
        //}

        //private void Settings_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        //{
        //    if (e.PropertyName == nameof(ViewModel.Machine.Settings.PositioningCamera))
        //    {
        //        StopCapture();
        //        if (ViewModel.Machine.Settings.PositioningCamera != null)
        //        {
        //            StartCapture();
        //        }
        //    }
        //}

        //private bool _running;
        //public bool CameraHasData;

        //private void CaptureLoop()
        //{
        //    Task.Run(async () =>
        //    {
        //        while (_running)
        //        {
        //            await Dispatcher.BeginInvoke(new Action(() =>
        //            {
        //                lock (_videoCaptureLocker)
        //                {
        //                    if (_videoCapture != null)
        //                    {
        //                        using (var originalFrame = _videoCapture.QueryFrame())
        //                        {
        //                            var result = PerformShapeDetection(null, originalFrame.ToImage<Bgr, byte>());
        //                        }

        //                    }
        //                }
        //            }));

        //            await Task.Delay(100);
        //        }
        //    });


        //}

        //private async Task InitCapture(int cameraIndex)
        //{
        //    await Task.Run(() =>
        //   {
        //       try
        //       {
        //           _videoCapture = new VideoCapture(cameraIndex);

        //           _videoCapture.Set(Emgu.CV.CvEnum.CapProp.AutoExposure, 0);

        //           _videoCapture.Set(Emgu.CV.CvEnum.CapProp.Brightness, 33);
        //           _videoCapture.Set(Emgu.CV.CvEnum.CapProp.Contrast, 54);
        //           _videoCapture.Set(Emgu.CV.CvEnum.CapProp.Exposure, -7);
        //       }
        //       catch (Exception ex)
        //       {
        //           Core.PlatformSupport.Services.Logger.AddException("ImageSensor_InitCapture", ex);
        //       }
        //   });
        //}

        //public async void StartCapture()
        //{
        //    lock (_videoCaptureLocker)
        //    {
        //        if (_videoCapture != null)
        //        {
        //            return;
        //        }
        //    }

        //    if (ViewModel.Machine.Settings.PositioningCamera == null)
        //    {
        //        MessageBox.Show("Please select a camera");
        //        new SettingsWindow(ViewModel.Machine, ViewModel.Machine.Settings, false, 2).ShowDialog();
        //        return;
        //    }

        //    try
        //    {
        //        LoadingMask.Visibility = Visibility.Visible;
        //        await InitCapture(ViewModel.Machine.Settings.PositioningCamera.CameraIndex);

        //        Play.Visibility = Visibility.Collapsed;
        //        Stop.Visibility = Visibility.Visible;

        //        CaptureLoop();
        //    }
        //    catch (NullReferenceException excpt)
        //    {
        //        MessageBox.Show(excpt.Message);
        //    }
        //    finally
        //    {
        //        LoadingMask.Visibility = Visibility.Collapsed;
        //    }

        //}

        //public void StopCapture()
        //{

        //    _running = false;

        //    lock (_videoCaptureLocker)
        //    {
        //        if (_videoCapture != null)
        //        {
        //            _videoCapture.Stop();
        //            _videoCapture = null;
        //        }
        //    }

        //    Play.Visibility = Visibility.Visible;
        //    Stop.Visibility = Visibility.Collapsed;

        //    var src = new BitmapImage();
        //    src.BeginInit();
        //    src.UriSource = new Uri("pack://application:,,/Imgs/TestPattern.jpg");
        //    src.CacheOption = BitmapCacheOption.OnLoad;
        //    src.EndInit();
        //    WebCamImage.Source = src;
        //}

        //public MainViewModel ViewModel
        //{
        //    get { return DataContext as MainViewModel; }
        //}

        //private void Stop_Click(object sender, RoutedEventArgs e)
        //{
        //    StopCapture();
        //}

        //public void ShutDown()
        //{
        //    lock (_videoCaptureLocker)
        //    {
        //        if (_videoCapture != null)
        //        {
        //            _videoCapture.Stop();
        //            _videoCapture = null;
        //        }
        //    }
        //}

        //private void Settings_Click(object sender, RoutedEventArgs e)
        //{
        //    new SettingsWindow(ViewModel.Machine, ViewModel.Machine.Settings, false, 2).ShowDialog();
        //}

        //private void Play_Click(object sender, RoutedEventArgs e)
        //{
        //    StartCapture();
        //}
    }
}