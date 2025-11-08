// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a9efe918968ea7e91daaeaea9dd4f66d2a0e7f72d7790a5aa821486c187ee43d
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
using System.Threading.Tasks;
using static LagoVista.Core.Models.AuthorizeResult;

namespace LagoVista.Manufacturing.Managers
{
    public class InventoryLocationManager : ManagerBase, IInventoryLocationManager
    {
        private readonly IInventoryLocationRepo _InventoryLocationRepo;

        public InventoryLocationManager(IInventoryLocationRepo partRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _InventoryLocationRepo = partRepo;
        }
        public async Task<InvokeResult> AddInventoryLocationAsync(InventoryLocation part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Create, user, org);
            ValidationCheck(part, Actions.Create);
            await _InventoryLocationRepo.AddInventoryLocationAsync(part);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _InventoryLocationRepo.GetInventoryLocationAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteInventoryLocationAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _InventoryLocationRepo.GetInventoryLocationAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _InventoryLocationRepo.DeleteInventoryLocationAsync(id);
            return InvokeResult.Success;
        }

        public async Task<InventoryLocation> GetInventoryLocationAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _InventoryLocationRepo.GetInventoryLocationAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<InventoryLocationSummary>> GetInventoryLocationsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(InventoryLocation));
            return await _InventoryLocationRepo.GetInventoryLocationSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateInventoryLocationAsync(InventoryLocation part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _InventoryLocationRepo.UpdateInventoryLocationAsync(part);

            return InvokeResult.Success;
        }
    }
}