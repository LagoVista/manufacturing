using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    public class CircuitBoardRevision
    {

        public List<PcbComponent> PcbComponents { get; set; } = new List<PcbComponent>();
    }

    public class CircuitBoardVarient
    {
        public string PartName { get; set; }

        

        public EntityHeader Component { get; set; }
    }
}
