﻿using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IAutoFeederManager
    {
        Task<InvokeResult> AddFeederAsync(AutoFeeder feeder, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateFeederAsync(AutoFeeder feeder, EntityHeader org, EntityHeader user);
        Task<ListResponse<AutoFeederSummary>> GetFeedersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<AutoFeeder> GetFeederAsync(string id, bool loadComponent, EntityHeader org, EntityHeader user);
        Task<AutoFeeder> GetFeederByFeederIdAsync(string feederId, bool loadComponent, EntityHeader org, EntityHeader user);
        Task<ListResponse<AutoFeeder>> GetFeedersForMachineAsync(string machineId, bool loadComponent, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteFeederAsync(string id, EntityHeader org, EntityHeader user);
        Task<AutoFeeder> CreateFromTemplateAsync(string templateId, EntityHeader org, EntityHeader user);

    }
}
