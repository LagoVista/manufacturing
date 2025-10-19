// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a4fc285036a433a4ad35ad5aaa2f3570813c11375c872711c43f7e577a9fcfbb
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public partial class MachineControlViewModel
    {
        private void InitCommands()
        {
            JogCommand = new RelayCommand((param) => Jog((JogDirections)param), CanJog);
            ResetCommand = new RelayCommand((param) => ResetAxisToZero((ResetAxis)param), CanResetAxis);

            SoftResetCommand = new RelayCommand(SoftReset, CanSoftReset);
            ClearAlarmCommand = new RelayCommand(ClearAlarm, CanClearAlarm);
            FeedHoldCommand = new RelayCommand(FeedHold, CanFeedHold);
            HomeCommand = new RelayCommand((param) => Home((HomeAxis)param), CanHome);
            CycleStartCommand = new RelayCommand(CycleStart, CanCycleStart);
            SetCameraCommand = new RelayCommand((param) => SetCamera(), CanSetCamera);
            SetTool1Command = new RelayCommand((param) => SetTool1(), CanSetTool1);

            MoveToBottomCameraCommand = new RelayCommand((obj) => MoveToBottomCamera(), CanJog);

            SetToMoveHeightCommand = new RelayCommand((obj) => SetToMoveHeight(), CanJog);
            SetToPickHeightCommand = new RelayCommand((obj) => SetToPickHeight(), CanJog);
            SetToPlaceHeightCommand = new RelayCommand((obj) => SetToBoardHeight(), CanJog);

            SetWorkspaceHomeCommand = new RelayCommand((obj) => _machineRepo.CurrentMachine.SetWorkspaceHome(), CanJog);
            GotoWorkspaceHomeCommand = new RelayCommand((obj) => _machineRepo.CurrentMachine.GotoWorkspaceHome(), CanJog);

            _machineRepo.PropertyChanged += _machineRepo_PropertyChanged;
            _machineRepo.CurrentMachine.PropertyChanged += Machine_PropertyChanged;
        }

        private void _machineRepo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            _machineRepo.CurrentMachine.PropertyChanged += Machine_PropertyChanged;
        }

        void SetToMoveHeight()
        {
            if (_machineRepo.CurrentMachine.Settings.FirmwareType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M54");
            }
            else
            {
                _machineRepo.CurrentMachine.SendCommand($"G0 Z{_machineRepo.CurrentMachine.Settings.SafMoveHeight} F5000");
            }
        }

        void SetToPickHeight()
        {
            if (_machineRepo.CurrentMachine.Settings.FirmwareType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M55");
            }
            else
            {
            
                _machineRepo.CurrentMachine.SendCommand($"G0 Z{_machineRepo.CurrentMachine.CurrentMachineToolHead.PickHeight} F5000");
            }
        }

        void SetToBoardHeight()
        {
            if (_machineRepo.CurrentMachine.Settings.FirmwareType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M56");
            }
            else
            {
            //    _machineRepo.CurrentMachine.SendCommand($"G0 Z{_machineRepo.CurrentMachine.Settings.ToolBoardHeight} F5000");
            }
        }

        void MoveToBottomCamera()
        {
            if (_machineRepo.CurrentMachine.Settings.FirmwareType == FirmwareTypes.LagoVista_PnP)
            {
                _machineRepo.CurrentMachine.SendCommand("M52");
            }
            else
            {
                //if (_machineRepo.CurrentMachine.Settings.PartInspectionCamera?.AbsolutePosition != null)
                //{
                //    _machineRepo.CurrentMachine.SendCommand($"G0 X{_machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.X} Y{_machineRepo.CurrentMachine.Settings.PartInspectionCamera.AbsolutePosition.Y} Z{_machineRepo.CurrentMachine.Settings.PartInspectionCamera.FocusHeight} F1{_machineRepo.CurrentMachine.Settings.FastFeedRate}");
                //}
            }
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(_machineRepo.CurrentMachine.Connected) ||
               e.PropertyName == nameof(_machineRepo.CurrentMachine.Mode))
            {
                SoftResetCommand.RaiseCanExecuteChanged();
                ClearAlarmCommand.RaiseCanExecuteChanged();
                FeedHoldCommand.RaiseCanExecuteChanged();
                CycleStartCommand.RaiseCanExecuteChanged();
                JogCommand.RaiseCanExecuteChanged();
                ResetCommand.RaiseCanExecuteChanged();
                HomeCommand.RaiseCanExecuteChanged();
                SetCameraCommand.RaiseCanExecuteChanged();
                SetTool1Command.RaiseCanExecuteChanged();

                GotoWorkspaceHomeCommand.RaiseCanExecuteChanged();
                SetWorkspaceHomeCommand.RaiseCanExecuteChanged();
                
                SetToMoveHeightCommand.RaiseCanExecuteChanged();
                SetToPickHeightCommand.RaiseCanExecuteChanged();
                SetToPlaceHeightCommand.RaiseCanExecuteChanged();
                MoveToBottomCameraCommand.RaiseCanExecuteChanged();
            }

            if(e.PropertyName == nameof(_machineRepo.CurrentMachine.ViewType))
            {
                SetCameraCommand.RaiseCanExecuteChanged();
                SetTool1Command.RaiseCanExecuteChanged();
            }

            if (e.PropertyName == nameof(_machineRepo.CurrentMachine.Settings))
            {
                /* Keep the saved values as temp vars since updating the StepMode will overwrite */
                var originalXYStepSize = _machineRepo.CurrentMachine.Settings.XYStepSize;
                var originalZStepSize = _machineRepo.CurrentMachine.Settings.ZStepSize;

                XYStepMode = _machineRepo.CurrentMachine.Settings.XYStepMode;
                ZStepMode = _machineRepo.CurrentMachine.Settings.ZStepMode;

                XYStepSizeSlider = originalXYStepSize;
                ZStepSizeSlider = originalZStepSize;
            }
        }

        public bool CanSetCamera(object param)
        {
            return _machineRepo.CurrentMachine.ViewType == ViewTypes.Tool;
        }

        public bool CanSetTool1(object param)
        {
            return _machineRepo.CurrentMachine.ViewType == ViewTypes.Camera;
        }

        public void SetCamera()
        {
            _machineRepo.CurrentMachine.SendCommand(SafeHeightGCodeGCode());
            _machineRepo.CurrentMachine.SetViewTypeAsync(ViewTypes.Camera);
        }

        private string SafeHeightGCodeGCode()
        {
            return $"G0 Z{_machineRepo.CurrentMachine.Settings.SafMoveHeight} F{_machineRepo.CurrentMachine.Settings.FastFeedRate}";
        }


        public void SetTool1()
        {
            _machineRepo.CurrentMachine.SendCommand(SafeHeightGCodeGCode());
            _machineRepo.CurrentMachine.SetViewTypeAsync(ViewTypes.Tool);
        }


        public bool CanHome(object param)
        {
            return _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanResetAxis(object param)
        {
            return _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanJog(object param)
        {
            return _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanCycleStart()
        {
            return _machineRepo.CurrentMachine.Connected;
        }

        public bool CanFeedHold()
        {
            return _machineRepo.CurrentMachine.Connected;
        }

        public bool CanSoftReset()
        {
            return _machineRepo.CurrentMachine.Connected;
        }

        public bool CanClearAlarm()
        {
            return _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.Mode == OperatingMode.Alarm;
        }

        public RelayCommand JogCommand { get; private set; }
        public RelayCommand HomeCommand { get; private set; }
        public RelayCommand ResetCommand { get; private set; }
        public RelayCommand SoftResetCommand { get; private set; }
        public RelayCommand ClearAlarmCommand { get; private set; }
        public RelayCommand FeedHoldCommand { get; private set; }
        public RelayCommand CycleStartCommand { get; private set; }

        public RelayCommand SetToMoveHeightCommand { get; private set; }
        public RelayCommand SetToPickHeightCommand { get; private set; }
        public RelayCommand SetToPlaceHeightCommand { get; private set; }
        public RelayCommand MoveToBottomCameraCommand { get; private set; }

        public RelayCommand SetCameraCommand { get; private set; }
        public RelayCommand SetTool1Command { get; private set; }

        public RelayCommand SetWorkspaceHomeCommand { get; private set; }
        public RelayCommand GotoWorkspaceHomeCommand { get; private set; }
    }
}
