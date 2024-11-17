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

namespace LagoVista.Manufacturing.Managers
{
    public class MachineManager : ManagerBase, IMachineManager
    {
        private readonly IMachineRepo _machineRepo;

        public MachineManager(IMachineRepo machienRepo, 
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _machineRepo = machienRepo;
        }
        public async Task<InvokeResult> AddMachineAsync(Machine machine, EntityHeader org, EntityHeader user)
        {
            Console.WriteLine("==== 1) Adding machine  ==>" + machine.Name);

            await AuthorizeAsync(machine, AuthorizeActions.Create, user, org);
            Console.WriteLine("==== 2) Adding machine  ==>" + machine.Name);

            ValidationCheck(machine, Actions.Create);

            Console.WriteLine("==== 3) Adding machine  ==>" + machine.Name);

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
            var part = await _machineRepo.GetMachineAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _machineRepo.DeleteMachineAsync(id);
            return InvokeResult.Success;
        }

        public async Task<Machine> GetMachineAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _machineRepo.GetMachineAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
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
    }
}