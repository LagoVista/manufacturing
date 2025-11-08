// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 4e1e743c375b681172b8e5d0581642612023b7557b14b48472e33a11f9b97564
// IndexVersion: 2
// --- END CODE INDEX META ---
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
