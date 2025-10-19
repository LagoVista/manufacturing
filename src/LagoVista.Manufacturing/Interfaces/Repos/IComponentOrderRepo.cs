// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c22a60d37e2cc618b230314ac3d74112713d829fff8aeb04dc87043579eef86a
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IComponentOrderRepo
    {
        Task AddComponentOrderAsync(ComponentOrder componentOrder);
        Task UpdateComponentOrderAsync(ComponentOrder componentOrder);
        Task<ListResponse<ComponentOrderSummary>> GetComponentOrderSummariesAsync(string id, ListRequest listRequest);
        Task<ComponentOrder> GetComponentOrderAsync(string id);
        Task DeleteComponentOrderAsync(string id);
    }
}
