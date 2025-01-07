using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IMachineCalibrationViewModel : IMachineViewModelBase
    {
        MachineStagingPlate SelectedStagingPlate { get; set; }
        MachineFeederRail SelectedFeederRail { get; set; }
        MachineToolHead SelectedToolHead { get; set; }
        MachineCamera SelectedMachineCamera { get; set; }
        ToolNozzleTip SelectedNozzleTip { get; set; }

        RelayCommand SetStagingPlateReferenceHole1LocationCommand { get;  }
        RelayCommand SetStagingPlateReferenceHole2LocationCommand { get; }

        RelayCommand MoveToStagingPlateReferenceHole1LocationCommand { get; }
        RelayCommand MoveToStagingPlateReferenceHole2LocationCommand { get; }

        RelayCommand SetMachineFiducialCommand { get; }
        RelayCommand MoveToMachineFiducialCommand { get; }

        RelayCommand SetFirstFeederOriginCommand { get; }
        RelayCommand MoveToFirstFeederOriginCommand { get; }


        RelayCommand SetDefaultToolReferencePointCommand { get;  }
        RelayCommand MoveToDefaultToolReferencePointCommand { get; }

        RelayCommand SetToolOffsetCommand { get;  }

        RelayCommand SetCameraLocationCommand { get;  }
        RelayCommand MoveToCameraLocationCommand { get; }

        Point2D<double> KnownLocation { get; }
        Point2D<double> Delta { get;  }

        RelayCommand CaptureKnownLocationCommand { get; }
    }
}
