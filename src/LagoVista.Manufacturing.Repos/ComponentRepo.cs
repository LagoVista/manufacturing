// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 314fcc797670aabfd05feb4832b7e68b3a1013c3643ba3c9408fb24cb3d6483b
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.CloudStorage.DocumentDB;
using LagoVista.CloudStorage.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Repo.Repos
{
    public class ComponentRepo : DocumentDBRepoBase<Component>, IComponentRepo
    {
        public ComponentRepo(IDocumentCloudCachedServices services) :
            base(services)
        {
        }
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
            return SearchComponentSummariesAsync(orgId, listRequest, null, null, null, null, false, true);
        }

        public Task<ListResponse<ComponentSummary>> GetComponentSummariesByTypeAsync(string orgId, string componentType, ListRequest listRequest)
        {
            return SearchComponentSummariesAsync(orgId, listRequest, componentType, null, null, null, false, true);
        }

        public Task<ListResponse<ComponentSummary>> SearchComponentSummariesAsync(string orgId, ListRequest listRequest,
            string componentType, string componentPackageId, string value, string searchText, bool inStockOnly, bool includePublic)
        {
            var typeKey = string.IsNullOrWhiteSpace(componentType) ? null : componentType.Trim();
            var packageId = string.IsNullOrWhiteSpace(componentPackageId) ? null : componentPackageId.Trim();
            var nominalValue = string.IsNullOrWhiteSpace(value) ? null : value.Trim().ToLowerInvariant();
            var term = string.IsNullOrWhiteSpace(searchText) ? null : searchText.Trim().ToLowerInvariant();

            // Filtering is applied by the storage provider before paging. Public catalog parts
            // are not necessarily physically owned inventory; callers can set includePublic=false.
            return base.QuerySummaryAsync<ComponentSummary, Component>(
                qry => (qry.OwnerOrganization.Id == orgId || (includePublic && qry.IsPublic == true))
                    && (typeKey == null || (qry.ComponentType != null && qry.ComponentType.Key == typeKey))
                    && (packageId == null || (qry.ComponentPackage != null && qry.ComponentPackage.Id == packageId))
                    && (nominalValue == null || (qry.Value != null && qry.Value.ToLower() == nominalValue))
                    && (!inStockOnly || qry.QuantityOnHand > 0)
                    && (term == null
                        || (qry.Name != null && qry.Name.ToLower().Contains(term))
                        || (qry.Value != null && qry.Value.ToLower().Contains(term))
                        || (qry.PartNumber != null && qry.PartNumber.ToLower().Contains(term))
                        || (qry.ManufacturePartNumber != null && qry.ManufacturePartNumber.ToLower().Contains(term))
                        || (qry.SupplierPartNumber != null && qry.SupplierPartNumber.ToLower().Contains(term))
                        || (qry.Attr1 != null && qry.Attr1.ToLower().Contains(term))
                        || (qry.Attr2 != null && qry.Attr2.ToLower().Contains(term))),
                itm => itm.Name, listRequest);
        }

        public Task UpdateComponentAsync(Component component)
        {
            if (component.ComponentPackage != null)
                component.ComponentPackage.Value = null;

            return UpsertDocumentAsync(component);
        }
    }
}
