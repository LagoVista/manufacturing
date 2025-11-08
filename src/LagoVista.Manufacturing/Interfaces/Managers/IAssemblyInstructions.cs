// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 8671b75d7a59a521f45d71dcee541e81a7de09e6c27ad0d1f2dd2de2e8654ed0
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IAssemblyInstructionManager
    {
        Task<InvokeResult> AddAssemblyInstructionAsync(AssemblyInstruction assemblyInstruction, EntityHeader org, EntityHeader user);
        Task<InvokeResult> UpdateAssemblyInstructionAsync(AssemblyInstruction assemblyInstruction, EntityHeader org, EntityHeader user);
        Task<ListResponse<AssemblyInstructionSummary>> GetAssemblyInstructionsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user);
        Task<AssemblyInstruction> GetAssemblyInstructionAsync(string id, EntityHeader org, EntityHeader user);
        Task<InvokeResult> DeleteAssemblyInstructionAsync(string id, EntityHeader org, EntityHeader user);
    }
}
