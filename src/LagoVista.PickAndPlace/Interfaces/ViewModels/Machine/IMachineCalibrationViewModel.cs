using LagoVista.Core.Commanding;
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

        RelayCommand SetFirstFeederOriginCommand { get; }

        RelayCommand SetDefaultToolReferencePointCommand { get;  }

        RelayCommand SetToolOffsetCommand { get;  }
        RelayCommand SetInspectionCameraLocation { get;  }
    }
}
