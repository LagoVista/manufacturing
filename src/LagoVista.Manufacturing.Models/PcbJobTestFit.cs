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
