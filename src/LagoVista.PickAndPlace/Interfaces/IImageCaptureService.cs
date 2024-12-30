using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IImageCaptureService : IViewModel
    {
        LocatedByCamera Camera { get; set; }
        VisionSettings Profile { get; }
        RelayCommand StartCaptureCommand { get; }
        RelayCommand StopCaptureCommand { get; }
        IMachineRepo MachineRepo { get; }
    }
}
