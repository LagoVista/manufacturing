using LagoVista.Core;
using LagoVista.Core.Interfaces;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartPad.PartPadStack;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class PcbPackage : IIDEntity, INamedEntity, IKeyedEntity, IDescriptionEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToId();

        public string Key { get; set; }

        public string LibraryName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<PcbLine> Wires { get; set; } = new List<PcbLine>();
        public List<SMDPad> SmdPads { get; set; } = new List<SMDPad>();
        public List<Text> Texts { get; set; } = new List<Text>();
        public List<Pad> Pads { get; set; } = new List<Pad>();
        public List<Circle> Circles { get; set; } = new List<Circle>();
        public List<Hole> Holes { get; set; } = new List<Hole>();
        public List<Rect> Rects { get; set; } = new List<Rect>();

        public List<Polygon> Polygons { get; set; } = new List<Polygon>();

        public double Width { get; set; }
        public double Height { get; set; }

        public bool IsSMD { get { return SmdPads.Any(); } }

        private static void SetComponentSize(PcbPackage pck)
        {

            var minX = 99999.0;
            var maxX = -1.0;
            var minY = 99999.0;
            var maxY = -1.0;

            if (pck.Pads.Any()) minY = Math.Min(minY, pck.Pads.Min(pd => pd.Y - pd.H));
            if (pck.Rects.Any()) minY = Math.Min(minY, pck.Rects.Min(pd => pd.Y1));
            if (pck.Rects.Any()) minY = Math.Min(minY, pck.Rects.Min(pd => pd.Y2));
            if (pck.SmdPads.Any()) minY = Math.Min(minY, pck.SmdPads.Min(pd => pd.Y1));
            if (pck.SmdPads.Any()) minY = Math.Min(minY, pck.SmdPads.Min(pd => pd.Y2));
            if (pck.Wires.Any()) minY = Math.Min(minY, pck.Wires.Min(pd => pd.Y1));
            if (pck.Wires.Any()) minY = Math.Min(minY, pck.Wires.Min(pd => pd.Y2));
            if (pck.Polygons.Any()) minY = Math.Min(minY, pck.Polygons.Min(pd => pd.Lines.Min(p => p.Y1)));
            if (pck.Polygons.Any()) minY = Math.Min(minY, pck.Polygons.Min(pd => pd.Lines.Min(p => p.Y2)));
            if (pck.Circles.Any()) minY = Math.Min(minY, pck.Circles.Min(pd => pd.Y - pd.R));

            if (pck.Pads.Any()) maxY = Math.Max(maxY, pck.Pads.Max(pd => pd.Y + pd.H));
            if (pck.Rects.Any()) maxY = Math.Max(maxY, pck.Rects.Max(pd => pd.Y1));
            if (pck.Rects.Any()) maxY = Math.Max(maxY, pck.Rects.Max(pd => pd.Y2));
            if (pck.SmdPads.Any()) maxY = Math.Max(maxY, pck.SmdPads.Max(pd => pd.Y1));
            if (pck.SmdPads.Any()) maxY = Math.Max(maxY, pck.SmdPads.Max(pd => pd.Y2));
            if (pck.Wires.Any()) maxY = Math.Max(maxY, pck.Wires.Max(pd => pd.Y1));
            if (pck.Wires.Any()) maxY = Math.Max(maxY, pck.Wires.Max(pd => pd.Y2));
            if (pck.Polygons.Any()) maxY = Math.Max(maxY, pck.Polygons.Max(pd => pd.Lines.Max(p => p.Y1)));
            if (pck.Polygons.Any()) maxY = Math.Max(maxY, pck.Polygons.Max(pd => pd.Lines.Max(p => p.Y2)));
            if (pck.Circles.Any()) maxY = Math.Max(maxY, pck.Circles.Max(pd => pd.Y + pd.R));

            if (pck.Pads.Any()) minX = Math.Min(minX, pck.Pads.Min(pd => pd.X - pd.W));
            if (pck.Rects.Any()) minX = Math.Min(minX, pck.Rects.Min(pd => pd.X1));
            if (pck.Rects.Any()) minX = Math.Min(minX, pck.Rects.Min(pd => pd.X2));
            if (pck.SmdPads.Any()) minX = Math.Min(minX, pck.SmdPads.Min(pd => pd.X1));
            if (pck.SmdPads.Any()) minX = Math.Min(minX, pck.SmdPads.Min(pd => pd.X2));
            if (pck.Wires.Any()) minX = Math.Min(minX, pck.Wires.Min(pd => pd.X1));
            if (pck.Wires.Any()) minX = Math.Min(minX, pck.Wires.Min(pd => pd.X2));
            if (pck.Polygons.Any()) minX = Math.Min(minX, pck.Polygons.Min(pd => pd.Lines.Min(p => p.X1)));
            if (pck.Polygons.Any()) minX = Math.Min(minX, pck.Polygons.Min(pd => pd.Lines.Min(p => p.X2)));
            if (pck.Circles.Any()) minX = Math.Min(minX, pck.Circles.Min(pd => pd.X - pd.R));

            if (pck.Pads.Any()) maxX = Math.Max(maxX, pck.Pads.Max(pd => pd.X + pd.W));
            if (pck.Rects.Any()) maxX = Math.Max(maxX, pck.Rects.Max(pd => pd.X1));
            if (pck.Rects.Any()) maxX = Math.Max(maxX, pck.Rects.Max(pd => pd.X2));
            if (pck.SmdPads.Any()) maxX = Math.Max(maxX, pck.SmdPads.Max(pd => pd.X1));
            if (pck.SmdPads.Any()) maxX = Math.Max(maxX, pck.SmdPads.Max(pd => pd.X2));
            if (pck.Wires.Any()) maxX = Math.Max(maxX, pck.Wires.Max(pd => pd.X1));
            if (pck.Wires.Any()) maxX = Math.Max(maxX, pck.Wires.Max(pd => pd.X2));
            if (pck.Polygons.Any()) maxX = Math.Max(maxX, pck.Polygons.Max(pd => pd.Lines.Max(p => p.X1)));
            if (pck.Polygons.Any()) maxX = Math.Max(maxX, pck.Polygons.Max(pd => pd.Lines.Max(p => p.X2)));
            if (pck.Circles.Any()) maxX = Math.Max(maxX, pck.Circles.Max(pd => pd.X + pd.R));

            pck.Width = maxX - minX;
            pck.Height = maxY - minY;
        }

        public static PcbPackage Create(XElement element)
        {
            var Name = element.GetString("name");

            var pck = new PcbPackage()
            {
                Id = Guid.NewGuid().ToId(),
                LibraryName = element.Ancestors(XName.Get("library")).First().Attribute("name").Value,
                Name = element.GetString("name"),
                Description = element.GetChildString("description"),
                Wires = (from childWires in element.Descendants("wire") select PcbLine.Create(childWires)).ToList(),
                Texts = (from childTexts in element.Descendants("text") select Text.Create(childTexts)).ToList(),
                SmdPads = (from childSMDs in element.Descendants("smd") select SMDPad.Create(childSMDs)).ToList(),
                Holes = (from childPads in element.Descendants("hole") select Hole.Create(childPads)).ToList(),
                Circles = (from childCircles in element.Descendants("circle") select Circle.Create(childCircles)).ToList(),
                Rects = (from childCircles in element.Descendants("rect") select Rect.Create(childCircles)).ToList(),
            };

            pck.Key = (pck.LibraryName + pck.Name).ToNuvIoTKey();

            foreach (var padSet in (from childPads in element.Descendants("pad") select Pad.Create(childPads)).ToList())
                pck.Pads.AddRange(padSet);

            SetComponentSize(pck);

            return pck;
        }

        public static PcbPackage Create(Footprint fp)
        {
            var pck = new PcbPackage()
            {
                Id = Guid.NewGuid().ToId(),
                Key = fp.Name.ToNuvIoTKey(),
                Name = fp.Name,
                Circles = fp.FpCircles.Select(cir => Circle.Create(cir)).ToList(),
                Texts =  fp.FpTexts.Select(txt => Text.Create(txt)).ToList(),
                Rects = fp.FpRects.Select(rect => Rect.Create(rect)).ToList(),
                Wires = fp.FpLines.Select(line => PcbLine.Create(line)).ToList(),
                Polygons = fp.FpPolys.Select(ply => Polygon.Create(ply)).ToList(),
            };

            foreach(var padSet in fp.Pads.Where(p => fp.Attr != "smd" && p.PadType != "smd").Select(pad => Pad.Create(pad, fp.PositionAt.Angle)).ToList())
                pck.Pads.AddRange(padSet);

            foreach (var padSet in fp.Pads.Where(p => fp.Attr == "smd" || p.PadType == "smd") .Select(pad => SMDPad.Create(pad, fp.PositionAt.Angle)).ToList())
                pck.SmdPads.AddRange(padSet);

            SetComponentSize(pck);

            return pck;
        }

        public PcbPackage Clone()
        {
            return new PcbPackage()
            {
                LibraryName = LibraryName,
                Name = Name,
                Key = Key,
                Description = Description,
                Wires = Wires,
                SmdPads = SmdPads,
                Texts = Texts,
                Pads = Pads,
                Circles = Circles,
                Holes = Holes,
                Rects = Rects,
            };
        }
    }
}
