// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 387436224a915c46f1ed8357d90a6d4a52d3c966f76bc6fd4cbaba7b4f36796d
// IndexVersion: 2
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LagoVista.Core.Models.Drawing;

namespace LagoVista.GCode.Commands
{
    public class GCodeProbe : GCodeCommand
    {
        public override TimeSpan EstimatedRunTime { get { return TimeSpan.FromSeconds(5); } }

        public override Vector3 CurrentPosition { get; set; }

        public double Feed { get; set; }

        public override string ToString()
        {
            return $"Probe Command {Command}";
        }
    }
}
