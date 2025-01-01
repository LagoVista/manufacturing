using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IAutoFeederTemplateRepo
    {
        Task AddFeederAsync(AutoFeederTemplate feeder);
        Task UpdateFeederAsync(AutoFeederTemplate feeder);
        Task<ListResponse<AutoFeederTemplateSummary>> GetFeederSummariesAsync(string id, ListRequest listRequest);
        Task<AutoFeederTemplate> GetFeederAsync(string id);
        Task DeleteFeederAsync(string id);
    }
}
