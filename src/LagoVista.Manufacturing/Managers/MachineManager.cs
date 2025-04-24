using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;
using System.Collections.Generic;
using LagoVista.Core.Models.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace LagoVista.Manufacturing.Managers
{
    public class MachineManager : ManagerBase, IMachineManager
    {
        private readonly IMachineRepo _machineRepo;
        private readonly IGCodeMappingRepo _gcodeRepo;
        private readonly IStripFeederManager _stripFeederManager;
        private readonly IAutoFeederManager _autoFeederManager;
        private readonly IComponentManager _componentManager;


        public MachineManager(IMachineRepo machienRepo, IGCodeMappingRepo gcodeRepo,
            IStripFeederManager stripFeederManager, IAutoFeederManager autoFeederManager, IComponentManager componentManager,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _machineRepo = machienRepo ?? throw new ArgumentNullException(nameof(machienRepo));
            _gcodeRepo = gcodeRepo ?? throw new ArgumentNullException(nameof(gcodeRepo));
            _stripFeederManager = stripFeederManager ?? throw new ArgumentNullException(nameof(stripFeederManager));
            _autoFeederManager = autoFeederManager ?? throw new ArgumentNullException(nameof(autoFeederManager));
            _componentManager = componentManager ?? throw new ArgumentNullException(nameof(componentManager));

        }
        public async Task<InvokeResult> AddMachineAsync(Machine machine, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(machine, AuthorizeActions.Create, user, org);
            ValidationCheck(machine, Actions.Create);
            await _machineRepo.AddMachineAsync(machine);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _machineRepo.GetMachineAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteMachineAsync(string id, EntityHeader org, EntityHeader user)
        {
            var machine = await _machineRepo.GetMachineAsync(id);
            await ConfirmNoDepenenciesAsync(machine);
            await AuthorizeAsync(machine, AuthorizeActions.Delete, user, org);
            await _machineRepo.DeleteMachineAsync(id);
            return InvokeResult.Success;
        }

        public async Task<Machine> GetMachineAsync(string id, EntityHeader org, EntityHeader user)
        {
            var machine = await _machineRepo.GetMachineAsync(id);
            await AuthorizeAsync(machine, AuthorizeActions.Read, user, org);
            if (!EntityHeader.IsNullOrEmpty(machine.GcodeMapping))
            {
                machine.GcodeMapping.Value = await _gcodeRepo.GetGCodeMappingAsync(machine.GcodeMapping.Id);
            }
            return machine;
        }


        public async Task<ListResponse<MachineSummary>> GetMachineSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(Machine));
            return await _machineRepo.GetMachineSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateMachineAsync(Machine machine, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(machine, AuthorizeActions.Update, user, org);
            ValidationCheck(machine, Actions.Update);
            await _machineRepo.UpdateMachineAsync(machine);

            return InvokeResult.Success;
        }


        public static List<EnumDescription> GetStagingPlateRows()
        {
            var options = new List<EnumDescription>();
            options.Add(new EnumDescription() { Id = "-1", Key = "-1", Text = "-select row-", Name = "-select row-", Label = "-select row-" });

            for (int idx = 65; idx <= 80; ++idx)
            {
                options.Add(new EnumDescription()
                {
                    Id = $"{Char.ConvertFromUtf32(idx)}",
                    Key = $"{Char.ConvertFromUtf32(idx)}",
                    Label = $"{Char.ConvertFromUtf32(idx)}",
                    Text = $"{Char.ConvertFromUtf32(idx)}",
                    Name = $"{Char.ConvertFromUtf32(idx)}",
                });
            }

            return options;
        }

        public static List<EnumDescription> GetStagingPlateColumns()
        {
            var options = new List<EnumDescription>();
            options.Add(new EnumDescription() { Id = "-1", Key = "-1", Text = "-select column-", Name = "-select column-", Label = "-select column-" });

            for (int idx = 1; idx <= 39; ++idx)
            {
                options.Add(new EnumDescription()
                {
                    Id = $"{idx}",
                    Key = $"{idx}",
                    Label = $"{idx}",
                    Text = $"{idx}",
                    Name = $"{idx}"
                });
            }

            return options;
        }

        public async Task<InvokeResult<PcbJobTestFit>> TestFitJobAsync(string machineId, CircuitBoardRevision boardRevision, EntityHeader org, EntityHeader user)
        {
            var jobTestFit = new PcbJobTestFit();

            var autoFeeders = await _autoFeederManager.GetFeedersForMachineAsync(machineId, true, org, user);
            var stripFeeders = await _stripFeederManager.GetStripFeedersForMachineAsync(machineId, true, org, user);


            var fiducials = boardRevision.PcbComponents.Where(pcbc => pcbc.Fiducial).Select(fid => new BoardFiducial()
            {
                Name = fid.Name,
                Expected = new Point2D<double>() { X = fid.X.Value, Y = fid.Y.Value },
            });

            foreach (var fiducial in fiducials)
                jobTestFit.Fiducials.Add(fiducial);

            jobTestFit.Warnings = new System.Collections.ObjectModel.ObservableCollection<string>();
            jobTestFit.Errors = new System.Collections.ObjectModel.ObservableCollection<string>();

            if (jobTestFit.Fiducials.Count < 2)
            {
                jobTestFit.Warnings.Add($"Board should have at least 2 fiducials, had {jobTestFit.Fiducials.Count}.");
            }

            var commonParts = boardRevision.PcbComponents.Where(prt => prt.Included).GroupBy(prt => prt.PackageAndValue.ToLower());
            foreach (var entries in commonParts)
            {
                var part = new PartsGroup()
                {
                    Component = entries.First().Component,
                    PackageName = entries.First().PackageName,
                    Value = entries.First().Value,
                };

                foreach (var entry in entries)
                {
                    part.Placements.Add(new PickAndPlaceJobPlacement()
                    {
                        Name = entry.Name,
                        Rotation = entry.Rotation,
                        PCBLocation = new Point2D<double>(entry.X.Value, entry.Y.Value)
                    });
                }

                jobTestFit.PlacedPartsGroups.Add(part);
            }

            var manualParts = boardRevision.PcbComponents.Where(prt => prt.ManualPlace).GroupBy(prt => prt.PackageAndValue.ToLower());
            foreach (var entries in manualParts)
            {
                var part = new PartsGroup()
                {
                    Component = entries.First().Component,
                    PackageName = entries.First().PackageName,
                    Value = entries.First().Value,
                };

                foreach (var entry in entries)
                {
                    part.Placements.Add(new PickAndPlaceJobPlacement()
                    {
                        Name = entry.Name,
                        Rotation = entry.Rotation,
                        PCBLocation = new Point2D<double>(entry.X.Value, entry.Y.Value)
                    });
                }

                jobTestFit.ManualPartGroups.Add(part);
            }

            foreach (var part in jobTestFit.PlacedPartsGroups)
            {
                if (part.Component == null)
                {
                    jobTestFit.Errors.Add($"Part {part.PackageName}/{part.Value} does not have a component");
                }
                else
                {
                    var autoFeeder = autoFeeders.Model.FirstOrDefault(af => af.Component?.Id == part.Component.Id);
                    if (autoFeeder != null)
                    {
                        part.AutoFeeder = autoFeeder.ToEntityHeader();
                        part.AvailableCount = autoFeeder.PartCount;

                        if(autoFeeder.Component.HasValue)
                        {
                            part.ComponentPackage = autoFeeder.Component.Value.ComponentPackage;
                        }
                    }
                    else
                    {
                        var stripFeeder = stripFeeders.Model.FirstOrDefault(sf => sf.Rows.Where(fd => fd.Component?.Id == part.Component.Id).Any());
                        if (stripFeeder != null)
                        {
                            part.StripFeeder = stripFeeder.ToEntityHeader();
                            var row = stripFeeder.Rows.First(sfr => sfr.Component?.Id == part.Component.Id);
                            if(row.Component.HasValue)
                            {
                                part.ComponentPackage = row.Component.Value.ComponentPackage;
                            }

                            part.StripFeederRow = row.ToEntityHeader();
                            part.AvailableCount = row.PartCapacity;
                        }
                    }

                    if(part.ComponentPackage == null)
                    {
                        var component = await _componentManager.GetComponentAsync(part.Component.Id, false, org, user);
                        part.ComponentPackage = component.ComponentPackage;
                    }
                }
            }

            var unresolvedParts = boardRevision.PcbComponents.Where(prt => !prt.ManualPlace && !prt.Ignore && !prt.Included && !prt.Fiducial);
            foreach (var part in unresolvedParts)
                jobTestFit.Errors.Add($"Part not resolved (not included, manual or ignored) {part.Name} {part.Value}, {part.PackageName}.");

            return InvokeResult<PcbJobTestFit>.Create(jobTestFit);
        }
    }
}