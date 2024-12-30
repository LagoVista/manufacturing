using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class VisionProfileViewViewModel : ViewModelBase
    {
        private readonly IMachineRepo _machineRepo;

        public VisionProfileViewViewModel(IMachineRepo machineRepo, VisionSettings settings)
        {
            _machineRepo = machineRepo;
            SaveCommand = new RelayCommand(() => Save());
            Profile = settings;
        }

        public async void Save()
        {
            await _machineRepo.SaveCurrentMachineAsync();
        }

        private VisionSettings _profile;
        public VisionSettings Profile 
        { 
            get => _profile; 
            set => Set(ref _profile, value);
        }

        public RelayCommand SaveCommand { get; }
    }
}
