﻿using LagoVista.Core.Models;
using System;
using System.Collections.Generic;
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
    }


    public class PcbComponent
    {
        public string Name { get; set; }
        public string LibraryName { get; set; }
        public string PackageName { get; set; }
        public string Value { get; set; }
        public double? X { get; set; }
        public double? Y { get; set; }
        public string Rotate { get; set; }
        public bool Polarized { get; set; }
        public bool Included { get; set; }
        public bool ManualPlace { get; set; }
        public bool Fiducial { get; set; }
        public string Function { get; set; }
        public string Notes { get; set; }
      
        public EntityHeader Component { get; set; }

        public int Layer
        {
            get
            {
                return (Rotate != null && Rotate.StartsWith("M")) ? 16 : 1;
            }
        }

        public double RotateAngle
        {
            get
            {
                return Rotate.ToAngle();
            }
        }

        public PhysicalPackage Package { get; set; }

        public List<Pad> Pads
        {
            get
            {
                var pads = new List<Pad>();
                foreach (var pad in Package.Pads)
                {
                    var rotatedPad = pad.ApplyRotation(Rotate.ToAngle());
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
                var holes = new List<Hole>();
                foreach(var hole in Package.Holes)
                {
                    var rotatedHole = hole.ApplyRotation(Rotate.ToAngle());
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
                var smdPads = new List<SMDPad>();

                foreach (var smd in Package.SmdPads)
                {
                    
                    var rotatedSMD = smd.ApplyRotation(Rotate.ToAngle());

                    if(!String.IsNullOrEmpty(Rotate))
                    {
                    }

                    rotatedSMD.X1 += X.Value;
                    rotatedSMD.Y1 += Y.Value;
                    rotatedSMD.X2 += X.Value;
                    rotatedSMD.Y2 += Y.Value;
                    smdPads.Add(smd);
                }

                return smdPads;
            }
        }

        public string Key => PackageName.ToUpper() + "-" + Value.ToUpper();

        public static PcbComponent Create(XElement element)
        {
            return new PcbComponent()
            {
                Name = element.GetString("name"),
                LibraryName = element.GetString("library"),
                PackageName = element.GetString("package"),
                Rotate = element.GetString("rot"),
                Value = element.GetString("value"),
                X = element.GetDouble("x"),
                Y = element.GetDouble("y")
            };
        }

        public PcbComponent Clone()
        {
            return new PcbComponent()
            {
                Name = Name,
                LibraryName = LibraryName,
                PackageName = PackageName,
                Rotate = Rotate,
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
