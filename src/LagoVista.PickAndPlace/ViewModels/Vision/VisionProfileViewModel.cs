﻿using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Models;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.Vision
{
    public class VisionProfileViewModel : MachineViewModelBase, IVisionProfileViewModel
    {

        public VisionProfileViewModel(IMachineRepo machineRepo) : base (machineRepo) 
        {
            VisionProfiles = new ObservableCollection<EntityHeader>(VisionProfile.DefaultVisionProfiles);
            SetPixelsPerMMCommand = new RelayCommand(SetPixelsPerMM, () => MeasuredMM.HasValue);
            CopyVisionProfileFromDefaultCommand = new RelayCommand(CopyVisionProfile, () => Camera.CurrentVisionProfile.Key != "default");
        }

        void SetPixelsPerMM()
        {
            if (MeasuredMM.HasValue && MeasuredMM.Value > 0)
            {
                Camera.CurrentVisionProfile.PixelsPerMM = 200 / MeasuredMM.Value;
            }
        }

        public string PolygonHelp { get { return "http://docs.opencv.org/2.4/doc/tutorials/imgproc/shapedescriptors/bounding_rects_circles/bounding_rects_circles.html?highlight=approxpolydp"; } }
        public string PolygonEpsilonHelp { get { return "Parameter specifying the approximation accuracy. This is the maximum distance between the original curve and its approximation"; } }

        public string HarrisCornerLink { get { return "http://docs.opencv.org/2.4/doc/tutorials/features2d/trackingmotion/harris_detector/harris_detector.html"; } }
        public string HarrisCornerApertureHelp { get { return "Apertur parameter for Sobel operation"; } }
        public string HarrisCornerBlockSizeString { get { return "Neighborhood Size"; } }
        public string HarrisCornerKHelp { get { return "Harris detector free parameter."; } }
        public string ContoursLink { get { return "https://docs.opencv.org/4.x/dc/dcf/tutorial_js_contour_features.html"; } }
        public string CannyLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/feature_detection.html"; } }
        public string CannyLink2 { get { return "https://en.wikipedia.org/wiki/Canny_edge_detector"; } }
        public string CannyLowThresholdHelp { get { return "Threshold for Line Detection"; } }
        public string CannyHighThresholdHelp { get { return "Recommended to be set to three times the lower threshold"; } }
        public string CannyHighThresholdTracksLowThresholdHelp { get { return "Force High Threshold to Map to 3x Low Threshold"; } }
        public string CannyApetureSizeHelp { get { return "The size of the Sobel kernel to be used internally"; } }
        public string CannyGradientHelp { get { return "a flag, indicating whether a more accurate  norm  should be used to calculate the image gradient magnitude ( L2gradient=true ), or whether the default  norm  is enough ( L2gradient=false )."; } }

        public string HoughLinesLink { get { return "http://docs.opencv.org/2.4/doc/tutorials/imgproc/imgtrans/hough_lines/hough_lines.html"; } }
        public string HoughLinesRHOHelp { get { return "The resolution of the parameter R in pixels."; } }
        public string HoughLinesThetaHelp { get { return "The resolution of the parameter Theta in Degrees."; } }
        public string HoughLinesThresholdHelp { get { return "The minimum number of intersections to detect a line."; } }
        public string HoughLinesMinLineHelp { get { return "The minimum number of points that can form a line. Lines with less than this number of points are disregarded."; } }
        public string HoughLinesMaxLineGapHelp { get { return "The maximum gap between two points to be considered in the same line."; } }

        public string HoughCirclesLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/feature_detection.html#houghcircles"; } }
        public string HoughCirclesDPHelp { get { return "Inverse ratio of the accumulator resolution to the image resolution. For example, if dp=1 , the accumulator has the same resolution as the input image. If dp=2 , the accumulator has half as big width and height"; } }
        public string HoughCirclesMinDistanceHelp { get { return "Minimum distance between the centers of the detected circles. If the parameter is too small, multiple neighbor circles may be falsely detected in addition to a true one. If it is too large, some circles may be missed."; } }
        public string HoughCirclesParam1Help { get { return "Higher threshold of the two passed to the Canny() edge detector (the lower one is twice smaller)."; } }
        public string HoughCirclesParam2Help { get { return " it is the accumulator threshold for the circle centers at the detection stage. The smaller it is, the more false circles may be detected. Circles, corresponding to the larger accumulator values, will be returned first."; } }
        public string HoughCirclesMinRadiusHelp { get { return "Minimum Radius"; } }
        public string HoughCirclesMaxRadiusHelp { get { return "Maximum Radius"; } }

        public string GaussianBlurLink { get { return "http://docs.opencv.org/2.4/modules/imgproc/doc/filtering.html?highlight=gaussianblur#cv2.GaussianBlur"; } }
        public string GaussianKSizeHelp { get { return "Gaussian kernel size.this is the supplied value for both X and Y.  This can be zero and it will be compueted from sigma."; } }
        public string GaussianSigmaHelp { get { return "Gaussian kernel standard deviation this is supplied for both the X and Y.."; } }
        

        void CopyVisionProfile()
        {
            var defaultProfile = Camera.VisionProfiles.FirstOrDefault(vp => vp.Key == VisionProfile.VisionProfile_Defauilt);
            var originalProfile = Camera.CurrentVisionProfile;
            Camera.CurrentVisionProfile = defaultProfile;
            Camera.CurrentVisionProfile.Key = originalProfile.Key;
            Camera.CurrentVisionProfile.Id = originalProfile.Id;
            Camera.CurrentVisionProfile.Name = originalProfile.Name;
        }

        ObservableCollection<EntityHeader> _visionProfiles;
        public  ObservableCollection<EntityHeader> VisionProfiles
        {
            get => _visionProfiles;
            set => Set(ref _visionProfiles, value);
        }

        public RelayCommand SaveCommand { get; }

        private string _selectedCameraDevicePath;
        public string SelectedCameraDevicePath
        {
            get { return _selectedCameraDevicePath; }
            set 
            { 
                if(value != "-1"  && value != Camera.CameraDevice?.Id)
                {
                    Camera.CameraDevice = CameraList.FirstOrDefault(x => x.Id == value);
                }

                Set(ref _selectedCameraDevicePath, value); 
            }
        }

        ObservableCollection<EntityHeader> _cameraList;
        public ObservableCollection<EntityHeader> CameraList
        {
            get => _cameraList;
            set => Set(ref _cameraList, value);
        }

        public string SelectedProfile
        {
            get => Camera.CurrentVisionProfile.Key;
            set
            {
                if(Camera.CurrentVisionProfile.Key != value)
                {
                    var profile = Camera.VisionProfiles.Where(pf => pf.Key == value).SingleOrDefault();
                    if (profile == null)
                    {
                        var defaultProfile = Camera.VisionProfiles.FirstOrDefault(vp => vp.Key == VisionProfile.VisionProfile_Defauilt);
                        var newProfile = JsonConvert.DeserializeObject<VisionProfile>(JsonConvert.SerializeObject(defaultProfile));
                        var ehProfile = VisionProfiles.FirstOrDefault(pf => pf.Key == value);
                        newProfile.Key = ehProfile.Key;
                        newProfile.Id = ehProfile.Id;
                        newProfile.Name = ehProfile.Text;
                        Camera.VisionProfiles.Add(newProfile);
                        Camera.CurrentVisionProfile = newProfile;
                    }
                    else
                        Camera.CurrentVisionProfile = profile;

                    RaisePropertyChanged(nameof(Profile));
                }

                CopyVisionProfileFromDefaultCommand.RaiseCanExecuteChanged();
            }
        }

        public VisionProfile Profile {  get => Camera.CurrentVisionProfile; }

        MachineCamera _camera;
        public MachineCamera Camera
        {
            get => _camera;
            set => Set(ref _camera, value);    
        }

        private double? _measuedMM;
        public double? MeasuredMM 
        {
            get => _measuedMM;
            set 
            {
                Set(ref _measuedMM, value);
                SetPixelsPerMMCommand.RaiseCanExecuteChanged();
            }
        }

        public RelayCommand SetPixelsPerMMCommand { get; }

        public RelayCommand CopyVisionProfileFromDefaultCommand { get; }
    }
}
