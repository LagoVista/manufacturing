using LagoVista.Core.Models.Drawing;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using PdfSharpCore.Pdf.Filters;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.Manufacturing.Services
{
    public class PickAndPlaceJobResolverService : IPickAndPlaceJobResolverService
    {
        public InvokeResult ResolveParts(PickAndPlaceJob job)
        {
            job.Parts.Clear();

            var commonParts = job.BoardRevision.PcbComponents.Where(prt => prt.Included).GroupBy(prt => prt.PackageAndValue.ToLower());
            foreach (var entries in commonParts)
            {
                var part = new PickAndPlaceJobPart()
                {
                    Component = entries.First().Component,
                    PackageName = entries.First().PackageName,
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

                job.Parts.Add(part);
            }

            return InvokeResult.Success;
        }

        public InvokeResult ResolveJobAsync(Machine machine, PickAndPlaceJob job, CircuitBoardRevision boardRevision, IEnumerable<StripFeeder> stripFeeders, IEnumerable<AutoFeeder> autoFeeders)
        {
            job.BoardFiducials.Clear();
            var fiducials = boardRevision.PcbComponents.Where(pcbc => pcbc.Fiducial).Select(fid => new BoardFiducial()
            {
                Name = fid.Name,
                Expected = new Point2D<double>() { X = fid.X.Value, Y = fid.Y.Value },                
            });

            foreach(var fiducial in fiducials)
                job.BoardFiducials.Add(fiducial);

            job.Warnings = new System.Collections.ObjectModel.ObservableCollection<string>();
            job.Errors = new System.Collections.ObjectModel.ObservableCollection<string>();
            job.Warnings.Clear();
            job.Errors.Clear();

            if(job.BoardFiducials.Count < 2)
            {
                job.Warnings.Add($"Board should have at least 2 fiducials, had {job.BoardFiducials.Count}.");
            }

            foreach (var part in job.Parts)
            {
                part.AutoFeeder = null;
                part.StripFeeder = null;
            }

            foreach (var part in job.Parts)
            {
                if (part.Component == null)
                {
                    job.Errors.Add($"Part {part.PackageName}/{part.Value} does not have a component");
                }
                else
                {
                    var autoFeeder = autoFeeders.FirstOrDefault(af => af.Component?.Id == part.Component.Id);
                    if (autoFeeder != null)
                    {
                        part.AutoFeeder = autoFeeder.ToEntityHeader();
                        part.AvailableCount = autoFeeder.PartCount;

                    }
                    else
                    {
                        var stripFeeder = stripFeeders.FirstOrDefault(sf => sf.Rows.Where(fd => fd.Component?.Id == part.Component.Id).Any());
                        if (stripFeeder != null)
                        {
                            part.StripFeeder = stripFeeder.ToEntityHeader();
                            var row = stripFeeder.Rows.First(sfr => sfr.Component?.Id == part.Component.Id);
                            part.StripFeederRow = row.ToEntityHeader();
                            part.AvailableCount = row.PartCapacity;
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
