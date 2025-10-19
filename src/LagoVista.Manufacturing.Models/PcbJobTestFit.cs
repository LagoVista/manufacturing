// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: b614812146ed0cfa47db385a41d51733f3d48a9e82d3517f373fb39c02e390d2
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using System.Collections.ObjectModel;

namespace LagoVista.Manufacturing.Models
{
    public class PcbJobTestFit : ModelBase
    {
        ObservableCollection<BoardFiducial> _fiducials = new ObservableCollection<BoardFiducial>();
        public ObservableCollection<BoardFiducial> Fiducials 
        {
            get => _fiducials;
            set => Set(ref _fiducials, value);
        } 

        ObservableCollection<PartsGroup> _placedParts = new ObservableCollection<PartsGroup>();
        public ObservableCollection<PartsGroup> PlacedPartsGroups 
        {
            get => _placedParts;
            set => Set(ref _placedParts, value);
        }

        ObservableCollection<PartsGroup> _manulParts = new ObservableCollection<PartsGroup>();
        public ObservableCollection<PartsGroup> ManualPartGroups
        {
            get => _manulParts;
            set => Set(ref _manulParts, value);
        }

        private ObservableCollection<string> _errors = new ObservableCollection<string>();
        public ObservableCollection<string> Errors
        {
            get => _errors;
            set => Set(ref _errors, value);
        }

        private ObservableCollection<string> _warnings = new ObservableCollection<string>();
        public ObservableCollection<string> Warnings
        {
            get => _warnings;
            set => Set(ref _warnings, value);
        }
    }
}
