// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: fbe2dbee123e49064239d0e6cbd086b8c70ab5c1d312ef273c24739318252f43
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models;
using LagoVista.PCB.Eagle.Extensions;
using MSDMarkwort.Kicad.Parser.PcbNew.Models.PartFootprint.PartFp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LagoVista.PCB.Eagle.Models
{
    public class Polygon
    {
        public List<PolyLine> Lines { get; set; } = new List<PolyLine>();
        public PCBLayers L { get; set; }
        public string S { get; set; }

        public static Polygon Create(FpPoly poly)
        {
            var p = new Polygon()
            {
                L = poly.Layer.FromKiCadLayer(),
                S = poly.Stroke.Color.ToString(poly.Layer),
            };

            for (var idx = 0; idx < poly.Pts.Positions.Count; ++idx)
            {
                if (idx == poly.Pts.Positions.Count - 1)
                {
                    var line = new PolyLine()
                    {
                        X1 = poly.Pts.Positions[idx].X,
                        Y1 = poly.Pts.Positions[idx].Y,
                        X2 = poly.Pts.Positions[0].X,
                        Y2 = poly.Pts.Positions[0].Y,
                      
                    };
                    p.Lines.Add(line);
                }
                else
                {
                    var line = new PolyLine()
                    {
                        X1 = poly.Pts.Positions[idx].X,
                        Y1 = poly.Pts.Positions[idx].Y,
                        X2 = poly.Pts.Positions[idx + 1].X,
                        Y2 = poly.Pts.Positions[idx + 1].Y,
                        
                    };
                    p.Lines.Add(line);
                }
            }

            if (p.Lines.Count > 4)
                p.Lines = p.Lines.Take(4).ToList();

            return p;
        }
    }

    public class PolyLine
    {
        public double X1 { get; set; }
        public double X2 { get; set; }
        public double Y1 { get; set; }
        public double Y2 { get; set; }
    }

}
