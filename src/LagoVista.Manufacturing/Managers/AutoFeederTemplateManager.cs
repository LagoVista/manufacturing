using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Core.Managers;

namespace LagoVista.Manufacturing.Managers
{
    internal class AutoFeederTemplateManager : ManagerBase, IAutoFeederTemplateManager
    {
        private readonly IAutoFeederTemplateRepo _autoFeederTemplateRepo;

        public AutoFeederTemplateManager(IAutoFeederTemplateRepo partRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _autoFeederTemplateRepo = partRepo;
        }
        public async Task<InvokeResult> AddAutoFeederTemplateAsync(AutoFeederTemplate autoFeederTemplate, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(autoFeederTemplate, AuthorizeActions.Create, user, org);
            ValidationCheck(autoFeederTemplate, Actions.Create);
            await _autoFeederTemplateRepo.AddFeederAsync(autoFeederTemplate);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _autoFeederTemplateRepo.GetFeederAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteAutoFeederTemplateAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _autoFeederTemplateRepo.GetFeederAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _autoFeederTemplateRepo.DeleteFeederAsync(id);
            return InvokeResult.Success;
        }

        public async Task<AutoFeederTemplate> GetAutoFeederTemplateAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _autoFeederTemplateRepo.GetFeederAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }

        public async Task<ListResponse<AutoFeederTemplateSummary>> GetAutoFeederTemplateSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(AutoFeederTemplate));
            return await _autoFeederTemplateRepo.GetFeederSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateAutoFeederTemplateAsync(AutoFeederTemplate autoFeederTemplate, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(autoFeederTemplate, AuthorizeActions.Update, user, org);
            ValidationCheck(autoFeederTemplate, Actions.Update);
            await _autoFeederTemplateRepo.UpdateFeederAsync(autoFeederTemplate);

            return InvokeResult.Success;
        }
    }
}