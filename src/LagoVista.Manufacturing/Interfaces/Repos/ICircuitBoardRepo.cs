// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 913b304052606dded6560c716ed32d3222fe550c0ae333b165584d836753639b
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface ICircuitBoardRepo
    {
        Task AddCircuitBoardAsync(CircuitBoard circuitBoard);
        Task UpdateCircuitBoardAsync(CircuitBoard circuitBoard);
        Task<ListResponse<CircuitBoardSummary>> GetCircuitBoardSummariesAsync(string id, ListRequest listRequest);
        Task<CircuitBoard> GetCircuitBoardAsync(string id);
        Task DeleteCircuitBoardAsync(string id);
    }
}
