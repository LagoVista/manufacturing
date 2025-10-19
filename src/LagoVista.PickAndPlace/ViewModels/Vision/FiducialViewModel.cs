// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d3fd4a46bb7692b0917e4e16f1655204a15c1b1b365ff9fa50a3392bfcc1874a
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Vision;

namespace LagoVista.PickAndPlace.ViewModels.Vision
{
    public class FiducialViewModel : ViewModelBase, IFiducialViewModel
    {
        private readonly IMachineRepo _machineRepo;
        private readonly ILocatorViewModel _locatorViewMoel;

        public FiducialViewModel(IMachineRepo repo, ILocatorViewModel locatorViewModel)
        {
            _locatorViewMoel = locatorViewModel;
            _machineRepo = repo;
        }

        private string _status;
        public string Status
        {
            set => Set(ref _status, value);
            get => _status;
        }

    }
}
