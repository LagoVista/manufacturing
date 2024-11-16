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
    public class StripFeederManager : ManagerBase, IStripFeederManager
    {
        private readonly IStripFeederRepo _StripFeederRepo;

        public StripFeederManager(IStripFeederRepo partRepo, 
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _StripFeederRepo = partRepo;
        }
        public async Task<InvokeResult> AddStripFeederAsync(StripFeeder feeder, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(feeder, AuthorizeActions.Create, user, org);
            ValidationCheck(feeder, Actions.Create);
            await _StripFeederRepo.AddStripFeederAsync(feeder);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _StripFeederRepo.GetStripFeederAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteStripFeederAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _StripFeederRepo.GetStripFeederAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _StripFeederRepo.DeleteStripFeederAsync(id);
            return InvokeResult.Success;
        }

        public async Task<StripFeeder> GetStripFeederAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _StripFeederRepo.GetStripFeederAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<StripFeederSummary>> GetStripFeedersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(StripFeeder));
            return await _StripFeederRepo.GetStripFeederSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateStripFeederAsync(StripFeeder part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _StripFeederRepo.UpdateStripFeederAsync(part);

            return InvokeResult.Success;
        }
    }
}