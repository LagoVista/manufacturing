// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 00bc779adea5bee8e48b90582e481677537d73200fc8f166443164c9f80afd3d
// IndexVersion: 2
// --- END CODE INDEX META ---
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
using System.Collections.Generic;
using System.Text;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Managers
{
    public class PartPackManager : ManagerBase, IPartPackManager
    {
        private readonly IPartPackRepo _PartPackRepo;

        public PartPackManager(IPartPackRepo partRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _PartPackRepo = partRepo;
        }
        public async Task<InvokeResult> AddPartPackAsync(PartPack part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Create, user, org);
            ValidationCheck(part, Actions.Create);
            await _PartPackRepo.AddPartPackAsync(part);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _PartPackRepo.GetPartPackAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeletePartPackAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _PartPackRepo.GetPartPackAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _PartPackRepo.DeletePartPackAsync(id);
            return InvokeResult.Success;
        }

        public async Task<PartPack> GetPartPackAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _PartPackRepo.GetPartPackAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<PartPackSummary>> GetPartPacksSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(PartPack));
            return await _PartPackRepo.GetPartPackSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdatePartPackAsync(PartPack part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _PartPackRepo.UpdatePartPackAsync(part);

            return InvokeResult.Success;
        }
    }
}