using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IFeederRepo
    {
        Task AddFeederAsync(Feeder feeder);
        Task UpdateFeederAsync(Feeder feeder);
        Task<ListResponse<FeederSummary>> GetFeederSummariesAsync(string id, ListRequest listRequest);
        Task<Feeder> GetFeederAsync(string id);
        Task DeleteFeederAsync(string id);
    }
}
