using LagoVista.Core.Interfaces;
using LagoVista.Core.Managers;
using LagoVista.Core.Models;
using LagoVista.Core.Models.UIMetaData;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.IoT.Logging.Loggers;
using System;
using System.Threading.Tasks;
using static LagoVista.Core.Models.AuthorizeResult;
using System.Xml.Linq;
using System.Collections.Generic;
using LagoVista.PCB.Eagle.Models;
using System.Linq;

namespace LagoVista.Manufacturing.Managers
{
    public class ComponentPackageManager : ManagerBase, IComponentPackageManager
    {
        private readonly IComponentPackageRepo _packageRepo;

        public ComponentPackageManager(IComponentPackageRepo pacakgeRepo, IAdminLogger logger, IAppConfig appConfig, IDependencyManager depmanager, ISecurity security) :
            base(logger, appConfig, depmanager, security)
        {
            _packageRepo = pacakgeRepo;
        }
        public async Task<InvokeResult> AddComponentPackageAsync(ComponentPackage part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Create, user, org);
            ValidationCheck(part, Actions.Create);
            await _packageRepo.AddComponentPackageAsync(part);

            return InvokeResult.Success;
        }

        public async Task<DependentObjectCheckResult> CheckInUseAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _packageRepo.GetComponentPackageAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return await base.CheckForDepenenciesAsync(part);
        }
       
        public async Task<InvokeResult> DeleteCommponentPackageAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _packageRepo.GetComponentPackageAsync(id);
            await ConfirmNoDepenenciesAsync(part);
            await AuthorizeAsync(part, AuthorizeActions.Delete, user, org);
            await _packageRepo.DeleteCommponentPackageAsync(id);
            return InvokeResult.Success;
        }

        public async Task<XDocument> GenerateOpenPnPPackagesForAllComponentPackagesAsync(EntityHeader org, EntityHeader user)
        {
            var openPnPPackages = new OpenPnPPackages();
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(ComponentPackage));
            var packages = await _packageRepo.GetFullPackagesAsync(org.Id);
            foreach(var package in packages)
            {
               openPnPPackages.Packages.Add( OpenPnPPackage.Create(package));
            }


            throw new NotImplementedException();
        }

        public async Task<ComponentPackage> GetComponentPackageAsync(string id, EntityHeader org, EntityHeader user)
        {
            var part = await _packageRepo.GetComponentPackageAsync(id);
            await AuthorizeAsync(part, AuthorizeActions.Read, user, org);
            return part;
        }

        public async Task<ListResponse<ComponentPackageSummary>> GetComponentPackagesSummariesAsync(ListRequest listRequest, EntityHeader org, EntityHeader user)
        {
            await AuthorizeOrgAccessAsync(user, org.Id, typeof(ComponentPackage));
            return await _packageRepo.GetComponentPackagesSummariesAsync(org.Id, listRequest);
        }

        public async Task<InvokeResult> SetLayoutAsync(string componentId, PcbPackage layout, EntityHeader org, EntityHeader user)
        {
            var package = await GetComponentPackageAsync(componentId, org, user);
            package.Layout = layout;
            return await UpdateComponentPackageAsync(package, org, user);
        }

        public async Task<InvokeResult> UpdateComponentPackageAsync(ComponentPackage part, EntityHeader org, EntityHeader user)
        {
            await AuthorizeAsync(part, AuthorizeActions.Update, user, org);
            ValidationCheck(part, Actions.Update);
            await _packageRepo.UpdateComponentPackageAsync(part);

            return InvokeResult.Success;
        }
    }
}
