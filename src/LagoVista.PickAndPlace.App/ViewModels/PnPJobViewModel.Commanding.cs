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

            HomingCycleCommand = new RelayCommand(() => Machine.HomingCycle());

    
            GoToWorkHomeCommand = new RelayCommand(() => GotoWorkspaceHome());
            HomeViaOriginCommand = new RelayCommand(() => HomeViaOrigin());
            SetWorkHomeViaVisionCommand = new RelayCommand(() => SetWorkComeViaVision());
            SetWorkHomeCommand = new RelayCommand(() => Machine.SetWorkspaceHome());
            GoToPCBOriginCommand = new RelayCommand(() => GoToPCBOrigin());


          


            SetBoardOffsetCommand = new RelayCommand(SetBoardOffset, () => { return true; });
            ClearBoardOffsetCommand = new RelayCommand(ClearBoardOffset, () => { return true; });


            SetBottomCameraPositionCommand = new RelayCommand(SetBottomCamera, () => Machine.Connected);
            GoToMachineFiducialCommand = new RelayCommand(GotoMachineFiducial, () => Machine.Connected);
            SetMachineFiducialCommand = new RelayCommand(SetMachineFiducial, () => Machine.Connected);

            ExportBOMCommand = new RelayCommand(ExportBOM);
        }

        public RelayCommand HomingCycleCommand { get; private set; }
        public RelayCommand RefreshConfigurationPartsCommand { get; private set; }
        public RelayCommand PrintManualPlaceCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand GoToMachineFiducialCommand { get; private set; }
        public RelayCommand SetMachineFiducialCommand { get; private set; }
        public RelayCommand GoToPartOnBoardCommand { get; private set; }
        public RelayCommand GoToPartPositionInTrayCommand { get; private set; }
        public RelayCommand MoveToPreviousComponentInTapeCommand { get; set; }
        public RelayCommand MoveToNextComponentInTapeCommand { get; set; }
        public RelayCommand GoToWorkHomeCommand { get; set; }
        public RelayCommand GoToPCBOriginCommand { get; set; }
        public RelayCommand HomeViaOriginCommand { get; set; }
        public RelayCommand SetWorkHomeCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        

        public RelayCommand GoToRefHoleCommand { get; set; }
        public RelayCommand SetRefHoleCommand { get; set; }
        public RelayCommand GoToCurrentPartInStripCommand { get; set; }


        public RelayCommand SetBoardOffsetCommand { get; private set; }
        public RelayCommand ClearBoardOffsetCommand { get; private set; }
        public RelayCommand SetBottomCameraPositionCommand { get; private set; }

        public RelayCommand SetWorkHomeViaVisionCommand { get; private set; }
        public RelayCommand ExportBOMCommand { get; private set; }




    }
}
