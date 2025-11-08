// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f1f30817b4ba13ae5acc8c99aed3d2c41fd11770457b26360c81aae821ce95ef
// IndexVersion: 2
// --- END CODE INDEX META ---
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
    internal class AutoFeederTemplateRepo : DocumentDBRepoBase<AutoFeederTemplate>, IAutoFeederTemplateRepo
    {
        private bool _shouldConsolidateCollections;

        public AutoFeederTemplateRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddAutoFeederTemplateAsync(AutoFeederTemplate autoFeederTemplate)
        {
            return CreateDocumentAsync(autoFeederTemplate);
        }

        public Task DeleteAutoFeederTemplateAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<AutoFeederTemplate> GetAutoFeederTemplateAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<AutoFeederTemplateSummary>> GetAutoFeederTemplateSummariesAsync(string orgId, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<AutoFeederTemplateSummary, AutoFeederTemplate>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task UpdateAutoFeederTemplateAsync(AutoFeederTemplate autoFeederTemplate)
        {
            return UpsertDocumentAsync(autoFeederTemplate);
        }

    }
}
