﻿using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Repos;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class PartPackRepo : DocumentDBRepoBase<PartPack>, IPartPackRepo
    {
        private bool _shouldConsolidateCollections;

        public PartPackRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddPartPackAsync(PartPack PartPack)
        {
            return CreateDocumentAsync(PartPack);
        }

        public Task DeletePartPackAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<PartPack> GetPartPackAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<PartPackSummary>> GetPartPackSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<PartPackSummary, PartPack>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdatePartPackAsync(PartPack PartPack)
        {
            return UpsertDocumentAsync(PartPack);
        }
    }
}
