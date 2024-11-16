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
    public class FeederManager : ManagerBase, IFeederManager
    {
        private readonly IFeederRepo _FeederRepo;

        public FeederManager(IFeederRepo partRepo, 
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _FeederRepo = partRepo;
        }
        public async Task<InvokeResult> AddFeederAsync(Feeder feeder, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(feeder, AuthorizeActions.Create, user, org);
            ValidationCheck(feeder, Actions.Create);
            await _FeederRepo.AddFeederAsync(feeder);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _FeederRepo.GetFeederAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteFeederAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _FeederRepo.GetFeederAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _FeederRepo.DeleteFeederAsync(id);
            return InvokeResult.Success;
        }

        public async Task<Feeder> GetFeederAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _FeederRepo.GetFeederAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<FeederSummary>> GetFeedersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(Feeder));
            return await _FeederRepo.GetFeederSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateFeederAsync(Feeder part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _FeederRepo.UpdateFeederAsync(part);

            return InvokeResult.Success;
        }
    }
}