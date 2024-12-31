using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IVisionProfileViewModel
    {
        VisionSettings Profile { get; set; }
        MachineCamera Camera { get; set; }
    }
}
