using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.CloudRepos;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class MachineRepo : DocumentDBRepoBase<Machine>, IMachineRepo
    {
        private bool _shouldConsolidateCollections;

        public MachineRepo(IDeviceRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.DeviceDocDbStorage.Uri, settings.DeviceDocDbStorage.AccessKey, settings.DeviceDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddMachineAsync(Machine Machine)
        {
            return CreateDocumentAsync(Machine);
        }

        public Task DeleteMachineAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<Machine> GetMachineAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<MachineSummary>> GetMachineSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<MachineSummary, Machine>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateMachineAsync(Machine Machine)
        {
            return UpsertDocumentAsync(Machine);
        }

    }
}
