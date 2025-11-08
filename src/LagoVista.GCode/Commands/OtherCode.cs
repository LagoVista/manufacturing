// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 52d08cdf488592a5aaaec333d0f2a14e07a8933d2ddcd38c794b2f903796e502
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
    public class OtherCode : GCodeCommand
    {
        public override Vector3 CurrentPosition { get; set; }
       
        public override TimeSpan EstimatedRunTime
        {
            get { return TimeSpan.Zero; }
        }
    }
}
