// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0c20a30e18ee070021ff21b85a3e757b80b3b809b1fe6b0cd8b985d9cbdcd5da
// IndexVersion: 2
// --- END CODE INDEX META ---
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class Signal
    {
        public List<ContactRef> Contacts { get; private set; }
        public List<PcbLine> Wires { get; private set; }

        public string Name { get; set; }

        public static Signal Create(XElement element)
        {
            var signal = new Signal()
            {
                Name = element.GetString("name"),
                Contacts = (from refs in element.Descendants("contactref") select ContactRef.Create(refs)).ToList(),
            };

            signal.Wires = (from childWires in element.Descendants("wire") select PcbLine.Create(childWires, signal.Name)).ToList();
           
            return signal;
        }

        public List<PcbLine> UnroutedWires
        {
            get { return Wires.Where(wire => wire.L == PCBLayers.Unrouted).ToList(); }
        }

        public List<PcbLine> TopWires
        {
            get { return Wires.Where(wire => wire.L == PCBLayers.TopCopper).ToList(); }
        }

        public List<PcbLine> BottomWires
        {
            get { return Wires.Where(wire => wire.L == PCBLayers.BottomCopper).ToList(); }
        }

        private List<Trace> FindTraces(List<PcbLine> unprocessedWires)
        {
            var traces = new List<Trace>();

            /* Grab first fire of list of wires not processed */
            var candidateWire = unprocessedWires.FirstOrDefault();
            while (candidateWire != null)
            {
                /* This wire is process so remove it */
                unprocessedWires.Remove(candidateWire);

                /* Create our new trace, add the first wire and add it to the traces */
                var trace = new Trace();
                trace.Wires.Add(candidateWire);
                traces.Add(trace);
                var newCandidates = new List<PcbLine>();

                /* Continue searching if we have a candidate coming in OR we have a new candidate that was put on the trace to review */
                while (candidateWire != null)
                {

                    var wiresProcessed = new List<PcbLine>();

                    foreach (var wire in unprocessedWires)
                    {

                        if (candidateWire.X2 == wire.X1 && candidateWire.Y2 == wire.Y1)
                        {
                            /* Add a new candidate to review */
                            newCandidates.Add(wire);

                            /* Make the junctions to the connected traces/wires */
                            candidateWire.EndJunctions.Add(wire);
                            wire.StartJunctions.Add(candidateWire);

                            /* Of course we add it to the trace */
                            trace.Wires.Add(wire);
                            wiresProcessed.Add(wire);
                        }

                        if (candidateWire.X1 == wire.X2 && candidateWire.Y1 == wire.Y2)
                        {

                            /* Add a new candidate to review */
                            newCandidates.Add(wire);

                            /* Make the junctions to the connected traces/wires */
                            candidateWire.StartJunctions.Add(wire);
                            wire.EndJunctions.Add(candidateWire);

                            /* Of course we add it to the trace */
                            trace.Wires.Add(wire);
                            wiresProcessed.Add(wire);
                        }

                        if (candidateWire.X2 == wire.X2 && candidateWire.Y2 == wire.Y2)
                        {

                            /* Add a new candidate to review */
                            newCandidates.Add(wire);

                            /* Make the junctions to the connected traces/wires */
                            candidateWire.EndJunctions.Add(wire);
                            wire.EndJunctions.Add(candidateWire);

                            /* Of course we add it to the trace */
                            trace.Wires.Add(wire);
                            wiresProcessed.Add(wire);
                        }

                        if (candidateWire.X1 == wire.X1 && candidateWire.Y1 == wire.Y1)
                        {
                            /* Add a new candidate to review */
                            newCandidates.Add(wire);

                            /* Make the junctions to the connected traces/wires */
                            candidateWire.StartJunctions.Add(wire);
                            wire.StartJunctions.Add(candidateWire);

                            /* Of course we add it to the trace */
                            trace.Wires.Add(wire);
                            wiresProcessed.Add(wire);
                        }
                    }

                    /* If the wire was added in this pass, remove it from the unprocessedWires list */
                    foreach (var wire in wiresProcessed)
                    {
                        if(wire.X2 == 34.925)
                        {
                            Debugger.Break();
                        }

                        unprocessedWires.Remove(wire);
                    }

                    /* If we have a new candidate review it */
                    candidateWire = newCandidates.FirstOrDefault();
                    if (candidateWire != null)
                    {
                        newCandidates.Remove(candidateWire);
                    }

                }

            
                /* Grab next wire...if any to be checked */
                candidateWire = unprocessedWires.FirstOrDefault();
            }

            return traces;
        }

        public List<Trace> TopTraces
        {
            get { return FindTraces(TopWires.ToList()); }
        }

        public List<Trace> BottomTraces
        {

            get { return FindTraces(BottomWires.ToList()); }
        }
    }
}
