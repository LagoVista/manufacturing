// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3d4f4fd50bc68a370d7bd5d5077c062436d8091c07f2cbb904a4383553eb45b4
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PCB.Eagle.Models
{
    public class Trace
    {
        public Trace()
        {
            Wires = new List<PcbLine>();
        }

        public List<PcbLine> Wires { get; set; }
    }
}
