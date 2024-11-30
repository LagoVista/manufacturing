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
