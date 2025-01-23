using LagoVista.Client.Core;
using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class DryRunViewModel : MachineViewModelBase, IDryRunViewModel
    {       
        public DryRunViewModel(IRestClient restClient, IPartsViewModel partsViewModel, IPickAndPlaceJobViewModel jobVM, IStorageService storage, IMachineRepo machineRepo) : base(machineRepo)
        {
            JobVM = jobVM ?? throw new ArgumentNullException(nameof (jobVM));
        }

        public IPickAndPlaceJobViewModel JobVM { get; }


        public RelayCommand GoToPartOnBoard { get; }
    }
}
