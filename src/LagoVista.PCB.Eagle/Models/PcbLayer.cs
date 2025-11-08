// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c684902e87baed7cd178b35f8350bf43f0b800132a8fa360da931ab2b8de3dba
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartLayers;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public class PcbLayer : IIDEntity, IKeyedEntity, INamedEntity
    {
        public PcbLayer()
        {
            Drills = new List<Drill>();
            Circles = new List<Circle>();
            Holes = new List<Hole>();
            Rects = new List<Rect>();
            Wires = new List<PcbLine>();
            SMDs = new List<SMDPad>();
            Pads = new List<Pad>();
            Vias = new List<Via>();
        }

        public string Id { get; set; }

        //        public int Number { get; set; }
        public string Name { get; set; }
        public string Key { get; set; }

        public PCBLayers Layer { get; set; }
        public int Color { get; set; }
        public int Fill { get; set; }

        public static PcbLayer Create(XElement element)
        {
            var layer = new PcbLayer()
            {
                Id = Guid.NewGuid().ToId(),
                Layer = element.GetInt32("number").FromEagleLayer(),
                Name = element.GetString("name"),
                Color = element.GetInt32("color"),
                Fill = element.GetInt32("fill"),
                Key = element.GetString("name").ToNuvIoTKey(),
            };

            return layer;
        }

        public static PcbLayer Create(BoardLayer layer)
        {
            return new PcbLayer()
            {
                Id = Guid.NewGuid().ToId(),
                Name = layer.Name,
                Key = layer.ShortName.ToNuvIoTKey(),
                Layer = String.IsNullOrEmpty(layer.Name) ? layer.ShortName.FromKiCadLayer() : layer.Name.FromKiCadLayer(),
            };
        }

        public List<Drill> Drills { get; set; }

        public List<Circle> Circles { get; set; }
        public List<Hole> Holes { get; set; }
        public List<Rect> Rects { get; set; }

        public List<PcbLine> Wires { get; set; }
        public List<Via> Vias { get; set; }
        public List<SMDPad> SMDs { get; set; }
        public List<Pad> Pads { get; set; }

    }


}
