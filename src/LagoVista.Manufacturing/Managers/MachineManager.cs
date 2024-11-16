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
        private readonly IMachineRepo _MachineRepo;

        public MachineManager(IMachineRepo partRepo, 
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _MachineRepo = partRepo;
        }
        public async Task<InvokeResult> AddMachineAsync(Machine Machine, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(Machine, AuthorizeActions.Create, user, org);
            ValidationCheck(Machine, Actions.Create);
            await _MachineRepo.AddMachineAsync(Machine);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _MachineRepo.GetMachineAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteMachineAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _MachineRepo.GetMachineAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _MachineRepo.DeleteMachineAsync(id);
            return InvokeResult.Success;
        }

        public async Task<Machine> GetMachineAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _MachineRepo.GetMachineAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<MachineSummary>> GetMachineSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(Machine));
            return await _MachineRepo.GetMachineSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateMachineAsync(Machine part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _MachineRepo.UpdateMachineAsync(part);

            return InvokeResult.Success;
        }
    }
}