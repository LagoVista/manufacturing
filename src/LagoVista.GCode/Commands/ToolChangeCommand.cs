// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c018504c684421cded755ea7e57617bcf3413fd425729d5a13c47e5ea0b734d1
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
    public class ToolChangeCommand : GCodeCommand
    {
        public override Vector3 CurrentPosition { get; set; }
       
        public override TimeSpan EstimatedRunTime
        {
            get { return TimeSpan.Zero; }
        }

        public string ToolName { get; set; }

        public string ToolSize { get; set; }

        public override string ToString()
        {
            return $"{LineNumber}. - {Line} Set Tool: {ToolName}, ToolSize: {ToolSize}";
        }

        public override void SetComment(string comment)
        {
            ToolSize = comment.Trim();
        }
    }
}
