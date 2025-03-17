using LagoVista.Core.Models.UIMetaData;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Repos
{
    public interface IAssemblyInstructionRepo
    {
        Task AddAssemblyInstructionAsync(AssemblyInstruction assemblyInstruction);
        Task UpdateAssemblyInstructionAsync(AssemblyInstruction assemblyInstruction);
        Task<ListResponse<AssemblyInstructionSummary>> GetAssemblyInstructionSummariesAsync(string id, ListRequest listRequest);
        Task<AssemblyInstruction> GetAssemblyInstructionAsync(string id);
        Task DeleteAssemblyInstructionAsync(string id);
    }
}
