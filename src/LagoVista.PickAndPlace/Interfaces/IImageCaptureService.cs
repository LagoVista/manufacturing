using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IImageCaptureService : IViewModel
    {
    //    LocatedByCamera Camera { get; set; }
        VisionProfile Profile { get; }
        CameraTypes CameraType { get; set; }
        MachineCamera Camera { get; }
        RelayCommand StartCaptureCommand { get; }
        RelayCommand StopCaptureCommand { get; }
        IMachineRepo MachineRepo { get; }
    }
}
