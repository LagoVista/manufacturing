// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 518c0f9d620ebc4d1f477095c51b3af02e9fcba3ab8be137bc49476bc6376bf2
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.GCode.Commands
{
    public class GCodeDwell : GCodeCommand
    {
        public TimeSpan DwellTime { get; set; }

        public override TimeSpan EstimatedRunTime
        {
            get { return DwellTime; }
        }

        public override Vector3 CurrentPosition { get; set; }
    }
}
