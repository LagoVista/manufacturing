using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public class PlaceableParts : ModelBase
    {
        public int? Count { get => Parts == null ? 0 : Parts.Count; }

        ObservableCollection<PcbComponent> _parts = new ObservableCollection<PcbComponent>();
        public ObservableCollection<PcbComponent> Parts 
        {
            get => _parts;
            set
            {
                Set(ref _parts, value);
                RaisePropertyChanged(nameof(Count));
            }
        }

        private AutoFeeder _autoFeeder;
        public AutoFeeder AutoFeeder 
        {
            get => _autoFeeder;
            set => Set(ref _autoFeeder, value);
        }

        private StripFeeder _stripFeeder;
        public StripFeeder StripFeeder
        {
            get => _stripFeeder;
            set => Set(ref _stripFeeder, value);
        }

        StripFeederRow _stripFeederRow;
        public StripFeederRow StripFeederRow
        {
            get => _stripFeederRow;
            set => Set(ref _stripFeederRow, value);
        }

        bool _hasPart;
        public bool HasPart
        {
            get => _hasPart;
            set => Set(ref _hasPart, value);
        }

        private string _status;
        public string Status
        {
            get => _status;
            set => Set(ref _status, value);
        }

        public string PartsNames
        {
            get{ return string.Join(',', Parts.Select(prt => prt.Name));}
        }

        private string _value;
        public string Value
        {
            get => _value;
            set => Set(ref _value, value);
        }

        private string _packageName;
        public string PackageName
        {
            get => _packageName;
            set => Set(ref _packageName, value);
        }

        private string _packageId;
        public string PackageId
        {
            get => _packageId;
            set => Set(ref _packageName, value);
        }

        private string _componentName;
        public string ComponentName
        {
            get => _componentName;
            set => Set(ref _componentName, value);
        }
    }
}
