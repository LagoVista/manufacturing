using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Managers
{
    public class PcbMillingProjectManager : ManagerBase, IPcbMillingProjectManager
    {
        private readonly IPcbMillingProjectRepo _pcbMillingProjectRepo;

        public PcbMillingProjectManager(IPcbMillingProjectRepo partRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _pcbMillingProjectRepo = partRepo;
        }
        public async Task<InvokeResult> AddPcbMillingProjectAsync(PcbMillingProject project, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(project, AuthorizeActions.Create, user, org);
            ValidationCheck(project, Actions.Create);
            await _pcbMillingProjectRepo.AddPcbMillingProjectAsync(project);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _pcbMillingProjectRepo.GetPcbMillingProjectAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public async Task<InvokeResult> DeletePcbMillingProjectAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _pcbMillingProjectRepo.GetPcbMillingProjectAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _pcbMillingProjectRepo.DeletePcbMillingProjectAsync(id);
            return InvokeResult.Success;
        }

        public async Task<PcbMillingProject> GetPcbMillingProjectAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _pcbMillingProjectRepo.GetPcbMillingProjectAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<PcbMillingProjectSummary>> GetPcbMillingProjectsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(PcbMillingProject));
            return await _pcbMillingProjectRepo.GetPcbMillingProjectSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdatePcbMillingProjectAsync(PcbMillingProject project, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(project, AuthorizeActions.Update, user, org);
            ValidationCheck(project, Actions.Update);
            await _pcbMillingProjectRepo.UpdatePcbMillingProjectAsync(project);

            return InvokeResult.Success;
        }
    }
}