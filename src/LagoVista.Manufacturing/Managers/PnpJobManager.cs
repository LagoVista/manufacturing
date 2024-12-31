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
using System.Drawing.Text;
using System.Collections.Generic;

namespace LagoVista.Manufacturing.Managers
{
    public class PickAndPlaceJobManager : ManagerBase, IPickAndPlaceJobManager
    {
        private readonly IPickAndPlaceJobRepo _PickAndPlaceJobRepo;
        private readonly IComponentRepo _componentRepo;

        public PickAndPlaceJobManager(IPickAndPlaceJobRepo partRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _PickAndPlaceJobRepo = partRepo;
        }
        public async Task<InvokeResult> AddPickAndPlaceJobAsync(PickAndPlaceJob job, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(job, AuthorizeActions.Create, user, org);
            ValidationCheck(job, Actions.Create);
            await _PickAndPlaceJobRepo.AddPickAndPlaceJobAsync(job);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var job = await _PickAndPlaceJobRepo.GetPickAndPlaceJobAsync(id);
            await AuthorizeAsync(job, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(job);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeletePickAndPlaceJobAsync(string id, EntityHeader org, EntityHeader user)
        {
            var job = await _PickAndPlaceJobRepo.GetPickAndPlaceJobAsync(id);
            await ConfirmNoDepenenciesAsync(job);
            await AuthorizeAsync(job, AuthorizeActions.Delete, user, org);
            await _PickAndPlaceJobRepo.DeletePickAndPlaceJobAsync(id);
            return InvokeResult.Success;
        }

        public async Task<PickAndPlaceJob> GetPickAndPlaceJobAsync(string id, EntityHeader org, EntityHeader user)
        {
            var job = await _PickAndPlaceJobRepo.GetPickAndPlaceJobAsync(id);
            await AuthorizeAsync(job, AuthorizeActions.Read, user, org);
            return job;
        }


        public async Task<ListResponse<PickAndPlaceJobSummary>> GetPickAndPlaceJobSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(PickAndPlaceJob));
            return await _PickAndPlaceJobRepo.GetPickAndPlaceJobSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdatePickAndPlaceJobAsync(PickAndPlaceJob job, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(job, AuthorizeActions.Update, user, org);
            ValidationCheck(job, Actions.Update);
            await _PickAndPlaceJobRepo.UpdatePickAndPlaceJobAsync(job);

            return InvokeResult.Success;
        }
    }
}
