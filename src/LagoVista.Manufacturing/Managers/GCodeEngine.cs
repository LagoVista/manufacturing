﻿using LagoVista.Core;
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PCB.Eagle.Managers
{
    public class GCodeEngine
    {
        /* Find the replacement bit that shoud be used */
        private static DrillBit GetConsolidated(Drill drill, ObservableCollection<ConsolidatedDrillBit> consolidatedBits)
        {
            foreach (var consolidatedBit in consolidatedBits)
            {
                foreach (var bit in consolidatedBit.Bits)
                {
                    if (bit.Diameter == drill.D)
                    {
                        return new DrillBit()
                        {
                            Diameter = consolidatedBit.Diameter,
                            ToolName = consolidatedBit.NewToolName
                        };
                    }
                }
            }

            return null;
        }

        public static List<DrillRackInfo> GetToolRack(PrintedCircuitBoard pcb, PcbMillingProject pcbProject)
        {
            if (pcbProject.PauseForToolChange)
            {
                //All the holes that need to be drilled in the board.
                var allDrills = pcb.Drills.ToList();


                var modifiedDrills = new List<Drill>();
                foreach (var drill in allDrills)
                {
                    var modifiedDrill = new Drill()
                    {
                        X = drill.X,
                        Y = drill.Y,
                    };

                    /* Attempt to find a match for drill based on diameter */
                    var consolidatedBit = GetConsolidated(drill, pcbProject.ConsolidatedDrillRack);

                    /* If it found a match replace it, otherwise assign to original */
                    modifiedDrill.D = consolidatedBit == null ? drill.D : consolidatedBit.Diameter;

                    modifiedDrills.Add(modifiedDrill);
                }

                var tools = modifiedDrills.GroupBy(drl => drl.D);

                var bits = (from tool
                         in tools
                            orderby tool.First().D
                            select new DrillRackInfo()
                            {
                                Diameter = tool.First().D,
                                DrillCount = tool.Count(),
                            }).ToList();

                var idx = 1;
                foreach (var bit in bits)
                {
                    if (bit.DrillName != null)
                    {
                        bit.DrillName = $"T{idx++}";
                    }
                }

                return bits;
            }
            else
            {
                return (from tool
                         in pcb.Drills.GroupBy(drl => drl.D)
                        orderby tool.First().D
                        select new DrillRackInfo()
                        {
                            Diameter = tool.First().D,
                            DrillCount = tool.Count(),
                        }).ToList();
            }
        }


        public static string CreateDrillGCode(PrintedCircuitBoard pcb, PcbMillingProject pcbProject)
        {
            var bldr = new StringBuilder();
            bldr.AppendLine("(Metric Mode)");
            bldr.AppendLine("G21");
            bldr.AppendLine("(Absolute Coordinates)");
            bldr.AppendLine("G90");

            if (pcbProject.PauseForToolChange)
            {
                var allDrills = pcb.Drills.ToList();

                var modifiedDrills = new List<Drill>();
                foreach (var drill in allDrills)
                {
                    var modifiedDrill = new Drill()
                    {
                        X = drill.X,
                        Y = drill.Y,
                    };

                    var consolidatedBit = GetConsolidated(drill, pcbProject.ConsolidatedDrillRack);
                    modifiedDrill.D = consolidatedBit == null ? drill.D : consolidatedBit.Diameter;

                    modifiedDrills.Add(modifiedDrill);
                }

                var tools = modifiedDrills.GroupBy(drl => drl.D);
                foreach(var tool in tools)
                {
                    bldr.AppendLine($"( {tool.First().D.ToString().Replace("TC","T")} : {tool.First().D.ToDim()} ) ");
                }
                

                /* Should be OK to do first here */
                foreach (var tool in tools.OrderBy(tl => tl.First().D))
                {
                    bldr.AppendLine("M05");
                    bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
                    bldr.AppendLine($"M06 {tool.First().D.ToString().Replace("TC","T")}; {tool.First().D.ToDim()}");
                    bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
                    bldr.AppendLine($"G00 X0.0000 Y0.0000");
                    bldr.AppendLine("M03");
                    bldr.AppendLine($"S{pcbProject.DrillSpindleRPM}");
                    bldr.AppendLine($"G04 P{pcbProject.DrillSpindleDwell}");

                    foreach (var drill in tool.OrderBy(drl => drl.X).ThenBy(drl => drl.Y))
                    {
                        bldr.AppendLine($"G01 X{(drill.X + pcbProject.ScrapSides).ToDim()} Y{(drill.Y + pcbProject.ScrapTopBottom).ToDim()} F1000");
                        bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
                        bldr.AppendLine($"G01 Z-{pcbProject.StockThickness.ToDim()} F{pcbProject.DrillPlungeRate}");
                        bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");
                    }
                }
            }
            else
            {
                var allDrills = pcb.Drills.OrderBy(drl => drl.X).ThenBy(drl => drl.Y);

                bldr.AppendLine("M03");
                bldr.AppendLine($"S{pcbProject.DrillSpindleRPM}");
                bldr.AppendLine($"G04 P{pcbProject.DrillSpindleDwell}");

                foreach (var drill in allDrills)
                {
                    bldr.AppendLine($"G01 X{(drill.X + pcbProject.ScrapSides).ToDim()} Y{(drill.Y + pcbProject.ScrapTopBottom).ToDim()} F1000");
                    bldr.AppendLine($"G01 Z0 F{pcbProject.SafePlungeRecoverRate}");
                    bldr.AppendLine($"G01 Z-{pcbProject.StockThickness.ToDim()} F{pcbProject.DrillPlungeRate}");
                    bldr.AppendLine($"G01 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");
                }
            }

            bldr.AppendLine("M05");
            bldr.AppendLine("G00 X0 Y0");

            return bldr.ToString();
        }

        /// <summary>
        /// Create GCode that will drill holes to secure the board to the hold down fixture
        /// </summary>
        /// <param name="pcb">PCB Specficiation</param>
        /// <param name="pcbProject">Details about the PCB Project</param>
        /// <param name="drillIntoUnderlayment">If this is true, holes will be drilled into the underlayment or fixture the board is mounted on.  This really only should be done the first time since once holes are created they can be reused and redrilling may result in an undesired offset.</param>
        /// <returns></returns>
        public static string CreateHoldDownGCode(PrintedCircuitBoard pcb, PcbMillingProject pcbProject, bool drillIntoUnderlayment)
        {
            var bldr = new StringBuilder();

            bldr.AppendLine("(Metric Mode)");
            bldr.AppendLine("G21");
            bldr.AppendLine("(Absolute Coordinates)");
            bldr.AppendLine("G90");
            bldr.AppendLine("M05");

            var leftDrillX = (pcbProject.ScrapSides - pcbProject.HoldDownBoardOffset);
            var rightDrillX = (pcbProject.ScrapSides + pcbProject.HoldDownBoardOffset + pcb.Width);
            var sideDrillY = (pcbProject.ScrapTopBottom + (pcb.Height / 2));

            var topBottomDrillX = (pcbProject.ScrapSides + (pcb.Width / 2));
            var topDrillY = (pcbProject.ScrapTopBottom + pcb.Height + pcbProject.HoldDownBoardOffset);
            var bottomDrillY = (pcbProject.ScrapTopBottom - pcbProject.HoldDownBoardOffset);


            bldr.AppendLine("M05");
            bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
            bldr.AppendLine($"G00 X0.0000 Y0.0000");
            bldr.AppendLine($"M06 {pcbProject.HoldDownDiameter.ToDim()}");
            bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
            bldr.AppendLine("M03");
            bldr.AppendLine($"S{pcbProject.DrillSpindleRPM}");

            bldr.AppendLine($"G04 P{pcbProject.DrillSpindleDwell}");

            //bldr.AppendLine($"G04 {pcbProject.DrillSpindleDwell}");

            var initialDrillDepth = pcbProject.HoldDownDiameter == pcbProject.HoldDownDrillDiameter && drillIntoUnderlayment ? -pcbProject.HoldDownDrillDepth : -pcbProject.StockThickness;
            //var drills = pcbProject.GetHoldDownDrills(pcb);
            //foreach (var hole in drills)
            //{
            //    bldr.AppendLine($"G00 X{hole.X.ToDim()} Y{hole.Y.ToDim()}");
            //    bldr.AppendLine($"G00 Z2 F{pcbProject.SafePlungeRecoverRate}");
            //    bldr.AppendLine($"G01 Z{initialDrillDepth.ToDim()} F{pcbProject.DrillPlungeRate}");
            //    bldr.AppendLine($"G00 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");
            //}
            

            //if (pcbProject.HoldDownDiameter != pcbProject.HoldDownDrillDiameter && drillIntoUnderlayment)
            //{
            //    bldr.AppendLine("M05");
            //    bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
            //    bldr.AppendLine($"G00 X0.0000 Y0.0000");
            //    bldr.AppendLine($"M06 {pcbProject.HoldDownDrillDiameter.ToDim()}");
            //    bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");

            //    bldr.AppendLine("M03");
            //    bldr.AppendLine($"S{pcbProject.DrillSpindleRPM}");
            //    //bldr.AppendLine($"G04 {pcbProject.DrillSpindleDwell}");

            //    foreach (var hole in drills)
            //    {
            //        bldr.AppendLine($"G00 X{hole.X.ToDim()} Y{hole.Y.ToDim()}");
            //        bldr.AppendLine($"G00 Z2 F{pcbProject.SafePlungeRecoverRate}");
            //        bldr.AppendLine($"G01 Z-{pcbProject.HoldDownDrillDepth.ToDim()} F{pcbProject.DrillPlungeRate}");
            //        bldr.AppendLine($"G00 Z{pcbProject.DrillSafeHeight} F{pcbProject.SafePlungeRecoverRate}");
            //    }
            //}

            Debugger.Break(); // THIS NEEDS TO BE IMPLEMENTED

            bldr.AppendLine("M05");
            bldr.AppendLine($"G0 Z{pcbProject.DrillSafeHeight.ToDim()}");
            bldr.AppendLine($"G00 X0.0000 Y0.0000");

            return bldr.ToString();
        }

        public static string CreateCutoutMill(PrintedCircuitBoard pcb, PcbMillingProject pcbProject)
        {
            var bldr = new StringBuilder();

            bldr.AppendLine("(Metric Mode)");
            bldr.AppendLine("G21");
            bldr.AppendLine("(Absolute Coordinates)");
            bldr.AppendLine("G90");

            bldr.AppendLine("M03");
            bldr.AppendLine($"S{pcbProject.MillSpindleRPM}");
            bldr.AppendLine($"G04 P{pcbProject.MillSpindleDwell}");

            double millRadius = pcbProject.MillToolSize / 2;
            double scrapX = pcbProject.ScrapSides;
            double scrapY = pcbProject.ScrapTopBottom;
            double width = pcb.Width;
            double height = pcb.Height;

            var cornerWires = pcb.Layers.Where(layer => layer.Layer == PCBLayers.BoardOutline).FirstOrDefault().Wires.Where(wire => wire.Crv.HasValue == true);

            var boardOutline = pcb.Layers.Single(layer => layer.Layer == PCBLayers.BoardOutline);

            /* Major hack here */
            var radius = cornerWires.Any() ? Math.Abs(cornerWires.First().X1 - cornerWires.First().X2) : 0;

            if (radius == 0)
            {
                var first = boardOutline.Wires.First();

                var depth = 0.0;
                bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                bldr.AppendLine($"G01 X{(scrapX + first.X1 + millRadius).ToDim()} Y{(scrapY + first.Y1 + millRadius).ToDim()} F{pcbProject.MillFeedRate}"); /* Move to bottom right */
                bldr.AppendLine($"G00 Z0 F{pcbProject.MillPlungeRate}");

                depth -= pcbProject.MillCutDepth;

                while (depth > -pcbProject.StockThickness)
                {
                    depth = Math.Min(depth, pcbProject.StockThickness);
                    bldr.AppendLine($"G01 Z{depth.ToDim()} F{pcbProject.MillPlungeRate}"); /* Move to cut depth interval at 0,0 */

                    foreach(var wire in boardOutline.Wires)
                    {
                        bldr.AppendLine($"G01 X{(scrapX + wire.X1 + millRadius).ToDim()} Y{(scrapY + wire.Y1 + millRadius).ToDim()} F{pcbProject.MillFeedRate}"); /* Move to bottom right */
                    }

                    var last = boardOutline.Wires.Last();
                    bldr.AppendLine($"G01 X{(scrapX + last.X1 + millRadius).ToDim()} Y{(scrapY + last.Y1 + millRadius).ToDim()} F{pcbProject.MillFeedRate}"); /* Move to bottom right */

                    depth -= pcbProject.MillCutDepth;
                }

                bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                bldr.AppendLine("M05");
                bldr.AppendLine("G0 X0 Y0");
            }
            else
            {

                var depth = 0.0;
                depth += pcbProject.MillCutDepth;
                bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                bldr.AppendLine($"G00 X{(scrapX + radius).ToDim()} Y{(scrapY - millRadius).ToDim()}");

                bldr.AppendLine($"G01 Z0 F{pcbProject.MillPlungeRate}");

                depth -= pcbProject.MillCutDepth;

                while (depth > -pcbProject.StockThickness)
                {
                    depth = Math.Min(depth, pcbProject.StockThickness);
                    bldr.AppendLine($"G01 Z{depth.ToDim()} F{pcbProject.MillPlungeRate}");

                    bldr.AppendLine($"G00 X{(scrapX + (width - radius)).ToDim()} Y{(scrapY - millRadius).ToDim()} F{pcbProject.MillFeedRate}");

                    bldr.AppendLine($"G03 X{(scrapX + (width + millRadius)).ToDim()} Y{(scrapY + radius).ToDim()} R{radius + millRadius}");

                    bldr.AppendLine($"G00 X{(scrapX + (width + millRadius)).ToDim()} Y{(scrapY + (height - radius)).ToDim()}");

                    bldr.AppendLine($"G03 X{(scrapX + (width - radius)).ToDim()} Y{(scrapY + (height + millRadius)).ToDim()} R{radius + millRadius}");

                    bldr.AppendLine($"G0 X{(scrapX + radius).ToDim()} Y{(scrapY + (height + millRadius)).ToDim()}");

                    bldr.AppendLine($"G03 X{(scrapX - millRadius).ToDim()} Y{(scrapY + (height - radius)).ToDim()} R{radius + millRadius}");

                    bldr.AppendLine($"G0 X{(scrapX - millRadius).ToDim()} Y{(scrapY + radius).ToDim()}");

                    bldr.AppendLine($"G03 X{(scrapX + radius).ToDim()} Y{(scrapY - millRadius).ToDim()} R{radius + millRadius}");

                    depth -= pcbProject.MillCutDepth;
                }

                bldr.AppendLine($"G00 Z{pcbProject.MillSafeHeight}");
                bldr.AppendLine("M05");
                bldr.AppendLine("G0 X0 Y0");
            }


            return bldr.ToString();
        }
    }
}
