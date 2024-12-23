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
