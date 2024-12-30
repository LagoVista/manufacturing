using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class FiducialViewModel : ViewModelBase
    {
        private readonly IMachine _machine;
        private readonly ILocatorViewModel _locatorViewMoel;

        public FiducialViewModel(IMachine machine, ILocatorViewModel locatorViewModel)
        {
            _locatorViewMoel = locatorViewModel;
            _machine = machine;
        }

        private string _status;
        public string Status
        {
            set => Set(ref _status, value);
            get => _status;
        }

    }
}
