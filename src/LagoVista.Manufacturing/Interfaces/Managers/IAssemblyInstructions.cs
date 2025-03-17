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
