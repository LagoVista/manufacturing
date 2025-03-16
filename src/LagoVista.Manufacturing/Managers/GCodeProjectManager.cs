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
using LagoVista.Manufacturing.Services;
using System.Linq;

namespace LagoVista.Manufacturing.Managers
{
    public class GCodeProjectManager : ManagerBase, IGCodeProjectManager
    {
        private readonly IGCodeProjectRepo _gcodeProjectRepo;
        private readonly IGCodeBuilder _gcodeBuilder;

        public GCodeProjectManager(IGCodeProjectRepo partRepo, IAdminLogger logger, IGCodeBuilder gcodeBuilder, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _gcodeProjectRepo = partRepo;
            _gcodeBuilder = gcodeBuilder ?? throw new ArgumentNullException(nameof(gcodeBuilder));
        }
        public async Task<InvokeResult> AddGCodeProjectAsync(GCodeProject part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Create, user, org);
            ValidationCheck(part, Actions.Create);
            await _gcodeProjectRepo.AddGCodeProjectAsync(part);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _gcodeProjectRepo.GetGCodeProjectAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult<string[]>> CreateGCode(GCodeProject project)
        {
            var bldr = new StringBuilder();

            _gcodeBuilder.CreateGCode(project, bldr);

            var lines = bldr.ToString().Split('\n').Select(s=>s.Trim());

            return Task.FromResult(InvokeResult<string[]>.Create(lines.ToArray()));
        }


        public Task<InvokeResult<string[]>> GetGCodeForProjectAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteGCodeProjectAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _gcodeProjectRepo.GetGCodeProjectAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _gcodeProjectRepo.DeleteGCodeProjectAsync(id);
            return InvokeResult.Success;
        }

        public async Task<GCodeProject> GetGCodeProjectAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _gcodeProjectRepo.GetGCodeProjectAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<GCodeProjectSummary>> GetGCodeProjectsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(GCodeProject));
            return await _gcodeProjectRepo.GetGCodeProjectSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateGCodeProjectAsync(GCodeProject part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _gcodeProjectRepo.UpdateGCodeProjectAsync(part);

            return InvokeResult.Success;
        }
    }
}