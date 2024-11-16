using LagoVista.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Models
{
    public class ToolNozzle
    {
        public ToolNozzle()
        {
            Id = Guid.NewGuid().ToId();
        }

        public string Id { get; set; }

        public string Name { get; set; }
        public double SafeMoveHeight { get; set; }
        public double PickHeight { get; set; }
        public double BoardHeight { get; set; }
    }
}
