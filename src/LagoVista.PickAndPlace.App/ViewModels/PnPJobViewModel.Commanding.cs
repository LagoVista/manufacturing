using LagoVista.Core.Commanding;
using System;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel
    {
        private void AddCommands()
        {
            SaveCommand = new RelayCommand(async () => await SaveJobAsync());
            CloseCommand = new RelayCommand(Close);
            
            PrintManualPlaceCommand = new RelayCommand(PrintManualPlace);

            GoToPartOnBoardCommand = new RelayCommand(async () => await GoToPartOnBoard());
            GoToPartPositionInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            HomingCycleCommand = new RelayCommand(() => _machineRepo.CurrentMachine.HomingCycle());

    
            GoToWorkHomeCommand = new RelayCommand(() => GotoWorkspaceHome());
            HomeViaOriginCommand = new RelayCommand(() => HomeViaOrigin());
            SetWorkHomeViaVisionCommand = new RelayCommand(() => SetWorkComeViaVision());
            SetWorkHomeCommand = new RelayCommand(() => _machineRepo.CurrentMachine.SetWorkspaceHome());
            GoToPCBOriginCommand = new RelayCommand(() => GoToPCBOrigin());



            SetBoardOffsetCommand = new RelayCommand(SetBoardOffset, () => { return true; });
            ClearBoardOffsetCommand = new RelayCommand(ClearBoardOffset, () => { return true; });


            //SetBottomCameraPositionCommand = new RelayCommand(SetBottomCamera, () => _machineRepo.Connected);
            GoTo_machineFiducialCommand = new RelayCommand(GotoMachineFiducial, () => _machineRepo.CurrentMachine.Connected);
            Set_machineFiducialCommand = new RelayCommand(SeMachineFiducial, () => _machineRepo.CurrentMachine.Connected);

            ExportBOMCommand = new RelayCommand(ExportBOM);
        }

        public RelayCommand HomingCycleCommand { get; private set; }
        public RelayCommand RefreshConfigurationPartsCommand { get; private set; }
        public RelayCommand PrintManualPlaceCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand GoTo_machineFiducialCommand { get; private set; }
        public RelayCommand Set_machineFiducialCommand { get; private set; }
        public RelayCommand GoToPartOnBoardCommand { get; private set; }
        public RelayCommand GoToPartPositionInTrayCommand { get; private set; }
        public RelayCommand MoveToPreviousComponentInTapeCommand { get; set; }
        public RelayCommand MoveToNextComponentInTapeCommand { get; set; }
        public RelayCommand GoToWorkHomeCommand { get; set; }
        public RelayCommand GoToPCBOriginCommand { get; set; }
        public RelayCommand HomeViaOriginCommand { get; set; }
        public RelayCommand SetWorkHomeCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        

        public RelayCommand SetBoardOffsetCommand { get; private set; }
        public RelayCommand ClearBoardOffsetCommand { get; private set; }
        public RelayCommand SetBottomCameraPositionCommand { get; private set; }

        public RelayCommand SetWorkHomeViaVisionCommand { get; private set; }
        public RelayCommand ExportBOMCommand { get; private set; }




    }
}
