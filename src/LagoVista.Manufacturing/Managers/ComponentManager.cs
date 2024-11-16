using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
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
    public class ComponentManager : ManagerBase, IComponentManager
    {
        private readonly IComponentRepo _componentRepo;

        public ComponentManager(IComponentRepo partRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _componentRepo = partRepo;
        }
        public async Task<InvokeResult> AddComponentAsync(Component part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Create, user, org);
            ValidationCheck(part, Actions.Create);
            await _componentRepo.AddComponentAsync(part);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentRepo.GetComponentAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteComponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentRepo.GetComponentAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _componentRepo.DeleteComponentAsync(id);
            return InvokeResult.Success;
        }

        public async Task<Component> GetComponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _componentRepo.GetComponentAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }

  
        public async Task<ListResponse<ComponentSummary>> GetComponentsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(Component));
            return await _componentRepo.GetComponentSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateComponentAsync(Component part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _componentRepo.UpdateComponentAsync(part);

            return InvokeResult.Success;
        }
    }
}
