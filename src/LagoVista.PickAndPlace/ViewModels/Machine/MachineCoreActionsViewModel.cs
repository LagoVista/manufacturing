using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineCoreActionsViewModel : MachineViewModelBase, IMachineCoreActionsViewModel, ICircleLocatedHandler
    {
        ILocatorViewModel _locatorViewModel;
        public MachineCoreActionsViewModel(IMachineRepo machineRepo, ILocatorViewModel locatorViewModel) : base(machineRepo)
        {
            HomeCommand = CreatedMachineConnectedCommand(Home);
            MachineVisionOriginCommand = CreatedMachineConnectedCommand(MachineVisionOrigin);

            _locatorViewModel = locatorViewModel;
        }

        public void Home()
        {
            Machine.HomingCycle();
        }

        public void MachineVisionOrigin()
        {
            if(MachineConfiguration.MachineFiducial.IsOrigin())
            {
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.FatalError, "Can not perform machine vision calibration.  No Machine Fiducial Set.  Please set Machine Fiducial on calibration tab.");
            }
            else
            {
                Machine.SetVisionProfile(Manufacturing.Models.CameraTypes.Position, VisionProfile.VisionProfile_MachineFiducual);
                Machine.SendSafeMoveHeight();
                Machine.GotoPoint(MachineConfiguration.MachineFiducial);
                _locatorViewModel.RegisterCircleLocatedHandler(this);
            }
        }

        public void CircleLocated(MVLocatedCircle circle)
        {
            if (circle.Centered)
            {
                _locatorViewModel.UnregisterCircleLocatedHandler(this);
                Machine.AddStatusMessage(Manufacturing.Models.StatusMessageTypes.Info, "Found origin");
            }
        }

        public void CircleLocatorTimeout()
        {
            _locatorViewModel.UnregisterCircleLocatedHandler(this);
        }

        public void CircleLocatorAborted()
        {
            _locatorViewModel.UnregisterCircleLocatedHandler(this);
        }

        public RelayCommand HomeCommand { get; set; }
        public RelayCommand MachineVisionOriginCommand { get; set; }
    }
}
