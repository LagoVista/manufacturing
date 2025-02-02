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
using System.Linq;
using System.Drawing.Imaging;

namespace LagoVista.Manufacturing.Managers
{
    public class PickAndPlaceJobManager : ManagerBase, IPickAndPlaceJobManager
    {
        private readonly IPickAndPlaceJobRepo _pickAndPlaceJobRepo;
        private readonly IPickAndPlaceJobRunRepo _jobRunRepo;

        public PickAndPlaceJobManager(IPickAndPlaceJobRepo partRepo, IPickAndPlaceJobRunRepo jobRunRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _pickAndPlaceJobRepo = partRepo ?? throw new ArgumentNullException(nameof(partRepo));
            _jobRunRepo = jobRunRepo ?? throw new ArgumentNullException(nameof(jobRunRepo));
        }

        public async Task<InvokeResult> AddPickAndPlaceJobAsync(PickAndPlaceJob job, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(job, AuthorizeActions.Create, user, org);
            ValidationCheck(job, Actions.Create);
            await _pickAndPlaceJobRepo.AddPickAndPlaceJobAsync(job);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var job = await _pickAndPlaceJobRepo.GetPickAndPlaceJobAsync(id);
            await AuthorizeAsync(job, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(job);
        }

        public async Task<InvokeResult> DeletePickAndPlaceJobAsync(string id, EntityHeader org, EntityHeader user)
        {
            var job = await _pickAndPlaceJobRepo.GetPickAndPlaceJobAsync(id);
            await ConfirmNoDepenenciesAsync(job);
            await AuthorizeAsync(job, AuthorizeActions.Delete, user, org);
            await _pickAndPlaceJobRepo.DeletePickAndPlaceJobAsync(id);
            return InvokeResult.Success;
        }

        public async Task<PickAndPlaceJob> GetPickAndPlaceJobAsync(string id, EntityHeader org, EntityHeader user)
        {
            var job = await _pickAndPlaceJobRepo.GetPickAndPlaceJobAsync(id);
            await AuthorizeAsync(job, AuthorizeActions.Read, user, org);
            return job;
        }

        public async Task<ListResponse<PickAndPlaceJobSummary>> GetPickAndPlaceJobSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(PickAndPlaceJob));
            return await _pickAndPlaceJobRepo.GetPickAndPlaceJobSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdatePickAndPlaceJobAsync(PickAndPlaceJob job, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(job, AuthorizeActions.Update, user, org);
            ValidationCheck(job, Actions.Update);
            await _pickAndPlaceJobRepo.UpdatePickAndPlaceJobAsync(job);

            return InvokeResult.Success;
        }

        public async Task<InvokeResult> AddPickAndPlaceJobRunAsync(PickAndPlaceJobRun jobRun, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(jobRun, AuthorizeActions.Create, user, org);
            ValidationCheck(jobRun, Actions.Create);
            await _jobRunRepo.AddJobRunAsync(jobRun);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> UpdatePickAndPlaceJobRunAsync(PickAndPlaceJobRun jobRun, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(jobRun, AuthorizeActions.Create, user, org);
            ValidationCheck(jobRun, Actions.Update);
            await _jobRunRepo.UpdateJobRunAsync(jobRun);
            return InvokeResult.Success;
        }

        public async Task<PickAndPlaceJobRun> GetPickAndPlaceJobRunAsync(string jobRunId, EntityHeader org, EntityHeader user)
        {
            var jobRun = await _jobRunRepo.GetJobRunAsync(jobRunId);
            await AuthorizeAsync(jobRun, AuthorizeActions.Read, user, org);
            return jobRun;
        }

        public async Task<ListResponse<PickAndPlaceJobRunSummary>> GetPickAndPlaceJobRunsAsync(string jobId, ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(PickAndPlaceJobRun));
            return await _jobRunRepo.GetJobRuns(jobId, org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdatePickAndPlaceJobRunPlacementAsync(string id, PickAndPlaceJobRunPlacement placement, EntityHeader org, EntityHeader user)
        {
            var job = await GetPickAndPlaceJobRunAsync(id, org, user);
            var existing = job.Placements.SingleOrDefault(plc => plc.Id == placement.Id);
            if(existing != null)
            {
                job.Placements.Remove(existing);
            }

            job.Placements.Add(placement);
            return await UpdatePickAndPlaceJobRunAsync(job, org, user);
        }
    }
}
