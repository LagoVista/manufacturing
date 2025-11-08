// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: aaf745a07048eb345acf1a2a9d5f0d96ee8fb957066e8b6932105afa331a9e9d
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.UserAdmin.Models.Users;
using PdfSharpCore.Drawing.BarCodes;
using RingCentral;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LagoVista.Manufacturing.Services
{
    public class GCodeBuilder : IGCodeBuilder
    {
        public void CreateGCode(GCodeProject project, StringBuilder bldr)
        {

            foreach (var layer in project.Layers)
                CreateGCode(layer, project, bldr);
            
        }

        public void CreateGCode(GCodeLayer layer, GCodeProject project, StringBuilder bldr)        
        {
            MoveToSafeMoveHeight(project, bldr);

            foreach(var drill in layer.Drill) 
                CreateGCode(drill, project, bldr);

            foreach (var hole in layer.Holes)
                CreateGCode(hole, project, bldr);

            foreach (var rect in layer.Rectangles)
                CreateGCode(rect, project, bldr);

            foreach (var poly in layer.Polygons)
                CreateGCode(poly, project, bldr);

            foreach (var plane in layer.Planes)
                CreateGCode(plane, project, bldr);

            bldr.AppendLine("G1 X0 Y0");
        }

        private void MoveToSurfaceGCode(StringBuilder bldr)
        {
            bldr.AppendLine("G1 Z0");
        }

        private void MoveToSafeMoveHeight(GCodeProject project, StringBuilder bldr)
        {
            bldr.AppendLine($"G1 Z{project.SafeMoveHeight}");
        }

        public void CreateGCode(GCodeDrill drill, GCodeProject project, StringBuilder bldr)
        {
            var tool = project.Tools.Single(tool => tool.Id == drill.GcodeOperationTool.Id);
            bldr.AppendLine($"G1 X{drill.Location.X} Y{drill.Location.Y}");
            bldr.AppendLine($"G1 Z0");
            var depth = drill.EntireDepth ? project.StockDepth : drill.Depth;
            bldr.AppendLine($"G1 Z{depth} F{tool.PlungeRate}");
            MoveToSafeMoveHeight(project, bldr);
        }

        public void CreateGCode(GCodeHole hole, GCodeProject project, StringBuilder bldr)
        {
            var tool = project.Tools.Single(tool => tool.Id == hole.GcodeOperationTool.Id);

            var radius =  hole.CutType.Value == GCodeCutTypes.Interior ? (hole.Diameter / 2.0) - (tool.Diameter / 2) : (hole.Diameter / 2.0) + (tool.Diameter / 2);

            bldr.AppendLine($"G1 X{hole.Location.X + radius} Y{hole.Location.Y + radius} F{project.TravelFeedRate}");
            MoveToSurfaceGCode(bldr);
            
            var depth = hole.EntireDepth ? project.StockDepth : hole.Depth;
            var targetDepth = 0.0;

            do
            {
                targetDepth += tool.PlungeDepth;
                targetDepth = Math.Min(targetDepth, depth);
                bldr.AppendLine($"G1 X{hole.Location.X + radius} Y{hole.Location.Y + radius} F{project.TravelFeedRate}");
                bldr.AppendLine($"G1 Z{-targetDepth} F{tool.PlungeRate}");
                bldr.AppendLine($"G2 I-{radius} J-{radius}");
            }
            while (targetDepth != depth);

            MoveToSafeMoveHeight(project, bldr);
        }

        public void CreateGCode(GCodeRectangle rect, GCodeProject project, StringBuilder bldr)
        {
            var tool = project.Tools.Single(tool => tool.Id == rect.GcodeOperationTool.Id);
            var toolRadius = tool.Diameter / 2;
            if(rect.CutType.Value == GCodeCutTypes.Exterior)
                toolRadius = -toolRadius;

            var x1 = rect.Origin.X + toolRadius;
            var y1 = rect.Origin.Y + toolRadius;
            var x2 = rect.Origin.X + rect.Size.X - toolRadius;
            var y2 = rect.Origin.Y + rect.Size.Y - toolRadius;

            var depth = rect.EntireDepth ? project.StockDepth : rect.Depth;
            var targetDepth = 0.0;


            if (rect.CornerRadius > 0)
            {
                var actualRadius = rect.CornerRadius - toolRadius;
                bldr.AppendLine($"G1 X{x1} Y{y1 + actualRadius} F{project.TravelFeedRate}");
                MoveToSurfaceGCode(bldr);
                MoveToSurfaceGCode(bldr);
                do
                {
                    targetDepth += tool.PlungeDepth;
                    targetDepth = Math.Min(targetDepth, depth);
                    bldr.AppendLine($"G1 Z{-targetDepth} F{tool.PlungeRate}");
                    bldr.AppendLine($"G1 Y{y2 - actualRadius} F{tool.FeedRate}");
                    bldr.AppendLine($"G2 X{x1 + actualRadius} Y{y2} R{actualRadius}");
                    bldr.AppendLine($"G1 X{x2 - actualRadius} F{tool.FeedRate}");
                    bldr.AppendLine($"G2 X{x2} Y{y2 - actualRadius} R{actualRadius}");
                    bldr.AppendLine($"G1 Y{y1 + actualRadius} F{tool.FeedRate}");
                    bldr.AppendLine($"G2 X{x2 - actualRadius} Y{y1} R{actualRadius}");
                    bldr.AppendLine($"G1 X{x1 + actualRadius} F{tool.FeedRate}");
                    bldr.AppendLine($"G2 X{x1} Y{y1 + actualRadius} R{actualRadius}");
                }
                while (targetDepth != depth);
            }
            else
            {
                bldr.AppendLine($"G1 X{x1} Y{y1} F{project.TravelFeedRate}");
                MoveToSurfaceGCode(bldr);
                do
                {
                    targetDepth += tool.PlungeDepth;
                    targetDepth = Math.Min(targetDepth, depth);
                    bldr.AppendLine($"G1 Z{-targetDepth} F{tool.PlungeRate}");
                    bldr.AppendLine($"G1 Y{y2} F{tool.FeedRate}");
                    bldr.AppendLine($"G1 X{x2} F{tool.FeedRate}");
                    bldr.AppendLine($"G1 Y{y1} F{tool.FeedRate}");
                    bldr.AppendLine($"G1 X{x1} F{tool.FeedRate}");
                }
                while (targetDepth != depth);
            }

            MoveToSafeMoveHeight(project, bldr);
        }

        public void CreateGCode(GCodePolygon polygon, GCodeProject project,  StringBuilder bldr)
        {
            var tool = project.Tools.Single(tool => tool.Id == polygon.GcodeOperationTool.Id);


        }

        public void CreateGCode(GCodePlane plane, GCodeProject project, StringBuilder bldr)
        {
            var tool = project.Tools.Single(tool => tool.Id == plane.GcodeOperationTool.Id);


        }
    }
}
