using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface IVisionProfileManagerViewModel
    {
        InvokeResult SelectProfile(string profile);

        EntityHeader CurrentMVProfile { get; }

        VisionSettings TopCameraProfile { get; }
        VisionSettings BottomCameraProfile { get; }
    }
}
