// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0343bc9e6552514d1cefe3974358f4371fb5d74f14fc57611af82ea49f6f4356
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core;
using LagoVista.Core.Attributes;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using LagoVista.PCB.Eagle.Resources;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace LagoVista.PCB.Eagle.Models
{
    public enum PCBLayers
    {
        [EnumLabel(PcbComponent.Layer_TopCopper, PcbResources.Names.Layer_TopCopper, typeof(PcbResources))]
        TopCopper,
        [EnumLabel(PcbComponent.Layer_BottomCopper, PcbResources.Names.Layer_BottomCopper, typeof(PcbResources))]
        BottomCopper,
        [EnumLabel(PcbComponent.Layer_TopSilk, PcbResources.Names.Layer_TopSilk, typeof(PcbResources))]
        TopSilk,
        [EnumLabel(PcbComponent.Layer_BottomSilk, PcbResources.Names.Layer_BottomSilk, typeof(PcbResources))]
        BottomSilk,
        [EnumLabel(PcbComponent.Layer_Drills, PcbResources.Names.Layer_Drills, typeof(PcbResources))]
        Drills,
        [EnumLabel(PcbComponent.Layer_Pads, PcbResources.Names.Layer_Pads, typeof(PcbResources))]
        Pads,
        [EnumLabel(PcbComponent.Layer_Holes, PcbResources.Names.Layer_Holes, typeof(PcbResources))]
        Holes,
        [EnumLabel(PcbComponent.Layer_BoardOutline, PcbResources.Names.Layer_BoardOutline, typeof(PcbResources))]
        BoardOutline,
        [EnumLabel(PcbComponent.Layer_TopRestrict, PcbResources.Names.Layer_TopRestrict, typeof(PcbResources))]
        TopRestrict,
        [EnumLabel(PcbComponent.Layer_BottomRestrict, PcbResources.Names.Layer_BottomRestrict, typeof(PcbResources))]
        BottomRestrict,
        [EnumLabel(PcbComponent.Layer_TopNames, PcbResources.Names.Layer_TopNames, typeof(PcbResources))]
        TopNames,
        [EnumLabel(PcbComponent.Layer_BottomNames, PcbResources.Names.Layer_BottomNames, typeof(PcbResources))]
        BottomNames,
        [EnumLabel(PcbComponent.Layer_TopDocument, PcbResources.Names.Layer_TopDocument, typeof(PcbResources))]
        TopDocument,
        [EnumLabel(PcbComponent.Layer_BottomDocument, PcbResources.Names.Layer_BottomDocument, typeof(PcbResources))]
        BottomDocument,
        [EnumLabel(PcbComponent.Layer_TopSolderMask, PcbResources.Names.Layer_TopSolderMask, typeof(PcbResources))]
        TopSolderMask,
        [EnumLabel(PcbComponent.Layer_BottomSolderMask, PcbResources.Names.Layer_BottomSolderMask, typeof(PcbResources))]
        BottomSolderMask,
        [EnumLabel(PcbComponent.Layer_TopStencil, PcbResources.Names.Layer_TopStencil, typeof(PcbResources))]
        TopStencil,
        [EnumLabel(PcbComponent.Layer_BottomStencil, PcbResources.Names.Layer_BottomStencil, typeof(PcbResources))]
        BottomStencil,
        [EnumLabel(PcbComponent.Layer_TopValues, PcbResources.Names.Layer_TopValues, typeof(PcbResources))]
        TopValues,
        [EnumLabel(PcbComponent.Layer_BottomValues, PcbResources.Names.Layer_BottomValues, typeof(PcbResources))]
        BottomValues,
        [EnumLabel(PcbComponent.Layer_Unrouted, PcbResources.Names.Layer_Unrouted, typeof(PcbResources))]
        Unrouted,
        [EnumLabel(PcbComponent.Layer_Vias, PcbResources.Names.Layer_Vias, typeof(PcbResources))]
        Vias,
        [EnumLabel(PcbComponent.Layer_Measures, PcbResources.Names.Layer_Measures, typeof(PcbResources))]
        Measures,
        [EnumLabel(PcbComponent.Layer_Other, PcbResources.Names.Layer_Other, typeof(PcbResources))]
        Other
    }


    public class PcbComponent : IIDEntity, INamedEntity, IKeyedEntity
    {
        public const string Layer_TopCopper = "top.copper";
        public const string Layer_BottomCopper = "bottom.copper";
        public const string Layer_TopSilk = "top.silk";
        public const string Layer_BottomSilk = "bottom.silk";
        public const string Layer_Drills = "drills";
        public const string Layer_Pads = "pads";
        public const string Layer_Holes = "holes";
        public const string Layer_BoardOutline = "boardoutline";
        public const string Layer_TopRestrict = "top.restrict";
        public const string Layer_BottomRestrict = "bottom.restrict";
        public const string Layer_TopNames = "top.names";
        public const string Layer_BottomNames = "bottom.names";
        public const string Layer_TopDocument = "top.document";
        public const string Layer_BottomDocument = "bottom.document";
        public const string Layer_TopSolderMask = "top.soldermask";
        public const string Layer_BottomSolderMask = "bottom.soldermask";
        public const string Layer_TopStencil = "top.stencil";
        public const string Layer_BottomStencil = "bottom.stencil";
        public const string Layer_TopValues = "top.values";
        public const string Layer_BottomValues = "bottom.values";
        public const string Layer_Unrouted = "unrouted";
        public const string Layer_Vias = "vias";
        public const string Layer_Measures = "measures";
        public const string Layer_Dimensions = "dimensions";
        public const string Layer_Other = "other";

        public string Id { get; set; }
        public string Key { get; set; }

        [JsonIgnore]
        public string PackageAndValue
        {
            get
            {
                return $"{PackageName}{Value}";
            }
        }

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
        public bool Ignore { get; set; }
        public string Function { get; set; }
        public string Notes { get; set; }      
        public EntityHeader Component { get; set; }
        public PCBLayers Layer { get; set; }
        
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
            var attr = element.Attributes();

            return new PcbComponent()
            {
                Id = Guid.NewGuid().ToId(),
                Key = element.GetString("name").ToNuvIoTKey() + "cmp",
                Name = element.GetString("name"),
                Layer = element.GetString("rot").StartsWith("M") ? PCBLayers.BottomCopper : PCBLayers.TopCopper,
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
                Id = Guid.NewGuid().ToId(),
                Key = reference + "cmp",
                Name = reference,
                Value = value,
                PackageName = footPrint,
                X = fp.PositionAt.X,
                Y = fp.PositionAt.Y,
                Rotation = fp.PositionAt.Angle,
                Layer = fp.Layer.FromKiCadLayer()
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
