using LagoVista.Core.Validation;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class VisionManagerViewModel : IVisionManagerViewModel
    {
        private readonly IMachine _machine;

        public VisionManagerViewModel(IMachine machine)
        {
            _machine = machine;
        }

        public InvokeResult SelectProfile(string profile )
        {
            return InvokeResult.Success;
        }
    }
}
