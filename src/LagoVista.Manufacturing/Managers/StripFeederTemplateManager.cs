using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Managers
{
    internal class StripFeederTemplateManager : ManagerBase, IStripFeederTemplateManager
    {
        private readonly IStripFeederTemplateRepo _stripFeederTemplateRepo;

        public StripFeederTemplateManager(IStripFeederTemplateRepo partRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _stripFeederTemplateRepo = partRepo;
        }
        public async Task<InvokeResult> AddStripFeederTemplateAsync(StripFeederTemplate stripFeederTemplate, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(stripFeederTemplate, AuthorizeActions.Create, user, org);
            ValidationCheck(stripFeederTemplate, Actions.Create);
            await _stripFeederTemplateRepo.AddFeederAsync(stripFeederTemplate);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _stripFeederTemplateRepo.GetFeederAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteStripFeederTemplateAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _stripFeederTemplateRepo.GetFeederAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _stripFeederTemplateRepo.DeleteFeederAsync(id);
            return InvokeResult.Success;
        }

        public async Task<StripFeederTemplate> GetStripFeederTemplateAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _stripFeederTemplateRepo.GetFeederAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<StripFeederTemplateSummary>> GetStripFeederTemplateSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(StripFeederTemplate));
            return await _stripFeederTemplateRepo.GetFeederSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateStripFeederTemplateAsync(StripFeederTemplate stripFeederTemplate, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(stripFeederTemplate, AuthorizeActions.Update, user, org);
            ValidationCheck(stripFeederTemplate, Actions.Update);
            await _stripFeederTemplateRepo.UpdateFeederAsync(stripFeederTemplate);

            return InvokeResult.Success;
        }
    }
}