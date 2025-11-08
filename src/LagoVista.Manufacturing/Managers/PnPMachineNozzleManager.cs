// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d2ead0859c590a1bb94e9fdf12f96bb4033f9b740d1e5ebdcb34b91d364a1078
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;
using LagoVista.Core.Managers;
using LagoVista.Manufacturing.Interfaces.Managers;

namespace LagoVista.Manufacturing.Managers
{
    public class PnPMachineNozzleManager : ManagerBase, IPnPMachineNozzleTipManager
    {
        private readonly IPnpMachineNozzleTipReo _PnPMachineNozzleTipRepo;

        public PnPMachineNozzleManager(IPnpMachineNozzleTipReo partRepo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _PnPMachineNozzleTipRepo = partRepo;
        }
        public async Task<InvokeResult> AddPnPMachineNozzleTipAsync(PnPMachineNozzleTip feeder, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(feeder, AuthorizeActions.Create, user, org);
            ValidationCheck(feeder, Actions.Create);
            await _PnPMachineNozzleTipRepo.AddPnPMachineNozzleTipAsync(feeder);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _PnPMachineNozzleTipRepo.GetPnPMachineNozzleTipAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeletePnPMachineNozzleTipAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _PnPMachineNozzleTipRepo.GetPnPMachineNozzleTipAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _PnPMachineNozzleTipRepo.DeletePnPMachineNozzleTipAsync(id);
            return InvokeResult.Success;
        }

        public async Task<PnPMachineNozzleTip> GetPnPMachineNozzleTipAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _PnPMachineNozzleTipRepo.GetPnPMachineNozzleTipAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<PnPMachineNozzleTipSummary>> GetPnPMachineNozzleTipsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(PnPMachineNozzleTip));
            return await _PnPMachineNozzleTipRepo.GetPnPMachineNozzleTipSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdatePnPMachineNozzleTipAsync(PnPMachineNozzleTip order, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(order, AuthorizeActions.Update, user, org);
            ValidationCheck(order, Actions.Update);
            await _PnPMachineNozzleTipRepo.UpdatePnPMachineNozzleTipAsync(order);

            return InvokeResult.Success;
        }
    }
}
