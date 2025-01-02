using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IVisionProfileViewModel
    {
        VisionProfile Profile { get; }
        MachineCamera Camera { get; set; }
        string SelectedCameraDevicePath { get; set; }
        ObservableCollection<EntityHeader> CameraList { get; set; }
    }
}
