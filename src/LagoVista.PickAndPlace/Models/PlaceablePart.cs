using LagoVista.PCB.Eagle.Models;
using System.Collections.ObjectModel;
using System.Linq;

namespace LagoVista.PickAndPlace.Models
{
    public class PlaceableParts
    {
        public int Count { get; set; }

        public ObservableCollection<PcbComponent> Parts {get; set;}
        
        public string Package { get; set; }
        public string Value { get; set; }

        public PartStrip PartStrip { get; set; }

        public string StripFeederPackage { get; set; }
        public string StripFeeder { get; set; }

        public string PartsNames
        {
            get
            {
                return string.Join(',', Parts.Select(prt => prt.Name));
            }
        }
    }
}
