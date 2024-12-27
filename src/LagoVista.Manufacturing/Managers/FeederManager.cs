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
    public class FeederManager : ManagerBase, IFeederManager
    {
        private readonly IFeederRepo _feederRepo;
        private readonly IComponentManager _componentManager;
        private readonly IComponentPackageRepo _packageRepo;

        public FeederManager(IFeederRepo feederRepo, IComponentManager componentManager, IComponentPackageRepo packageRepo,
            IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _feederRepo = feederRepo;
            _componentManager = componentManager ?? throw new ArgumentNullException(nameof(componentManager));
            _packageRepo = packageRepo ?? throw new ArgumentNullException(nameof(packageRepo));

        }
        public async Task<InvokeResult> AddFeederAsync(AutoFeeder feeder, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(feeder, AuthorizeActions.Create, user, org);
            ValidationCheck(feeder, Actions.Create);
            await _feederRepo.AddFeederAsync(feeder);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _feederRepo.GetFeederAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }

        public async Task<InvokeResult> DeleteFeederAsync(string id, EntityHeader org, EntityHeader user)
        {
            var feeder = await _feederRepo.GetFeederAsync(id);
            await ConfirmNoDepenenciesAsync(feeder);
            await AuthorizeAsync(feeder, AuthorizeActions.Delete, user, org);
            await _feederRepo.DeleteFeederAsync(id);
            return InvokeResult.Success;
        }

        public async Task<AutoFeeder> GetFeederAsync(string id, bool loadComponent, EntityHeader org, EntityHeader user)
        {
            var feeder = await _feederRepo.GetFeederAsync(id);
            if (!EntityHeader.IsNullOrEmpty(feeder.Component) && loadComponent)
            {
                feeder.Component.Value = await _componentManager.GetComponentAsync(feeder.Component.Id, true, org, user);
                if (!EntityHeader.IsNullOrEmpty(feeder.Component.Value.ComponentPackage))
                {
                    feeder.Component.Value.ComponentPackage.Value = await _packageRepo.GetComponentPackageAsync(feeder.Component.Value.ComponentPackage.Id);
                }
            }

            await AuthorizeAsync(feeder, AuthorizeActions.Read, user, org);
            return feeder;
        }

        public async Task<AutoFeeder> GetFeederByFeederIdAsync(string feederId, bool loadComponent, EntityHeader org, EntityHeader user)
        {
            var feeder = await _feederRepo.GetFeederByFeederIdAsync(feederId);
            if (!EntityHeader.IsNullOrEmpty(feeder.Component) && loadComponent)
            {
                feeder.Component.Value = await _componentManager.GetComponentAsync(feeder.Component.Id, true, org, user);
                if (!EntityHeader.IsNullOrEmpty(feeder.Component.Value.ComponentPackage))
                {
                    feeder.Component.Value.ComponentPackage.Value = await _packageRepo.GetComponentPackageAsync(feeder.Component.Value.ComponentPackage.Id);
                }
            }

            await AuthorizeAsync(feeder, AuthorizeActions.Read, user, org);
            return feeder;
        }

        public async Task<ListResponse<AutoFeeder>> GetFeedersForMachineAsync(string machineId, bool loadComponent, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(AutoFeeder));
            var response = await _feederRepo.GetFeedersForMachineAsync(machineId);
            if (loadComponent)
            {
                foreach (var feeder in response.Model)
                {
                    if (!EntityHeader.IsNullOrEmpty(feeder.Component))
                    {
                        feeder.Component.Value = await _componentManager.GetComponentAsync(feeder.Component.Id, true, org, user);
                        if (!EntityHeader.IsNullOrEmpty(feeder.Component.Value.ComponentPackage))
                        {
                            feeder.Component.Value.ComponentPackage.Value = await _packageRepo.GetComponentPackageAsync(feeder.Component.Value.ComponentPackage.Id);
                        }
                    }
                }
            }
            
            return response;
        }

        public async Task<ListResponse<FeederSummary>> GetFeedersSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(AutoFeeder));
            return await _feederRepo.GetFeederSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> UpdateFeederAsync(AutoFeeder feeder, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(feeder, AuthorizeActions.Update, user, org);
            ValidationCheck(feeder, Actions.Update);
            await _feederRepo.UpdateFeederAsync(feeder);

            return InvokeResult.Success;
        }
    }
}