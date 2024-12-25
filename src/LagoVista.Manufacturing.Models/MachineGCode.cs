using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    public class MachineGCode
    {
        public string TopLightOn { get; set; }
        public string BottmLightOn { get; set; }

        public string TopLightOff { get; set; }
        public string BottmLightOff { get; set; }


        public string LeftVacuumOn { get; set; }
        public string RightVacuumOn { get; set; }


        public string ReadLeftVacuumCmd { get; set; }
        public string ReadRightVacuumCmd { get; set; }


        public string LeftVacuumResponseExample { get; set; }
        public string ParseLeftVacuumRegEx { get; set; }

        public string RightVacuumResponseExample { get; set; }
        public string ParseRightVacuumRegEx { get; set; }

        public string HomeCommandAll { get; set; }

        public string HomeCommandX { get; set; }
        public string HomeCommandY { get; set; }
        public string HomeCommandZ { get; set; }

        public string RequestStatusCommand { get; set; }
        public string StatusRegEx { get; set; }
        public string StatusResponseExample { get; set; }
        
    }
}
