// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 314fcc797670aabfd05feb4832b7e68b3a1013c3643ba3c9408fb24cb3d6483b
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Repos;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class ComponentRepo : DocumentDBRepoBase<Component>, IComponentRepo
    {
        private bool _shouldConsolidateCollections;

        public ComponentRepo(IManufacturingRepoSettings settings, IAdminLogger logger, ICacheProvider cacheProvider, IDependencyManager dependencyMgr) :
            base(settings.ManufacturingDocDbStorage.Uri, settings.ManufacturingDocDbStorage.AccessKey, settings.ManufacturingDocDbStorage.ResourceName, logger, cacheProvider, dependencyMgr)
        {
            _shouldConsolidateCollections = settings.ShouldConsolidateCollections;
        }

        protected override bool ShouldConsolidateCollections => _shouldConsolidateCollections;

        public Task AddComponentAsync(Component component)
        {
            return CreateDocumentAsync(component);
        }

        public Task DeleteComponentAsync(string id)
        {
            return DeleteDocumentAsync(id);
        }

        public Task<Component> GetComponentAsync(string id)
        {
            return GetDocumentAsync(id);
        }

        public Task<ListResponse<ComponentSummary>> GetComponentSummariesAsync(string orgId, ListRequest listRequest)
        { 
            return base.QuerySummaryAsync<ComponentSummary, Component>(qry => qry.IsPublic == true || qry.OwnerOrganization.Id == orgId, itm => itm.Name, listRequest);
        }

        public Task<ListResponse<ComponentSummary>> GetComponentSummariesByTypeAsync(string orgId, string componentType, ListRequest listRequest)
        {
            return base.QuerySummaryAsync<ComponentSummary, Component>(qry => (qry.IsPublic == true || qry.OwnerOrganization.Id == orgId) && qry.ComponentType.Key == componentType, itm => itm.Name, listRequest);
        }

        public Task UpdateComponentAsync(Component component)
        {
            if (component.ComponentPackage != null)
                component.ComponentPackage.Value = null;

            return UpsertDocumentAsync(component);
        }
    }
}
