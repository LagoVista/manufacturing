// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 515cc8264efb1a2898b13701608b96c36a28c9ecb9e966106c1ed93064c3d950
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
    public class MCode : GCodeCommand
    {
        public int Code;

        public override string ToString()
        {
            return $"{LineNumber}. - {Line}";
        }

        public override TimeSpan EstimatedRunTime
        {
            get { return TimeSpan.Zero; }
        }

        public override Vector3 CurrentPosition { get; set; }
       
        public override void SetComment(string comment)
        {
            switch (Command)
            {
                case "M6":
                case "M06": DrillSize = (String.IsNullOrEmpty(comment)) ? -1 : double.Parse(comment);  break;
            }
        }

        public double Power { get; set; }
     
        public double DrillSize { get; private set; }

        public String Tool { get; private set; }
    }
}
