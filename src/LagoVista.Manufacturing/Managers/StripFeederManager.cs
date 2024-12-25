using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Threading.Tasks;

namespace LagoVista.Manufacturing.Managers
{
    public class StripFeederManager : ManagerBase, IStripFeederManager
    {
        private readonly IStripFeederRepo _stripFeederRepo;
        private readonly IComponentManager _componentManager;
        private readonly IComponentPackageRepo _packageRepo;

        public StripFeederManager(IStripFeederRepo stripFeederRepo, IComponentManager componentManager, IComponentPackageRepo packageRepo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _stripFeederRepo = stripFeederRepo ?? throw new ArgumentNullException(nameof(stripFeederRepo));
            _componentManager = componentManager ?? throw new ArgumentNullException(nameof(componentManager));
            _packageRepo = packageRepo ?? throw new ArgumentNullException(nameof(packageRepo));
        }
        public async Task<InvokeResult> AddStripFeederAsync(StripFeeder feeder, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(feeder, AuthorizeActions.Create, user, org);
            ValidationCheck(feeder, Actions.Create);
            await _stripFeederRepo.AddStripFeederAsync(feeder);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var feeder = await _stripFeederRepo.GetStripFeederAsync(id);
            await AuthorizeAsync(feeder, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(feeder);
        }

        public async Task<InvokeResult> DeleteStripFeederAsycn(string id, EntityHeader org, EntityHeader user)
        {
            var feeder = await _stripFeederRepo.GetStripFeederAsync(id);
            await AuthorizeAsync(feeder, AuthorizeActions.Delete, user, org);
            await _stripFeederRepo.DeleteStripFeederAsync(id);
            return InvokeResult.Success;
        }

        public async Task<InvokeResult> DeleteStripFeederAsync(string id, EntityHeader org, EntityHeader user)
        {
            var feeder = await _stripFeederRepo.GetStripFeederAsync(id);
            await ConfirmNoDepenenciesAsync(feeder);
            await AuthorizeAsync(feeder, AuthorizeActions.Delete, user, org);
            await _stripFeederRepo.DeleteStripFeederAsync(id);
            return InvokeResult.Success;
        }

        public async Task<StripFeeder> GetStripFeederAsync(string id, bool loadComponent, EntityHeader org, EntityHeader user)
        {
            //var feeder = await _stripFeederRepo.GetStripFeederAsync(id);
            //await AuthorizeAsync(feeder, AuthorizeActions.Read, user, org);
            //if (!EntityHeader.IsNullOrEmpty(feeder.Component) && loadComponent)
            //{
            //    feeder.Component.Value = await _componentManager.GetComponentAsync(feeder.Component.Id, true, org, user);
            //    if(!EntityHeader.IsNullOrEmpty(feeder.Component.Value.ComponentPackage))
            //    {
            //        feeder.Component.Value.ComponentPackage.Value = await _packageRepo.GetComponentPackageAsync(feeder.Component.Value.ComponentPackage.Id);
            //    }
            //}

            throw new NotImplementedException();

          //  return feeder;
        }


        public async Task<ListResponse<StripFeederSummary>> GetStripFeedersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(StripFeeder));
            return await _stripFeederRepo.GetStripFeederSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateStripFeederAsync(StripFeeder feeder, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(feeder, AuthorizeActions.Update, user, org);
            ValidationCheck(feeder, Actions.Update);
            await _stripFeederRepo.UpdateStripFeederAsync(feeder);

            return InvokeResult.Success;
        }
    }
}