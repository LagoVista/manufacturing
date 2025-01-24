using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using LagoVista.PickAndPlace.Models;
using System;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PartInspectionViewModel : MachineViewModelBase, IPartInspectionViewModel, IRectangleLocatedHandler
    {
        private readonly ILocatorViewModel _locatorViewModel;

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

            _locatorViewModel.RegisterRectangleLocatedHandler(this);

            await Machine.GoToPartInspectionCameraAsync();

            return InvokeResult.Success;
        }

        public Task<InvokeResult> InspectAsync(Component component)
        {
            CurrentComponent = component;
            return InspectAsync();
        }

        public void RectangleLocated(MVLocatedRectangle rectangleLocated)
        {
            _locatorViewModel.UnregisterRectangleLocatedHandler(this);
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
