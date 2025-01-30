using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IVisionProfileViewModel : IViewModel
    {
        VisionProfile Profile { get; }
        MachineCamera Camera { get; set; }
        string SelectedCameraDevicePath { get; set; }
        ObservableCollection<EntityHeader> CameraList { get; set; }

        double? MeasuredMM { get; set; }

        RelayCommand SetPixelsPerMMCommand { get; }
        RelayCommand SaveCommand { get; }
        RelayCommand CopyVisionProfileFromDefaultCommand { get; }

    }
}
