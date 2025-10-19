// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a16dfffafcb72a76d044236bce44f564ceb34a89513562e159546beb5356c567
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.PCB.Eagle.Extensions;
using LagoVista.PCB.Eagle.Models;
using MSDMarkwort.Kicad.Parser.PcbNew;
using System;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace LagoVista.PCB.Eagle.Managers
{
    public class KicadImport
    {
        public static Models.PrintedCircuitBoard ImportPCB(Stream stream)
        {
            var parser = new PcbNewParser();
            var results = parser.Parse(stream);
            var pcb = new Models.PrintedCircuitBoard();

            if (results.Success)
            {
                foreach (var fp in results.Result.Footprints)
                {
                    var cmp = PcbComponent.Create(fp);                    

                    if (!pcb.Packages.Any(pc => pc.Key == cmp.Package.Key))
                        pcb.Packages.Add(cmp.Package.Value);

                    pcb.Components.Add(cmp);
                }

                foreach(var ly in results.Result.Layers.Layers)
                {
                    pcb.Layers.Add(PcbLayer.Create(ly));
                }

                pcb.Vias = results.Result.Vias.Select(v => new Via() { X = v.Position.X, Y = v.Position.Y, DrillDiameter = v.Drill }).ToList();
                foreach(var via in results.Result.Vias)
                {
                    foreach(var layerName in via.Layers)
                    {
                        var layer = pcb.Layers.Single(lay => lay.Layer == layerName.FromKiCadLayer());
                        layer.Vias.Add(new Via() { X = via.Position.X, Y = via.Position.Y, DrillDiameter = via.Drill });
                    }
                }

                var top = results.Result.Segments.Where(seg => seg.Layer.FromKiCadLayer() == PCBLayers.TopCopper);
                var bottom = results.Result.Segments.Where(seg => seg.Layer.FromKiCadLayer() == PCBLayers.BottomCopper);
                pcb.TopWires = top.Select(seg => new PcbLine() { W = seg.Width, X1 = seg.StartPosition.X, Y1 = seg.StartPosition.Y, X2 = seg.EndPosition.X, Y2 = seg.EndPosition.Y }).ToList();
                pcb.BottomWires = bottom.Select(seg => new PcbLine() {W = seg.Width,  X1 = seg.StartPosition.X, Y1 = seg.StartPosition.Y, X2 = seg.EndPosition.X, Y2 = seg.EndPosition.Y }).ToList();
                foreach (var layer in pcb.Layers)
                {
                    if (layer.Layer == PCBLayers.BoardOutline)
                    {
                        layer.Drills = results.Result.Vias.Select(v => new Drill() { X = v.Position.X, Y = v.Position.Y, D = v.Drill }).ToList();
                        layer.Holes = results.Result.GrCircle.Select(cir => new Hole() { X = cir.CenterPosition.X, Y = cir.CenterPosition.Y, D = cir.Width, Layer = cir.Layer.FromKiCadLayer() }).ToList();
                    }
                    else
                    {
                        layer.Circles = results.Result.GrCircle.Select(cir => new Circle() { X = cir.CenterPosition.X, Y = cir.CenterPosition.Y, W = cir.Width, L = cir.Layer.FromKiCadLayer() }).ToList();
                    }
                }

                foreach (var seg in results.Result.GrCircle)
                {
                    Console.WriteLine(seg.CenterPosition.X + " - " + seg.CenterPosition.Y + " - " + seg.Layer);
                }

                var edges = results.Result.GrLines.Where(ln => ln.Layer == "Edge.Cuts");
                var args = results.Result.Arc.Where(ln => ln.Layer == "Edge.Cuts");
                var minX = edges.Min(edg => edg.StartPosition.X < edg.EndPosition.X ? edg.StartPosition.X : edg.EndPosition.X);
                var maxX = edges.Max(edg => edg.StartPosition.X > edg.EndPosition.X ? edg.StartPosition.X : edg.EndPosition.X);
                var minY = edges.Min(edg => edg.StartPosition.Y < edg.EndPosition.Y ? edg.StartPosition.Y : edg.EndPosition.Y);
                var maxY = edges.Max(edg => edg.StartPosition.Y > edg.EndPosition.Y ? edg.StartPosition.Y : edg.EndPosition.Y);
                pcb.Width = maxX - minX;
                pcb.Height = maxY - minY;

                pcb.Outline = edges.Select(ln=> new PcbLine() {Crv = ln.Angle, X1 = ln.StartPosition.X - minX, Y1 = ln.StartPosition.Y - minY, X2 = ln.EndPosition.X - minX, Y2 = ln.EndPosition.Y - minY}).ToList();
                pcb.Outline.AddRange(args.Select(arc => new PcbLine() { Crv = arc.Angle, X1 = arc.StartPosition.X - minX, Y1 = arc.StartPosition.Y - minY, X2 = arc.EndPosition.X - minX, Y2 = arc.EndPosition.Y - minY }));

                foreach (var cmp in pcb.Components)
                {
                    cmp.X -= minX;
                    cmp.Y = maxY - cmp.Y;
                }
            }

            return pcb;
        }

    }
}
