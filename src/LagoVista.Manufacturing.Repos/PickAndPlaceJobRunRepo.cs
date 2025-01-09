using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Repos
{
    public class PickAndPlaceJobRunRepo : DocumentDBRepoBase<PickAndPlaceJobRun>, IPickAndPlaceJobRunRepo
    {
        private bool _shouldConsolidateCollections;

        public PickAndPlaceJobRunRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddJobRunAsync(PickAndPlaceJobRun jobRun)
        {
            return CreateDocumentAsync(jobRun);
        }

        public Task DeleteJobRunAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<PickAndPlaceJobRun> GetJobRunAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task UpdateJobRunAsync(PickAndPlaceJobRun jobRun)
        {
            return UpsertDocumentAsync(jobRun);
        }

        public Task<ListResponse<PickAndPlaceJobRunSummary>> GetJobRuns(string orgId, string jobId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<PickAndPlaceJobRunSummary, PickAndPlaceJobRun>(qry => qry.Job.Id == jobId && qry.OwnerOrganization.Id == orgId,
                itm => itm.Name, listRequest);
        }
    }
}
