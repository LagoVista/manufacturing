using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IPickAndPlaceJobRunRepo
    {
        Task AddJobRunAsync(PickAndPlaceJobRun jobRun);
        Task DeleteJobRunAsync(string id);
        Task<PickAndPlaceJobRun> GetJobRunAsync(string id);
        Task UpdateJobRunAsync(PickAndPlaceJobRun jobRun);
        Task<ListResponse<PickAndPlaceJobRunSummary>> GetJobRuns(string orgId, string jobId, ListRequest listRequest);
    }
}
