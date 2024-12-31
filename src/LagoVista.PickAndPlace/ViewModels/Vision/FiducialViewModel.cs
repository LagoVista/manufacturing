using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
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
