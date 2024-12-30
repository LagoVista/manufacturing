using LagoVista.Core.Validation;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface IVisionManagerViewModel
    {
        InvokeResult SelectProfile(string profile);
    }
}
