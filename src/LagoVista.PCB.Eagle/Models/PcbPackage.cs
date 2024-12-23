using LagoVista.Core;
using LagoVista.Core.Interfaces;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public bool IsSMD { get { return SmdPads.Any(); } }

        public static PcbPackage Create(XElement element)
        {
            var pck = new PcbPackage()
            {
                Id = Guid.NewGuid().ToId(),
                LibraryName = element.Ancestors(XName.Get("library")).First().Attribute("name").Value,
                Name = element.GetString("name"),
                Key = element.GetString("name").ToNuvIoTKey(),
                Description = element.GetChildString("description"),
                Wires = (from childWires in element.Descendants("wire") select PcbLine.Create(childWires)).ToList(),
                Texts = (from childTexts in element.Descendants("text") select Text.Create(childTexts)).ToList(),
                SmdPads = (from childSMDs in element.Descendants("smd") select SMDPad.Create(childSMDs)).ToList(),
                Pads = (from childPads in element.Descendants("pad") select Pad.Create(childPads)).ToList(),
                Holes = (from childPads in element.Descendants("hole") select Hole.Create(childPads)).ToList(),
                Circles = (from childCircles in element.Descendants("circle") select Circle.Create(childCircles)).ToList(),
                Rects = (from childCircles in element.Descendants("rect") select Rect.Create(childCircles)).ToList(),
            };
            
            return pck;
        }

        public static PcbPackage Create(Footprint fp)
        {
            var pck = new PcbPackage()
            {
                Id = Guid.NewGuid().ToId(),
                Key = fp.Name.ToNuvIoTKey(),
                Name = fp.Name,
                Pads = fp.Pads.Where(p => fp.Attr != "smd").Select(pad => Pad.Create(pad, fp.PositionAt.Angle)).ToList(),
                SmdPads = fp.Pads.Where(p => fp.Attr == "smd").Select(pad => SMDPad.Create(pad, fp.PositionAt.Angle)).ToList(),
                Circles = fp.FpCircles.Select(cir => Circle.Create(cir)).ToList(),
                Texts =  fp.FpTexts.Select(txt => Text.Create(txt)).ToList(),
                Rects = fp.FpRects.Select(rect => Rect.Create(rect)).ToList(),
                Wires = fp.FpLines.Select(line => PcbLine.Create(line)).ToList()
            };
            
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
