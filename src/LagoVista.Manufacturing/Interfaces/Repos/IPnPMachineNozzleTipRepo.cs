// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8418a6435419a32db5476404c7c910481b94a81663818a9b0ce934e118db8553
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IPnpMachineNozzleTipReo
    {
        Task AddPnPMachineNozzleTipAsync(PnPMachineNozzleTip feeder);
        Task UpdatePnPMachineNozzleTipAsync(PnPMachineNozzleTip feeder);
        Task<ListResponse<PnPMachineNozzleTipSummary>> GetPnPMachineNozzleTipSummariesAsync(string id, ListRequest listRequest);
        Task<PnPMachineNozzleTip> GetPnPMachineNozzleTipAsync(string id);
        Task DeletePnPMachineNozzleTipAsync(string id);
    }
}
