using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PCB.Eagle.Models
{
    public class Drill
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double D { get; set; }

        public override string ToString()
        {
            return $"Drill X={X}, Y={Y}, Diameter={D}";
        }
    }
}
