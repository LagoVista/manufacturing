// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c7587233c8c1ee028a147e570df15f8aeb9804938c4c4c33ac781121890498a3
// IndexVersion: 2
// --- END CODE INDEX META ---
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class PrintedCircuitBoard
    {
        public Plain Plain { get; set; }
        public List<PcbLayer> Layers { get; set; } = new List<PcbLayer>();
        public List<PcbPackage> Packages { get; set; } = new List<PcbPackage>();
        public List<PcbComponent> Components { get; set; } = new List<PcbComponent>();
        public List<Via> Vias { get; set; } = new List<Via>();
        public List<Signal> Signals { get; set; } = new List<Signal>();
        public List<Hole> Holes { get; set; } = new List<Hole>();
        public List<PcbLine> Outline { get; set; } = new List<PcbLine>();

        public double Width { get; set; }
        public double Height { get; set; }

        public List<Fiducial> Fiducials { get; set; } = new List<Fiducial>();

        public List<Drill> Drills
        {
            get
            {
                var drillsLayer = Layers.Where(layer => layer.Layer == PCBLayers.Drills).FirstOrDefault();
                if(drillsLayer == null)
                {
                    return new List<Drill>();
                }
                var drills = Layers.Where(layer => layer.Layer == PCBLayers.Drills).FirstOrDefault().Drills;
                foreach (var via in Vias)
                {
                    var existingDrill = drills.Where(drl => drl.X == via.X && drl.Y == via.Y);

                    /* Vias have drills/holes on top and bottom, only need one */
                    if (!existingDrill.Any())
                    {
                        drills.Add(new Drill() { X = via.X, Y = via.Y, D = via.DrillDiameter });
                    }
                }

                var drillFromHolesLayer = Layers.Where(layer => layer.Layer == PCBLayers.Drills).FirstOrDefault().Drills;
                drills.AddRange(drillFromHolesLayer);

                return drills;
            }
        }

        public List<DrillBit> OriginalToolRack
        {
            get
            {
                /* Probably get this down to about 25% of the lines w/ effective linq...in a hurry KDW 2017-03-15 */
                var drills = Drills.GroupBy(drl => drl.D);
                var bits = new List<DrillBit>();
                var toolIndex = 1;

                foreach (var drill in drills)
                {
                    bits.Add(new DrillBit()
                    {
                        Diameter = drill.First().D,
                    });
                }

                var orderedBits = bits.OrderBy(drl => drl.Diameter).ToList();
                foreach (var bit in orderedBits)
                {
                    bit.ToolName = $"T{toolIndex++:00}";
                }

                return orderedBits;
            }
        }

        public List<PcbLine> UnroutedWires { get; set; }

        public List<PcbLine> TopWires { get; set; }

        public List<PcbLine> BottomWires { get; set; }
    }
}
