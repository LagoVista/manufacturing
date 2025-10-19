// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: aa63cd109b60cfa581f9232fe1d05dd3becaa1372b517756dedfefd3def87410
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PCB.Eagle.Models
{
    public class BOMEntry
    {
        public BOMEntry()
        {
            Components = new List<PcbComponent>();
        }

        public PcbPackage Package { get;  internal set; }

        public string Value { get; internal set; }

        public List<PcbComponent> Components { get; private set; }

        public override string ToString()
        {
            return $"{Package.Name} - {Value}, QTY: {Components.Count}";
        }
    }
}
