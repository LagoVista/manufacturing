using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class PhysicalPackage
    {
        public string LibraryName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public List<Wire> Wires { get; set; } = new List<Wire>();
        public List<SMDPad> SmdPads { get; set; } = new List<SMDPad>();
        public List<Text> Texts { get; set; } = new List<Text>();
        public List<Pad> Pads { get; set; } = new List<Pad>();
        public List<Circle> Circles { get; set; } = new List<Circle>();
        public List<Hole> Holes { get; set; } = new List<Hole>();
        public List<Rect> Rects { get; set; } = new List<Rect>();

        public bool IsSMD { get { return SmdPads.Any(); } }

        public static PhysicalPackage Create(XElement element)
        {
            return new PhysicalPackage()
            {
                LibraryName = element.Ancestors(XName.Get("library")).First().Attribute("name").Value,
                Name = element.GetString("name"),
                Description = element.GetChildString("description"),
                Wires = (from childWires in element.Descendants("wire") select Wire.Create(childWires)).ToList(),
                Texts = (from childTexts in element.Descendants("text") select Text.Create(childTexts)).ToList(),
                SmdPads = (from childSMDs in element.Descendants("smd") select SMDPad.Create(childSMDs)).ToList(),
                Pads = (from childPads in element.Descendants("pad") select Pad.Create(childPads)).ToList(),
                Holes = (from childPads in element.Descendants("hole") select Hole.Create(childPads)).ToList(),
                Circles = (from childCircles in element.Descendants("circle") select Circle.Create(childCircles)).ToList(),
                Rects = (from childCircles in element.Descendants("rect") select Rect.Create(childCircles)).ToList(),
            };
        }

        public PhysicalPackage Clone()
        {
            return new PhysicalPackage()
            {
                LibraryName = LibraryName,
                Name = Name,
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
