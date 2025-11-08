// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7f322ec2f0d044618d93c0e42b3943defd4c47ec6681a2c6c8567822abd02c64
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System.Diagnostics;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineCoreActionsViewModel : MachineViewModelBase, IMachineCoreActionsViewModel, ICircleLocatedHandler
    {
        ILocatorViewModel _locatorViewModel;
        public MachineCoreActionsViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel) : base(machineRepo)
        {
            HomeCommand = new RelayCommand(Home, () => Machine != null && Machine.Connected);
            SkipHomeCommand = new RelayCommand(() => Machine.SetHomed(), () => Machine != null && Machine.Connected && !Machine.WasMachineHomed);
            RegisterCommandHandler(SkipHomeCommand);
            RegisterCommandHandler(HomeCommand);

            MachineVisionOriginCommand = new RelayCommand(MachineVisionOrigin, () => Machine != null && Machine.Connected && Machine.WasMachineHomed);
            RegisterCommandHandler(MachineVisionOriginCommand);

            GoToSafeMoveHeightCommand = CreatedMachineConnectedCommand(() => Machine.SendSafeMoveHeight());
            GoToPartInspectionCameraCommand = CreatedMachineConnectedCommand(GoToPartInspectionCamera);
            SetCameraNavigationCommand = CreatedMachineConnectedCommand(() => Machine.CurrentMachineToolHead = null, () => Machine.CurrentMachineToolHead != null );
            SetToolHeadNavigationCommand = RelayCommand<MachineToolHead>.Create((mh) => Machine.CurrentMachineToolHead = mh,  (mch) => { return Machine != null &&  Machine.WasMachinOriginCalibrated && Machine.Connected && Machine.CurrentMachineToolHead != mch; } );

            _locatorViewModel = locatorViewModel;
        }

        public async void GoToPartInspectionCamera()
        {
            await Machine.GoToPartInspectionCameraAsync();
        }

        protected override void MachineChanged(IMachine machine)
        {
            machine.PropertyChanged += Machine_PropertyChanged;
            base.MachineChanged(machine);
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.CurrentMachineToolHead))
            {
                RaiseCanExecuteChanged();
            }

            if (e.PropertyName == nameof(Machine.Busy))
            {
                RaiseCanExecuteChanged();
            }
        }

        protected override void RaiseCanExecuteChanged()
        {
            SetToolHeadNavigationCommand.RaiseCanExecuteChanged();
            base.RaiseCanExecuteChanged();
        }


        public void Home()
        {
            Machine.HomingCycle();
        }

        Stopwatch _originSW;

        public async void MachineVisionOrigin()
        {
            _originSW = Stopwatch.StartNew();
            Machine.DebugWriteLine("------------------------------------");
            Machine.DebugWriteLine("[MachineVisionOrigin] - Start");


            if (MachineConfiguration.MachineFiducial.IsOrigin())
            {
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, "Can not perform machine vision calibration.  No Machine Fiducial Set.  Please set Machine Fiducial on calibration tab.");
            }
            else
            {
                Machine.SetVisionProfile(Manufacturing.Models.CameraTypes.Position, VisionProfile.VisionProfile_MachineFiducual);
                Machine.SendSafeMoveHeight();
                await Machine.MoveToCameraAsync();
                Machine.GotoPoint(MachineConfiguration.MachineFiducial);
                await Machine.SpinUntilIdleAsync();
                Machine.DebugWriteLine($"[MachineVisionOrigin] - Moved into position {_originSW.Elapsed.TotalMilliseconds}ms"); 
                _locatorViewModel.RegisterCircleLocatedHandler(this);
            }
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            if (circle.Stabilized && !Machine.Busy)
            {
                if (circle.Centered)
                {
                    _locatorViewModel.UnregisterCircleLocatedHandler(this);
                    Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.Info, "Found origin");
                    Machine.WasMachinOriginCalibrated = true;
                    Machine.ResetMachineCoordinates(MachineConfiguration.MachineFiducial);
                    Machine.DebugWriteLine($"[MachineVisionOrigin] - Success {_originSW.Elapsed.TotalMilliseconds}ms");
                    Machine.DebugWriteLine("-----------------------------");

                }
                else
                {
                    Machine.GotoPoint(circle.OffsetMM, relativeMove: true);
                }

                circle.Reset();
            }
        }

        public void CircleLocatorTimeout()
        {
            _locatorViewModel.UnregisterCircleLocatedHandler(this);
            Machine.DebugWriteLine($"[MachineVisionOrigin] - Failed {_originSW.Elapsed.TotalMilliseconds}ms");
            Machine.DebugWriteLine("-----------------------------");
        }

        public void CircleLocatorAborted()
        {
            _locatorViewModel.UnregisterCircleLocatedHandler(this);
        }

        public RelayCommand HomeCommand { get;  }
        public RelayCommand SkipHomeCommand { get; }
        public RelayCommand MachineVisionOriginCommand { get; }
        public RelayCommand GoToSafeMoveHeightCommand { get; }
        public RelayCommand GoToPartInspectionCameraCommand { get; }

        public RelayCommand SetCameraNavigationCommand { get; }
        public RelayCommand<MachineToolHead> SetToolHeadNavigationCommand { get; }
    }
}
