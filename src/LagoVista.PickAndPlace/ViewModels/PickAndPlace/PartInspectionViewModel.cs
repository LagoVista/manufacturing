﻿using LagoVista.Core.Models;
using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
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

        Point2D<double> _corectionFactor;

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

            if (!CurrentComponent.ComponentPackage.Value.PickOffset.IsOrigin())
            {
                Machine.SetRelativeMode();
                Machine.SendCommand(CurrentComponent.ComponentPackage.Value.PickOffset.ToGCode());
                Machine.SetAbsoluteMode();
            }

            return InvokeResult.Success;
        }
       
        public async Task<InvokeResult> CenterPartAsync(Component component, PickAndPlaceJobPlacement placement, string algorithm, double? rotation = null)
        {
            lock (this)
            {
                if (_waitForCenter != null)
                {
                    return InvokeResult.FromError("Already attempting to center part.");
                }

                _waitForCenter = new ManualResetEventSlim(false);
            }

            await Machine.GoToPartInspectionCameraAsync(rotation);
            await Machine.SpinUntilIdleAsync();
         
            if (!component.ComponentPackage.Value.PickOffset.IsOrigin())
            {
                Machine.SetRelativeMode();
                Machine.SendCommand(component.ComponentPackage.Value.PickOffset.ToGCode());
                Machine.SetAbsoluteMode();
            }

            _corectionFactor = new Point2D<double>();

            if (component.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfileSource.Component, component.Id, component.PartInspectionVisionProfile);
            else if (component.ComponentPackage.Value.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfileSource.ComponentPackage, component.ComponentPackage.Id, component.ComponentPackage.Value.PartInspectionVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInspection);

            _waitForCenter = new ManualResetEventSlim(false);
            _locatorViewModel.RegisterRectangleLocatedHandler(this,10000);

            placement.State = EntityHeader<PnPStates>.Create(PnPStates.Inspecting);

            await Task.Run(() =>
            {
                var attemptCount = 0;
                while (!_waitForCenter.IsSet && ++attemptCount < 1000)
                    _waitForCenter.Wait(25);

                Debug.WriteLine($"Did we get location? {_waitForCenter.IsSet}");
            });

            placement.State = EntityHeader<PnPStates>.Create(PnPStates.Inspected);

            _locatorViewModel.UnregisterRectangleLocatedHandler(this);

            var success = _waitForCenter.IsSet;
            _waitForCenter.Dispose();
            _waitForCenter = null;

            if(!success)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Couldn't find rectangle to center.");
            }

            placement.PickErrorOffset = _corectionFactor;
            
            return success ? InvokeResult.Success : InvokeResult.FromError("Timeout centering part.");
        }

        public Task<InvokeResult> InspectAsync(Component component)
        {
            CurrentComponent = component;

            if (component.ComponentPackage.Value.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfileSource.ComponentPackage, component.ComponentPackage.Id, component.ComponentPackage.Value.PartInspectionVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInspection);

            return InspectAsync();
        }

        public void RectangleLocated(MVLocatedRectangle rectangleLocated)
        {
            if (rectangleLocated.Centered)
            {
                if (rectangleLocated.Stabilized)
                {
                    _locatorViewModel.UnregisterRectangleLocatedHandler(this);
                    _waitForCenter.Set();
                }
            }
            else
            {
                if (rectangleLocated.Stabilized)
                {
                    Machine.SetRelativeMode();
                    Machine.SendCommand($"G0  X{rectangleLocated.OffsetMM.X},Y{rectangleLocated.OffsetMM.Y}");
                    var offset = new Point2D<double>(rectangleLocated.OffsetMM.X * 0.8, rectangleLocated.OffsetMM.Y * 0.75);
                    _corectionFactor += offset;
                   
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
