using System;
using System.Linq;
using System.Xml.Linq;
using LagoVista;
using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Models;

namespace LagoVista.PCB.Eagle.Managers
{
    public class EagleParser
    {

        public static Models.PrintedCircuitBoard ReadPCB(XDocument doc)
        {
            var pcb = new Models.PrintedCircuitBoard();

            pcb.Components = (from eles
                           in doc.Descendants("element")
                              select Models.PcbComponent.Create(eles)).ToList();

            pcb.Layers = (from eles
                           in doc.Descendants("layer")
                          select Models.PcbLayer.Create(eles)).ToList();

            pcb.Packages = (from eles
                           in doc.Descendants("package")
                            select Models.PcbPackage.Create(eles)).ToList();

            pcb.Plain = (from eles
                         in doc.Descendants("plain")
                         select Models.Plain.Create(eles)).First();

            pcb.Vias = (from eles
                        in doc.Descendants("via")
                        select Models.Via.Create(eles)).ToList();

            pcb.Signals = (from eles
                        in doc.Descendants("signal")
                           select Models.Signal.Create(eles)).ToList();

            foreach (var layer in pcb.Layers)
            {
                layer.Wires = pcb.Plain.Wires.Where(wire => wire.L == layer.Layer).ToList();             
            }

                /* FIrst assign packages to components */
            foreach (var element in pcb.Components)
            {
                var pck = pcb.Packages.Where(pkg => pkg.LibraryName == element.LibraryName && pkg.Name == element.PackageName).FirstOrDefault();

                element.Package = EntityHeader<PcbPackage>.Create(pck);

                foreach (var layer in pcb.Layers)
                {
                    if (layer.Layer == PCBLayers.Pads)
                    {
                        foreach (var pad in element.Package.Value.Pads)
                        {
                            var rotatedPad = pad.ApplyRotation(element.Rotation);
                            layer.Pads.Add(new Models.Pad() { D = rotatedPad.D, X = element.X.Value + rotatedPad.X, Y = element.Y.Value + rotatedPad.Y, A = pad.A });
                        }
                    }

                    if (layer.Layer == Models.PCBLayers.Drills)
                    {
                        foreach (var hole in element.Package.Value.Pads)
                        {
                            var rotatedHole = hole.ApplyRotation(element.Rotation);
                            layer.Drills.Add(new Models.Drill() { D = hole.D, X = element.X.Value + rotatedHole.X, Y = element.Y.Value + rotatedHole.Y });
                        }
                    }

                    if (layer.Layer == Models.PCBLayers.Holes)
                    {
                        foreach(var hole in element.Holes)
                        {
                            layer.Holes.Add(new Models.Hole() { D = hole.D, X = hole.X, Y = hole.Y });
                        }
                    }
                }
            }

            pcb.UnroutedWires = new System.Collections.Generic.List<Models.PcbLine>();
            pcb.TopWires = new System.Collections.Generic.List<Models.PcbLine>();
            pcb.BottomWires = new System.Collections.Generic.List<Models.PcbLine>();

            foreach(var signal in pcb.Signals)
            {
                pcb.UnroutedWires.AddRange(signal.UnroutedWires);
                pcb.TopWires.AddRange(signal.TopWires);
                pcb.BottomWires.AddRange(signal.BottomWires);
            }

            var outlineWires = pcb.Layers.Where(layer => layer.Layer == PCBLayers.BoardOutline).FirstOrDefault().Wires;

            foreach (var outline in outlineWires)
            {
                pcb.Width = Math.Max(outline.X1, pcb.Width);
                pcb.Width = Math.Max(outline.X2, pcb.Width);
                pcb.Height = Math.Max(outline.Y1, pcb.Height);
                pcb.Height = Math.Max(outline.Y2, pcb.Height);
            }

            foreach (var via in pcb.Vias)
            {
                pcb.Layers.Where(layer => layer.Layer == PCBLayers.Vias).First().Vias.Add(via);
            }

            return pcb;
        }
    }
}
