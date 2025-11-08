// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8976a496bf732234f7c33e87140fbfc13972d0a7df03eb733d72291ce9bb91d7
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Via
    {
        public EntityHeader<PCBLayers> Layer { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double DrillDiameter { get; set; }
        public string Fill { get; set; }

        public static Via Create(XElement element)
        {
            return new Via()
            {
                Layer = EntityHeader<PCBLayers>.Create(PCBLayers.Vias),
                DrillDiameter = element.GetDouble("drill"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                Fill = ((int)PCBLayers.Vias).FromEagleColor()
            };
        }
    }
}
