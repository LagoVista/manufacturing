using LagoVista.Core.Commanding;
using LagoVista.Core.Models.Drawing;
using LagoVista.Manufacturing.Models;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IMachineCalibrationViewModel : IMachineViewModelBase
    {
        MachineStagingPlate SelectedStagingPlate { get; set; }
        MachineFeederRail SelectedFeederRail { get; set; }
        MachineCamera SelectedMachineCamera { get; set; }
        ToolNozzleTip SelectedNozzleTip { get; set; }

        RelayCommand SetStagingPlateReferenceHole1LocationCommand { get; }
        RelayCommand SetStagingPlateReferenceHole2LocationCommand { get; }

        RelayCommand MoveToStagingPlateReferenceHole1LocationCommand { get; }
        RelayCommand MoveToStagingPlateReferenceHole2LocationCommand { get; }

        RelayCommand SetMachineFiducialCommand { get; }
        RelayCommand MoveToMachineFiducialCommand { get; }

        RelayCommand SetFirstAutoFeederOriginCommand { get; }
        RelayCommand MoveToFirstAutoFeederOriginCommand { get; }


        RelayCommand SetCameraLocationCommand { get; }
        RelayCommand MoveToCameraLocationCommand { get; }


        RelayCommand SetDefaultSafeMoveHeightCommand { get; }

        RelayCommand CaptureKnownLocationCommand { get; }
        RelayCommand MoveToKnownLocationCommand { get; }
    }
}
