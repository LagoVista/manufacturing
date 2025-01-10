using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.Manufacturing.Services
{
    public class PickAndPlaceJobResolverService : IPickAndPlaceJobResolverService
    {
        public InvokeResult ResolveParts(PickAndPlaceJob job)
        {
            var commonParts = job.BoardRevision.PcbComponents.Where(prt => prt.Included).GroupBy(prt => prt.PackageAndValue.ToLower());
            foreach (var entries in commonParts)
            {
                var part = new PickAndPlaceJobPart()
                {
                    PcbComponent = entries.First().Component,
                    Value = entries.First().Value,
                };

                foreach(var entry in entries)
                {
                    part.Placements.Add(new PickAndPlaceJobPlacement()
                    {
                        Name = entry.Name,
                        Rotation = entry.Rotation,
                        PCBLocation = new Point2D<double>(entry.X.Value, entry.Y.Value)
                    });
                }                
            }

            return InvokeResult.Success;
        }

        public InvokeResult ResolveJobAsync(Machine machine, PickAndPlaceJob job, CircuitBoard board, List<StripFeeder> stripFeeders, List<AutoFeeder> autoFeeders)
        {
            var revision = board.Revisions.Single(rev => rev.Id == job.BoardRevision.Id);
            job.BoardFiducials.AddRange( revision.PcbComponents.Where(pcbc => pcbc.Fiducial).Select(fid=>new BoardFiducial() { 
                Expected = new Point2D<double>() { X = fid.X.Value, Y = fid.Y.Value } }));

            if(job.BoardFiducials.Count < 2)
            {
                job.Warnings.Add($"Board should have at least 2 fiducials, had {job.BoardFiducials.Count}.");
            }

            foreach (var part in job.Parts)
            {
                if (part.Component == null)
                {
                    job.Errors.Add($"Part {part.Errors}");
                }
                else
                {
                    var autoFeeder = autoFeeders.First(af => af.Component?.Id == part.Component.Id);
                    if (autoFeeder != null)
                    {
                        part.AutoFeeder = autoFeeder.ToEntityHeader();
                    }
                    else
                    {
                        var stripFeeder = stripFeeders.First(sf => sf.Rows.Where(fd => fd.Component?.Id == part.Component.Id).Any());
                        if (stripFeeder != null)
                        {
                            part.StripFeeder = stripFeeder.ToEntityHeader();
                            part.StripFeederRow = stripFeeder.Rows.First(sfr => sfr.Component.Id == part.Component.Id).ToEntityHeader();
                        }
                        else
                        {
                            job.Errors.Add($"Could not find part on machine for {part.Component.Text}");
                        }
                    }
                }
            }

            return InvokeResult.Success;
        }
    }
}
