// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d5714d2161b1860de611463869e663bd9dfa0412c2575d8c7ab779774d6a1fc8
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
    public interface IPickAndPlaceJobRunRepo
    {
        Task AddJobRunAsync(PickAndPlaceJobRun jobRun);
        Task DeleteJobRunAsync(string id);
        Task<PickAndPlaceJobRun> GetJobRunAsync(string id);
        Task UpdateJobRunAsync(PickAndPlaceJobRun jobRun);
        Task<ListResponse<PickAndPlaceJobRunSummary>> GetJobRuns(string orgId, string jobId, ListRequest listRequest);
    }
}
