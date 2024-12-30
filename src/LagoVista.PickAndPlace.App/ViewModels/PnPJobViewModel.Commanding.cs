﻿using LagoVista.Core.Commanding;
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
            PeformMachineAlignmentCommand = new RelayCommand(PerformMachineAlignment);

            GoToPartOnBoardCommand = new RelayCommand(async () => await GoToPartOnBoard());
            GoToPartPositionInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            HomingCycleCommand = new RelayCommand(() => Machine.HomingCycle());

            AlignBottomCameraCommand = new RelayCommand(() => AlignBottomCamera());

            ResetCurrentComponentCommand = new RelayCommand(ResetCurrentComponent, () => SelectedPartStrip != null);

            GoToWorkHomeCommand = new RelayCommand(() => GotoWorkspaceHome());
            HomeViaOriginCommand = new RelayCommand(() => HomeViaOrigin());
            SetWorkHomeViaVisionCommand = new RelayCommand(() => SetWorkComeViaVision());
            SetWorkHomeCommand = new RelayCommand(() => Machine.SetWorkspaceHome());
            GoToPCBOriginCommand = new RelayCommand(() => GoToPCBOrigin());

            MoveToPreviousComponentInTapeCommand = new RelayCommand(() => { }, () => PartsVM.CanMoveToReferenceHoleInTape() );
            MoveToNextComponentInTapeCommand = new RelayCommand(() => { }, () => PartsVM.CanMoveToNextInTape());


            RefreshConfigurationPartsCommand = new RelayCommand(PopulateConfigurationParts);
            
            GoToPartInTrayCommand = new RelayCommand(GoToPartPositionInTray);

            PlaceCurrentPartCommand = new RelayCommand(PlacePart, CanPlacePart);
            PlaceAllPartsCommand = new RelayCommand(PlaceAllParts, CanPlacePart);
            PausePlacmentCommand = new RelayCommand(PausePlacement, CanPausePlacement);

            SetFiducialCalibrationCommand = new RelayCommand((prm) => SetFiducialCalibration(prm));

            CalibrateBottomCameraCommand = new RelayCommand(() => CalibrateBottomCamera());

            AbortMVLocatorCommand = new RelayCommand(() => AbortMVLocator());


            SetBoardOffsetCommand = new RelayCommand(SetBoardOffset, () => { return true; });
            ClearBoardOffsetCommand = new RelayCommand(ClearBoardOffset, () => { return true; });

            GoToInspectPartRefHoleCommand = new RelayCommand(() => { }, () => SelectedInspectPart != null);

            SetInspectPartRefHoleCommand = new RelayCommand(() => { }, () => { return true; });

            GoToInspectedPartCommand = new RelayCommand(GoToFirstPartInPartsToPlace, () => SelectedInspectPart != null);

            SetBottomCameraPositionCommand = new RelayCommand(SetBottomCamera, () => Machine.Connected);
            GoToMachineFiducialCommand = new RelayCommand(GotoMachineFiducial, () => Machine.Connected);
            SetMachineFiducialCommand = new RelayCommand(SetMachineFiducial, () => Machine.Connected);

            ExportBOMCommand = new RelayCommand(ExportBOM);

            GoToFiducial1Command = new RelayCommand(() => GoToFiducial(1));
            GoToFiducial2Command = new RelayCommand(() => GoToFiducial(2));

            GoToRefHoleCommand = new RelayCommand(() => GoToRefPoint(), () => SelectedPartStrip != null);
            SetRefHoleCommand = new RelayCommand(async () => await SetRefPoint(), () => SelectedPartStrip != null);
            GoToCurrentPartInStripCommand = new RelayCommand(async () => await GoToCurrentPartInPartStrip(), () => SelectedPartStrip != null);
        }

        public RelayCommand HomingCycleCommand { get; private set; }
        public RelayCommand GoToCurrentPartCommand { get; private set; }
        public RelayCommand AddFeederCommand { get; private set; }
        public RelayCommand RefreshConfigurationPartsCommand { get; private set; }
        public RelayCommand PrintManualPlaceCommand { get; private set; }
        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand GoToMachineFiducialCommand { get; private set; }
        public RelayCommand SetMachineFiducialCommand { get; private set; }
        public RelayCommand GoToPartOnBoardCommand { get; private set; }
        public RelayCommand GoToPartPositionInTrayCommand { get; private set; }
        public RelayCommand SelectMachineFileCommand { get; private set; }
        public RelayCommand SelectBoardFileCommand { get; private set; }
        public RelayCommand RefreshBoardCommand { get; private set; }
        public RelayCommand ResetCurrentComponentCommand { get; set; }
        public RelayCommand MoveToPreviousComponentInTapeCommand { get; set; }
        public RelayCommand PausePlacmentCommand { get; set; }
        public RelayCommand MoveToNextComponentInTapeCommand { get; set; }
        public RelayCommand GoToWorkHomeCommand { get; set; }
        public RelayCommand GoToPCBOriginCommand { get; set; }
        public RelayCommand HomeViaOriginCommand { get; set; }
        public RelayCommand SetWorkHomeCommand { get; set; }
        public RelayCommand CloseCommand { get; set; }
        public RelayCommand PlaceCurrentPartCommand { get; set; }
        public RelayCommand PlaceAllPartsCommand { get; set; }
        public RelayCommand GoToPartInTrayCommand { get; private set; }
        public RelayCommand PeformMachineAlignmentCommand { get; private set; }
        public RelayCommand GoToFiducial1Command { get; private set; }
        public RelayCommand GoToFiducial2Command { get; private set; }
        public RelayCommand AlignBottomCameraCommand { get; private set; }
        public RelayCommand CalibrateBottomCameraCommand { get; private set; }
        public RelayCommand SetFiducialCalibrationCommand { get; private set; }


        public RelayCommand GoToRefHoleCommand { get; set; }
        public RelayCommand SetRefHoleCommand { get; set; }
        public RelayCommand GoToCurrentPartInStripCommand { get; set; }


        public RelayCommand SetBoardOffsetCommand { get; private set; }
        public RelayCommand ClearBoardOffsetCommand { get; private set; }
        public RelayCommand SetBottomCameraPositionCommand { get; private set; }

        public RelayCommand SetWorkHomeViaVisionCommand { get; private set; }
        public RelayCommand ExportBOMCommand { get; private set; }

        public RelayCommand AbortMVLocatorCommand { get; private set; }


        public bool CanPlacePart()
        {
            if (_selectedPart == null)
                return false;

            if (_isPlacingParts)
                return false;

            if (SelectedPart.StripFeederPackage == null)
                return false;

            return true;
        }

        public bool CanPausePlacement(Object obj)
        {
            return _isPlacingParts;
        }


        public bool CanSaveJob()
        {
            return _isDirty;
        }
    }
}
