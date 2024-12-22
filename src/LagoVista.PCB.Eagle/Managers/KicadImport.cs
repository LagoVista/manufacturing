using LagoVista.PCB.Eagle.Models;
using MSDMarkwort.Kicad.Parser.PcbNew;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace LagoVista.PCB.Eagle.Managers
{
    public class KicadImport
    {
        public static Models.PrintedCircuitBoard ReadPCB(Stream stream)
        {
            var parser = new PcbNewParser();
            var results = parser.Parse(stream);
            var pcb = new Models.PrintedCircuitBoard();

            if (results.Success)
            {
                foreach (var fp in results.Result.Footprints)
                {
                    var reference = fp.Properties.FirstOrDefault(f => f.Name == "Reference")?.Value;
                    var value = fp.Properties.FirstOrDefault(f => f.Name == "Value")?.Value;
                    var footPrint = fp.Properties.FirstOrDefault(f => f.Name == "Footprint")?.Value;

                    var cmp = new PcbComponent()
                    {
                        Name = reference,
                        Value = value,
                        PackageName = footPrint,
                        X = fp.PositionAt.X,
                        Y = fp.PositionAt.Y,
                        Rotate = fp.PositionAt.Angle.ToString(),
                    };

                    cmp.Package = new PhysicalPackage()
                    {
                        Name = footPrint,
                        Pads = fp.Pads.Where(p=> fp.Attr != "smd").Select(pad => new Pad()
                        {
                            DrillDiameter = pad.Drill.OuterDiameter,
                            OriginX = pad.PositionAt.X,
                            OriginY = pad.PositionAt.Y,
                            X = pad.PositionAt.X,
                            Y = pad.PositionAt.Y,
                            RotateStr = (pad.PositionAt.Angle - fp.PositionAt.Angle).ToString()
                        }).ToList(),

                        SmdPads = fp.Pads.Where(p => fp.Attr == "smd").Select(pad => new SMDPad()
                        {
                            OriginX = pad.PositionAt.X,
                            OriginY = pad.PositionAt.Y,
                            DX = pad.Size.Width,
                            DY = pad.Size.Height,
                            RotateStr = (pad.PositionAt.Angle - fp.PositionAt.Angle).ToString()
                        }).ToList(),

                        Rects = fp.FpRects.Select(rect => new Rect()
                        {
                            X1 = rect.StartPosition.X,
                            Y1 = rect.StartPosition.Y,
                            X2 = rect.EndPosition.X,
                            Y2 = rect.EndPosition.Y,
                        }).ToList(),

                        Wires = fp.FpLines.Select(line => new Wire()
                        {
                            Rect = new Rect()
                            {
                                X1 = line.StartPosition.X,
                                Y1 = line.StartPosition.Y,
                                X2 = line.EndPosition.X,
                                Y2 = line.EndPosition.Y,
                            }
                        }).ToList()
                    };

                    if (!pcb.Packages.Any(pc => pc.Name == cmp.PackageName))
                    {
                        pcb.Packages.Add(cmp.Package);
                    }

                    pcb.Components.Add(cmp);
                }

                var edges = results.Result.GrLines.Where(ln => ln.Layer == "Edge.Cuts");
                var minX = edges.Min(edg => edg.StartPosition.X < edg.EndPosition.X ? edg.StartPosition.X : edg.EndPosition.X);
                var maxX = edges.Max(edg => edg.StartPosition.X > edg.EndPosition.X ? edg.StartPosition.X : edg.EndPosition.X);
                var minY = edges.Min(edg => edg.StartPosition.Y < edg.EndPosition.Y ? edg.StartPosition.Y : edg.EndPosition.Y);
                var maxY = edges.Max(edg => edg.StartPosition.Y > edg.EndPosition.Y ? edg.StartPosition.Y : edg.EndPosition.Y);

                pcb.Outline = edges.Select(ln=> new Wire() {Curve = ln.Angle, Rect = new Rect() { X1 = ln.StartPosition.X, Y1 = ln.StartPosition.Y, X2 = ln.EndPosition.X, Y2 = ln.EndPosition.Y } }).ToList();

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
