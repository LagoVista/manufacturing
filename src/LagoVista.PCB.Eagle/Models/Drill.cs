// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7beb11597e2060bd5f808801af2a7b50656ead5c793d09a8998d122f3a391db3
// IndexVersion: 0
// --- END CODE INDEX META ---
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
