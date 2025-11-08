// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5688644e772d935f761e26e9a03c4ec0538541596b463942df739235cd9f0a96
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Managers
{
    public class AssemblyInstructionManager : ManagerBase, IAssemblyInstructionManager
    {
        private readonly IAssemblyInstructionRepo _assemblyInstructionRepo;

        public AssemblyInstructionManager(IAssemblyInstructionRepo AssemblyInstructionRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _assemblyInstructionRepo = AssemblyInstructionRepo;
        }
        public async Task<InvokeResult> AddAssemblyInstructionAsync(AssemblyInstruction instructions, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(instructions, AuthorizeActions.Create, user, org);
            ValidationCheck(instructions, Actions.Create);
            await _assemblyInstructionRepo.AddAssemblyInstructionAsync(instructions);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var millingProject = await _assemblyInstructionRepo.GetAssemblyInstructionAsync(id);
            await AuthorizeAsync(millingProject, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(millingProject);
        }

        public async Task<InvokeResult> DeleteAssemblyInstructionAsync(string id, EntityHeader org, EntityHeader user)
        {
            var millingProject = await _assemblyInstructionRepo.GetAssemblyInstructionAsync(id);
            await ConfirmNoDepenenciesAsync(millingProject);
            await AuthorizeAsync(millingProject, AuthorizeActions.Delete, user, org);
            await _assemblyInstructionRepo.DeleteAssemblyInstructionAsync(id);
            return InvokeResult.Success;
        }

        public async Task<AssemblyInstruction> GetAssemblyInstructionAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _assemblyInstructionRepo.GetAssemblyInstructionAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<AssemblyInstructionSummary>> GetAssemblyInstructionsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(AssemblyInstruction));
            return await _assemblyInstructionRepo.GetAssemblyInstructionSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateAssemblyInstructionAsync(AssemblyInstruction instructions, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(instructions, AuthorizeActions.Update, user, org);
            ValidationCheck(instructions, Actions.Update);
            await _assemblyInstructionRepo.UpdateAssemblyInstructionAsync(instructions);

            return InvokeResult.Success;
        }
    }
}