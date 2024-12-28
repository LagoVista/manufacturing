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

namespace LagoVista.Manufacturing.Managers
{
    public class MachineManager : ManagerBase, IMachineManager
    {
        private readonly IMachineRepo _machineRepo;
        private readonly IGCodeMappingRepo _gcodeRepo;

        public MachineManager(IMachineRepo machienRepo, IGCodeMappingRepo gcodeRepo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _machineRepo = machienRepo;
            _gcodeRepo = gcodeRepo;
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
            if(!EntityHeader.IsNullOrEmpty(machine.GcodeMapping))
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

            for (int idx = 65; idx <= 71; ++idx)
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
    }
}