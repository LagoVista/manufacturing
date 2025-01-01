using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IMachineCalibrationViewModel : IViewModel
    {
        public RelayCommand SetToolReferencePointCommand { get; }
        public Manufacturing.Models.Machine Machine { get; }
    }
}
