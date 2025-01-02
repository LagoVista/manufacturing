using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineCalibrationViewModel : MachineViewModelBase, IMachineCalibrationViewModel
    {
        

        public MachineCalibrationViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
            SetDefaultToolReferencePointCommand = CreatedMachineConnectedCommand(() => MachineConfiguration.DefaultToolReferencePoint = Machine.MachinePosition.ToPoint2D());
            SetStagingPlateReferenceHole1LocationCommand = CreatedMachineConnectedCommand(() => SelectedStagingPlate.ReferenceHoleLocation1 = Machine.MachinePosition.ToPoint2D(), () => SelectedStagingPlate != null);
            SetStagingPlateReferenceHole2LocationCommand = CreatedMachineConnectedCommand(() => SelectedStagingPlate.ReferenceHoleLocation2 = Machine.MachinePosition.ToPoint2D(), () => SelectedStagingPlate != null);
            SetDefaultToolReferencePointCommand = CreatedMachineConnectedCommand(() => MachineConfiguration.DefaultToolReferencePoint = Machine.MachinePosition.ToPoint2D());
            SetToolOffsetCommand = CreatedMachineConnectedCommand(() => SelectedToolHead.Offset = Machine.MachinePosition.ToPoint2D() - MachineConfiguration.DefaultToolReferencePoint, () => SelectedToolHead != null);
        }

        MachineToolHead _selectedToolHead;
        public MachineToolHead SelectedToolHead
        {
            get => _selectedToolHead;
            set
            {
                Set(ref _selectedToolHead, value);
                RaiseCanExecuteChanged();
            }
        }

        MachineStagingPlate _selectedStagingPlate;
        public MachineStagingPlate SelectedStagingPlate
        { 
            get => _selectedStagingPlate;
            set
            {
                Set(ref _selectedStagingPlate, value);
                RaiseCanExecuteChanged();
            }
        }

        MachineFeederRail _selectedFeederRail;
        public MachineFeederRail SelectedFeederRail 
        {
            get => _selectedFeederRail;
            set
            {
                Set(ref _selectedFeederRail, value);
                RaiseCanExecuteChanged();
            }
        }        

        MachineCamera _selectedMachineCamera;
        public MachineCamera SelectedMachineCamera
        {
            get => _selectedMachineCamera;
            set => Set(ref _selectedMachineCamera, value);
        }

        ToolNozzleTip _selectedNozzleTip;
        public ToolNozzleTip SelectedNozzleTip
        {
            get => _selectedNozzleTip;
            set => Set(ref _selectedNozzleTip, value);  
        }

        public RelayCommand SetStagingPlateReferenceHole1LocationCommand { get; }
        public RelayCommand SetStagingPlateReferenceHole2LocationCommand { get; }
        public RelayCommand SetFirstFeederOriginCommand { get; }
        public RelayCommand SetDefaultToolReferencePointCommand { get; }
        public RelayCommand SetToolOffsetCommand { get; }
        public RelayCommand SetInspectionCameraLocation { get; }
    }
}
