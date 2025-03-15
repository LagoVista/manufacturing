using LagoVista.Manufacturing.Models;
using PdfSharpCore.Drawing.BarCodes;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Services
{
    public class GCodeBuilder
    {
        public void CreateGCode(GCodeProject project, StringBuilder bldr)
        {

            foreach (var layer in project.Layers)
                CreateGCode(layer, project.Tools, bldr);
            
        }

        public void CreateGCode(GCodeLayer layer, IEnumerable<GCodeTool> tools, StringBuilder bldr)
        {
            foreach(var drill in layer.Drill) 
                CreateGCode(drill, tools, bldr);

            foreach (var hole in layer.Holes)
                CreateGCode(hole, tools, bldr);

            foreach (var rect in layer.Rectangles)
                CreateGCode(rect, tools, bldr);

            foreach (var poly in layer.Polygons)
                CreateGCode(poly, tools, bldr);
        }

        public void CreateGCode(GCodeDrill drill, IEnumerable<GCodeTool> tools, StringBuilder bldr)
        {
            bldr.AppendLine($"G0 Y{drill.Location.X} Y{drill.Location.Y}");
        }

        public void CreateGCode(GCodeHole hole, IEnumerable<GCodeTool> tools, StringBuilder bldr)
        {
            bldr.AppendLine($"G0 Y{hole.Location.X} Y{hole.Location.Y}");
        }

        public void CreateGCode(GCodeRectangle rectangle, IEnumerable<GCodeTool> tools, StringBuilder bldr)
        {

        }

        public void CreateGCode(GCodePolygon polygon, IEnumerable<GCodeTool> tools,  StringBuilder bldr)
        {
        }
    }
}
