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
