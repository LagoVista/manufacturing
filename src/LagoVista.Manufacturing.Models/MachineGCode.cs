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


        public string ParseLeftVacuumRegEx { get; set; }
        public string ParseRightVacuumRegEx { get; set; }
    }
}
