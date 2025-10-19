// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 638cc4bbe9f9ffdaa25f0b7b64e0b3d9b35bee514f7e6f5fe1b86b86a4001c3c
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IPnPMachineNozzleTipManager
    {

        Task<InvokeResult> AddPnPMachineNozzleTipAsync(PnPMachineNozzleTip feeder, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdatePnPMachineNozzleTipAsync(PnPMachineNozzleTip feeder, EntityHeader org, EntityHeader user);
        Task<ListResponse<PnPMachineNozzleTipSummary>> GetPnPMachineNozzleTipsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<PnPMachineNozzleTip> GetPnPMachineNozzleTipAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user);
    }
}
