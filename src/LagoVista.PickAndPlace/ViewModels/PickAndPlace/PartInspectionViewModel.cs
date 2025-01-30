﻿using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PartInspectionViewModel : MachineViewModelBase, IPartInspectionViewModel, IRectangleLocatedHandler
    {
        private readonly ILocatorViewModel _locatorViewModel;
        ManualResetEventSlim _waitForCenter;

        public PartInspectionViewModel(ILocatorViewModel locatorViewModel, IMachineRepo machineRepo) : base(machineRepo)
        {
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
        }

        Component _currentComponent;
        public Component CurrentComponent
        {
            get => _currentComponent;
            set => Set(ref _currentComponent, value);
        }

        public async Task<InvokeResult> InspectAsync()
        {
            if(CurrentComponent == null)
            {
                return InvokeResult.FromError("Current Component is required.");
            }

            
            Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInspection);
            await Machine.GoToPartInspectionCameraAsync();

            return InvokeResult.Success;
        }
       
        public async Task<InvokeResult> CenterPartAsync(Component component, PickAndPlaceJobPlacement placement, string algorithm)
        {
            if (component.ComponentPackage.Value.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, component.ComponentPackage.Value.PartInspectionVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartOnBoard);

            _waitForCenter = new ManualResetEventSlim(false);
            _locatorViewModel.RegisterRectangleLocatedHandler(this);

            await Task.Run(() =>
            {
                var attemptCount = 0;
                while (!_waitForCenter.IsSet && ++attemptCount < 200)
                    _waitForCenter.Wait(25);

                Debug.WriteLine($"Did we get location? {_waitForCenter.IsSet}");
            });

            _locatorViewModel.UnregisterRectangleLocatedHandler(this);

            var success = _waitForCenter.IsSet;
            _waitForCenter.Dispose();
            _waitForCenter = null;

            if(!success)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Couldn't find rectangle to center.");
            }

            return success ? InvokeResult.Success : InvokeResult.FromError("Timeout centering part.");
        }

        public Task<InvokeResult> InspectAsync(Component component)
        {
            CurrentComponent = component;

            if (component.ComponentPackage.Value.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, component.ComponentPackage.Value.PartInspectionVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartOnBoard);

            return InspectAsync();
        }

        public void RectangleLocated(MVLocatedRectangle rectangleLocated)
        {
            if (rectangleLocated.Centered)
            {
                if (rectangleLocated.FoundCount > 5)
                {
                    _locatorViewModel.UnregisterRectangleLocatedHandler(this);
                    _waitForCenter.Set();
                }
            }
            else
            {
                if (rectangleLocated.FoundCount > 5)
                {
                    Machine.SetRelativeMode();
                    Machine.SendCommand($"G0  X{rectangleLocated.OffsetMM.X},Y{rectangleLocated.OffsetMM.Y}");
                    Machine.SetAbsoluteMode();
                    rectangleLocated.Reset();
                }
            }
        }

        public void RectangleLocatorTimeout()
        {
            _locatorViewModel.UnregisterRectangleLocatedHandler(this);
        }

        public void RectangleLocatorAborted()
        {
            _locatorViewModel.UnregisterRectangleLocatedHandler(this);
        }
    }
}
