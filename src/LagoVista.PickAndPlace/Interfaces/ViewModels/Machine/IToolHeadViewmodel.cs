using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IToolHeadViewModel : IMachineViewModelBase
    {
        RelayCommand SetPartPickedVacuumCommand { get; }
        RelayCommand SetNoPartPickedVacuumCommand { get; }
        RelayCommand SetIdleVacuumCommand { get; }
        RelayCommand SetDefaultOriginCommand { get; }
        RelayCommand CaptureKnownLocationCommand { get; }
        RelayCommand MoveToKnownLocationCommand { get; }
        RelayCommand SetToolOffsetCommand { get; }
        RelayCommand ToggleCaptureToolHeadCalibrationCommand {get;}
        RelayCommand SetToolHeadCalibrationCommand { get; }

        Point2D<double> KnownLocation { get; }
        Point2D<double> Delta { get; }


        MachineToolHead Current { get; set; }
    }
}
