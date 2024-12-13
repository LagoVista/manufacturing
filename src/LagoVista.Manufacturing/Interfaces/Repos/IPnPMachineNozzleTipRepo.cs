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
