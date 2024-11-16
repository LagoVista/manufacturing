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
