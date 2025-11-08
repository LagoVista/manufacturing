// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 743b70d12559c275cbc458a436598096e5d15ce6d4ee2a34e7e8f6ba642157ff
// IndexVersion: 2
// --- END CODE INDEX META ---
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
