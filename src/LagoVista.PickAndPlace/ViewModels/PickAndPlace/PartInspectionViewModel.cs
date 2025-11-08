// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 49f5c4d5344e15978f497da10068fd9cc84631f449c844e6c7169794b35fa448
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
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
            var sw = Stopwatch.StartNew();
            if(CurrentComponent == null)
            {
                return InvokeResult.FromError("Current Component is required.");
            }
 
            Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInspection);
            await Machine.GoToPartInspectionCameraAsync();
            await Machine.SpinUntilIdleAsync();
            Machine.DebugWriteLine($"[InspectAsync] - Moved to position: {sw.Elapsed.TotalMilliseconds}");
            sw.Restart();

            if (!CurrentComponent.ComponentPackage.Value.PickOffset.IsOrigin())
            {
                Machine.SetRelativeMode();
                Machine.SendCommand(CurrentComponent.ComponentPackage.Value.PickOffset.ToGCode());
                Machine.SetAbsoluteMode();
                await Machine.SpinUntilIdleAsync();
                Machine.DebugWriteLine($"[InspectAsync] - Adjusted focus height: {sw.Elapsed.TotalMilliseconds}");
                sw.Restart();
            }

            return InvokeResult.Success;
        }
       
        public async Task<InvokeResult> CenterPartAsync(Component component, PickAndPlaceJobPlacement placement, string algorithm, double? rotation = null)
        {
            var fullSW = Stopwatch.StartNew();
            var sw = Stopwatch.StartNew();
            Machine.DebugWriteLine("--------------------------");
            Machine.DebugWriteLine("[CenterPart]");

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
            Machine.DebugWriteLine($"[CenterPart] - Moved to Inspection Camera {sw.Elapsed.TotalMilliseconds} ms");

            if (!component.ComponentPackage.Value.PickOffset.IsOrigin())
            {
                Machine.SetRelativeMode();
                Machine.SendCommand(component.ComponentPackage.Value.PickOffset.ToGCode());
                Machine.SetAbsoluteMode();

                await Machine.SpinUntilIdleAsync();
                Machine.DebugWriteLine($"[CenterPart] - Adjusted focus height: {sw.Elapsed.TotalMilliseconds}");
                sw.Restart();
            }

            _corectionFactor = new Point2D<double>();

            if (component.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfileSource.Component, component.Id, component.PartInspectionVisionProfile);
            else if (component.ComponentPackage.Value.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfileSource.ComponentPackage, component.ComponentPackage.Id, component.ComponentPackage.Value.PartInspectionVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInspection);
           
            Machine.DebugWriteLine($"[CenterPart] - Set Vision Profile: {sw.Elapsed.TotalMilliseconds}");
            sw.Restart();

            _waitForCenter = new ManualResetEventSlim(false);
            _locatorViewModel.RegisterRectangleLocatedHandler(this,10000);

            placement.State = EntityHeader<PnPStates>.Create(PnPStates.Inspecting);

            await Task.Run(() =>
            {
                var attemptCount = 0;
                while (!_waitForCenter.IsSet && ++attemptCount < 1000)
                    _waitForCenter.Wait(25);
            });

            placement.State = EntityHeader<PnPStates>.Create(PnPStates.Inspected);

            _locatorViewModel.UnregisterRectangleLocatedHandler(this);

            var success = _waitForCenter.IsSet;
            _waitForCenter.Dispose();
            _waitForCenter = null;

            if(!success)
            {
                Machine.AddStatusMessage(StatusMessageTypes.Warning, "Couldn't find rectangle to center.");
                Machine.DebugWriteLine($"[CenterPart] - Did not find part: {fullSW.Elapsed.TotalMilliseconds}");
                sw.Restart();
            }
            else
            {
                Machine.DebugWriteLine($"[CenterPart] - Found Part: {fullSW.Elapsed.TotalMilliseconds}");
                sw.Restart();
           
                Machine.SendSafeMoveHeight();
                await Machine.SpinUntilIdleAsync();
                Machine.DebugWriteLine($"[CenterPart] - Moved back to Safe Height: {fullSW.Elapsed.TotalMilliseconds}");
                sw.Restart();
            }

            placement.PickErrorOffset = _corectionFactor;

            if(success)
                Machine.DebugWriteLine($"[CenterPart] SUCCESS: {sw.Elapsed.TotalMilliseconds} ms");
            else
                Machine.DebugWriteLine($"[CenterPart] FALED: {sw.Elapsed.TotalMilliseconds} ms");

            Machine.DebugWriteLine("--------------------------");

            return success ? InvokeResult.Success : InvokeResult.FromError("Timeout centering part.");
        }

        public async Task<InvokeResult> InspectAsync(Component component)
        {
            var sw = Stopwatch.StartNew();
            Machine.DebugWriteLine("--------------------------");
            Machine.DebugWriteLine("[InspectAsync]");

            CurrentComponent = component;

            if (component.ComponentPackage.Value.PartInspectionVisionProfile != null)
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfileSource.ComponentPackage, component.ComponentPackage.Id, component.ComponentPackage.Value.PartInspectionVisionProfile);
            else
                Machine.SetVisionProfile(CameraTypes.PartInspection, VisionProfile.VisionProfile_PartInspection);

            var result = await InspectAsync();

            if(result.Successful) 
                Machine.DebugWriteLine($"[InspectAsync] SUCCESS: Inspected in {sw.Elapsed.TotalMilliseconds}");
            else
                Machine.DebugWriteLine($"[InspectAsync] FAILED: Inspected in {sw.Elapsed.TotalMilliseconds}");

            Machine.DebugWriteLine("--------------------------");

            return result;
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
