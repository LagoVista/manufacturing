using LagoVista.Core.Interfaces;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.IoT.Logging.Loggers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using System;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Core.Managers;

namespace LagoVista.Manufacturing.Managers
{
    public class CircuitBoardManager : ManagerBase, ICircuitBoardManager
    {
        private readonly ICircuitBoardRepo _CircuitBoardRepo;

        public CircuitBoardManager(ICircuitBoardRepo partRepo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _CircuitBoardRepo = partRepo;
        }
        public async Task<InvokeResult> AddCircuitBoardAsync(CircuitBoard CircuitBoard, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(CircuitBoard, AuthorizeActions.Create, user, org);
            ValidationCheck(CircuitBoard, Actions.Create);
            await _CircuitBoardRepo.AddCircuitBoardAsync(CircuitBoard);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _CircuitBoardRepo.GetCircuitBoardAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteCircuitBoardAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _CircuitBoardRepo.GetCircuitBoardAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _CircuitBoardRepo.DeleteCircuitBoardAsync(id);
            return InvokeResult.Success;
        }

        public async Task<CircuitBoard> GetCircuitBoardAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _CircuitBoardRepo.GetCircuitBoardAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }


        public async Task<ListResponse<CircuitBoardSummary>> GetCircuitBoardsSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(CircuitBoard));
            return await _CircuitBoardRepo.GetCircuitBoardSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateCircuitBoardAsync(CircuitBoard part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _CircuitBoardRepo.UpdateCircuitBoardAsync(part);

            return InvokeResult.Success;
        }
    }
}