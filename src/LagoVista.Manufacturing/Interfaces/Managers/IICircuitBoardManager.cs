// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: d61c5115160ccde268d881254a5b9335887ca2f37a7727643decb48ea9665114
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface ICircuitBoardManager
    {
        Task<InvokeResult> AddCircuitBoardAsync(CircuitBoard circuitBoard, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateCircuitBoardAsync(CircuitBoard circuitBoard, EntityHeader org, EntityHeader user);
        Task<ListResponse<CircuitBoardSummary>> GetCircuitBoardsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<CircuitBoard> GetCircuitBoardAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user);
    }
}
