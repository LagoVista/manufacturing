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
    public class ComponentOrderManager : ManagerBase, IComponentOrderManager
    {
        private readonly IComponentOrderRepo _ComponentOrderRepo;

        public ComponentOrderManager(IComponentOrderRepo partRepo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _ComponentOrderRepo = partRepo;
        }
        public async Task<InvokeResult> AddComponentOrderAsync(ComponentOrder order, EntityHeader org, EntityHeader user)
        {
            if(!String.IsNullOrEmpty(order.LineItemsCSV))
            {
                var lines = order.LineItemsCSV.Split('\n');
                var skippedHeader = false;
                foreach(var line in lines)
                {
                    if (!skippedHeader)
                        skippedHeader = true;
                    else
                    {
                        var parts = line.Split(',');
                        order.LineItems.Add(ComponentOrderLineItem.FromOrderLine(line.Split(',')));
                    }
                }
            }

            await AuthorizeAsync(order, AuthorizeActions.Create, user, org);
            ValidationCheck(order, Actions.Create);
            await _ComponentOrderRepo.AddComponentOrderAsync(order);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _ComponentOrderRepo.GetComponentOrderAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public Task<InvokeResult> DeleteCommponentAsync(string id, EntityHeader org, EntityHeader user)
        {
            throw new NotImplementedException();
        }

        public async Task<InvokeResult> DeleteComponentOrderAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _ComponentOrderRepo.GetComponentOrderAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _ComponentOrderRepo.DeleteComponentOrderAsync(id);
            return InvokeResult.Success;
        }

        public async Task<ComponentOrder> GetComponentOrderAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _ComponentOrderRepo.GetComponentOrderAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }

        public async Task<ListResponse<ComponentOrderSummary>> GetComponentOrdersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(ComponentOrder));
            return await _ComponentOrderRepo.GetComponentOrderSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateComponentOrderAsync(ComponentOrder order, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(order, AuthorizeActions.Update, user, org);
            ValidationCheck(order, Actions.Update);
            await _ComponentOrderRepo.UpdateComponentOrderAsync(order);

            return InvokeResult.Success;
        }
    }
}
