// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0b7e8df2b24cf42ca8025379ac38ec7c5a8fac8a5edb4e6a98e9361c9e8fb009
// IndexVersion: 0
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
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;
using System.Text;

namespace LagoVista.Manufacturing.Managers
{
    public class GCodeMappingManager : ManagerBase, IGCodeMappingManager
    {
        private readonly IGCodeMappingRepo _repo;
        private readonly IComponentManager _componentManager;
        private readonly IComponentPackageRepo _packageRepo;

        public GCodeMappingManager(IGCodeMappingRepo GCodeMappingRepo, IComponentManager componentManager, IComponentPackageRepo packageRepo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _repo = GCodeMappingRepo;
            _componentManager = componentManager ?? throw new ArgumentNullException(nameof(componentManager));
            _packageRepo = packageRepo ?? throw new ArgumentNullException(nameof(packageRepo));
        }
        public async Task<InvokeResult> AddGCodeMappingAsync(GCodeMapping mapping, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(mapping, AuthorizeActions.Create, user, org);
            ValidationCheck(mapping, Actions.Create);
            await _repo.AddGCodeMappingAsync(mapping);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _repo.GetGCodeMappingAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

      
        public async Task<InvokeResult> DeleteGCodeMappingAsync(string id, EntityHeader org, EntityHeader user)
        {
            var GCodeMapping = await _repo.GetGCodeMappingAsync(id);
            await ConfirmNoDepenenciesAsync(GCodeMapping);
            await AuthorizeAsync(GCodeMapping, AuthorizeActions.Delete, user, org);
            await _repo.DeleteGCodeMappingAsync(id);
            return InvokeResult.Success;
        }


        public async Task<GCodeMapping> GetGCodeMappingAsync(string id,  EntityHeader org, EntityHeader user)
        {
            var GCodeMapping = await _repo.GetGCodeMappingAsync(id);
            await AuthorizeAsync(GCodeMapping, AuthorizeActions.Read, user, org);
            return GCodeMapping;
        }

        public async Task<ListResponse<GCodeMappingSummary>> GetGCodeMappingsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(GCodeMapping));
            return await _repo.GetGCodeMappingSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateGCodeMappingAsync(GCodeMapping mapping, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(mapping, AuthorizeActions.Update, user, org);
            ValidationCheck(mapping, Actions.Update);
            await _repo.UpdateGCodeMappingAsync(mapping);
            return InvokeResult.Success;
        }
    }
}