using LagoVista.PCB.Eagle.Models;
using MSDMarkwort.Kicad.Parser.PcbNew;
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

                var edges = results.Result.GrLines.Where(ln => ln.Layer == "Edge.Cuts");
                var args = results.Result.Arc.Where(ln => ln.Layer == "Edge.Cuts");
                var minX = edges.Min(edg => edg.StartPosition.X < edg.EndPosition.X ? edg.StartPosition.X : edg.EndPosition.X);
                var maxX = edges.Max(edg => edg.StartPosition.X > edg.EndPosition.X ? edg.StartPosition.X : edg.EndPosition.X);
                var minY = edges.Min(edg => edg.StartPosition.Y < edg.EndPosition.Y ? edg.StartPosition.Y : edg.EndPosition.Y);
                var maxY = edges.Max(edg => edg.StartPosition.Y > edg.EndPosition.Y ? edg.StartPosition.Y : edg.EndPosition.Y);

                pcb.Outline = edges.Select(ln=> new PcbLine() {Curve = ln.Angle, X1 = ln.StartPosition.X, Y1 = ln.StartPosition.Y, X2 = ln.EndPosition.X, Y2 = ln.EndPosition.Y }).ToList();

                pcb.Width = maxX - minX;
                pcb.Height = maxY - minY;
                foreach(var cmp in pcb.Components)
                {
                    cmp.X -= minX;
                    cmp.Y = maxY - cmp.Y;
                }
            }

            return pcb;
        }

    }
}
