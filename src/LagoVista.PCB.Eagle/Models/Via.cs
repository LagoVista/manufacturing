using LagoVista.Core.Models;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Via
    {
        public EntityHeader<PCBLayers> Layer { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double OriginX { get; set; }
        public double OriginY { get; set; }
        public double DrillDiameter { get; set; }

        public static Via Create(XElement element)
        {
            return new Via()
            {
                Layer = EntityHeader<PCBLayers>.Create(PCBLayers.Vias),
                DrillDiameter = element.GetDouble("drill"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y"),
                OriginX = element.GetDouble("x"),
                OriginY = element.GetDouble("y")
            };
        }
    }
}
