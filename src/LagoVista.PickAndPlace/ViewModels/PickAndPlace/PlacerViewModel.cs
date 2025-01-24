using LagoVista.Client.Core.Models;
using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class PlacerViewModel : MachineViewModelBase, IPlacerViewModel
    {
        IStripFeederViewModel _stripFeederViewModel;
        IAutoFeederViewModel _autoFeederViewModel;
        IFeederViewModel _feederViewModel;
        ILocatorViewModel _locatorViewModel;
       
        public PlacerViewModel(IJobManagementViewModel pnpJobVM, ILocatorViewModel locatorViewModel, IMachineRepo machineRepo,  IStripFeederViewModel stripFeederViewModel, IAutoFeederViewModel autoFeederViewModel) : base(machineRepo)
        {
            JobVM = pnpJobVM ?? throw new ArgumentNullException(nameof(pnpJobVM));
            _stripFeederViewModel = stripFeederViewModel ?? throw new ArgumentNullException(nameof(stripFeederViewModel));
            _autoFeederViewModel = autoFeederViewModel ?? throw new ArgumentNullException(nameof(autoFeederViewModel));
            _locatorViewModel = locatorViewModel ?? throw new ArgumentNullException(nameof(locatorViewModel));
        }

       


        public async Task PlaceComponent()
        {

            /* Cycle is:
             * 1) Verify part can be placed
             * 
             */
        }



        public async Task PlacePart()
        {
            /* Cycle is:
             * 1) Verify we have part in 
             * 2) Move to part
             * 3) Pick part
             * 4) Move to placement
             * 5) Place part
             * 6) Verify part is placed
             * 7) Move to next part
             */
        }



        public RelayCommand PlaceComponentCommand { get; }


        public IJobManagementViewModel JobVM { get; }
    }
}
