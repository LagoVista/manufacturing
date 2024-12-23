using LagoVista.Core;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public enum PCBLayers
    {
        TopCopper,
        BottomCopper,
        TopSilk,
        BottomSilk,
        Drills,
        Pads,
        Holes,
        BoardOutline,
        TopStayOut,
        BottomStayOut,
        TopNames,
        BottomNames,
        TopDocument,
        BottomDocument,
        TopSolderMask,
        BottomSolderMask,
        TopStencil,
        BottomStencil,
        TopValues,
        BottomValues,
        Unrouted,
        Via,
        Other
    }


    public class PcbComponent : IIDEntity, INamedEntity, IKeyedEntity
    {
        public string Id { get; set; }
        public string Key { get; set; }

        public string Name { get; set; }
        public string LibraryName { get; set; }
        public string PackageName { get; set; }
        public string Value { get; set; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public double Rotation { get; set; }
        public bool Polarized { get; set; }
        public bool Included { get; set; }
        public bool ManualPlace { get; set; }
        public bool Fiducial { get; set; }
        public string Function { get; set; }
        public string Notes { get; set; }
      
        public EntityHeader Component { get; set; }

        public EntityHeader<PCBLayers> Layer { get; set; }


        public EntityHeader<PcbPackage> Package { get; set; }

        public List<Pad> Pads
        {
            get
            {
                if (Package?.Value == null)
                    return null;

                var pads = new List<Pad>();
                foreach (var pad in Package.Value.Pads)
                {
                    var rotatedPad = pad.ApplyRotation(Rotation);
                    rotatedPad.X += X.Value;
                    rotatedPad.Y += Y.Value;                    
                    pads.Add(rotatedPad);
                }
           
                return pads;
            }
        }

        public List<Hole> Holes
        {
            get
            {
                if (Package?.Value == null)
                    return null;

                var holes = new List<Hole>();
                foreach(var hole in Package.Value.Holes)
                {
                    var rotatedHole = hole.ApplyRotation(Rotation);
                    rotatedHole.X += X.Value;
                    rotatedHole.Y += Y.Value;
                    holes.Add(rotatedHole);
                }


                return holes;
            }
        }

        public List<SMDPad> SMDPads
        {
            get
            {
                if (Package?.Value == null)
                    return null;

                var smdPads = new List<SMDPad>();

                foreach (var smd in Package.Value.SmdPads)
                {
                    
                    var rotatedSMD = smd.ApplyRotation(Rotation);


                    rotatedSMD.X1 += X.Value;
                    rotatedSMD.Y1 += Y.Value;
                    rotatedSMD.X2 += X.Value;
                    rotatedSMD.Y2 += Y.Value;
                    smdPads.Add(smd);
                }

                return smdPads;
            }
        }

        public static PcbComponent Create(XElement element)
        {
            return new PcbComponent()
            {
                Name = element.GetString("name"),
                LibraryName = element.GetString("library"),
                PackageName = element.GetString("package"),
                Rotation = element.GetString("rot").ToAngle(),
                Value = element.GetString("value"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y")
            };
        }

        public static PcbComponent Create(Footprint fp)
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
                Rotation = fp.PositionAt.Angle,
            };

            var pck = PcbPackage.Create(fp);
            cmp.Package = EntityHeader<PcbPackage>.Create(pck);


            return cmp;
        }

        public PcbComponent Clone()
        {
            return new PcbComponent()
            {
                Id = Guid.NewGuid().ToId(),
                Key = Key,
                Name = Name,
                LibraryName = LibraryName,
                PackageName = PackageName,
                Rotation = Rotation,
                Value = Value,
                X = X,
                Y = Y,
                Package = Package.Clone(),
                Included = Included,
                Function = Function,
                Notes = Notes,
            };
        }
    }
}
