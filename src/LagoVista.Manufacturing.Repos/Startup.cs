using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Repo.Repos;
using LagoVista.Core.Interfaces;

namespace LagoVista.Manufacturing.Repos
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IComponentRepo, ComponentRepo>();
            services.AddTransient<IStripFeederRepo, StripFeederRepo>();
            services.AddTransient<IComponentPackageRepo, ComponentPackageRepo>();
            services.AddTransient<IPartPackRepo, PartPackRepo>();
            services.AddTransient<IAutoFeederRepo, AutoFeederRepo>();
            services.AddTransient<IMachineRepo, MachineRepo>();
            services.AddTransient<IPickAndPlaceJobRepo, PickAndPlaceJobRepo>();
            services.AddTransient<ICircuitBoardRepo, CircuitBoardRepo>();
            services.AddTransient<IPnpMachineNozzleTipReo, PnPMachineNozzleTipRepo>();
            services.AddTransient<IComponentOrderRepo, ComponentOrderRepo>();
            services.AddTransient<IGCodeMappingRepo, GCodeMappingRepo>();
            services.AddTransient<IStripFeederTemplateRepo, StripFeederTemplateRepo>();
            services.AddTransient<IAutoFeederTemplateRepo, AutoFeederTemplateRepo>();
            services.AddTransient<IInventoryLocationRepo, InventoryLocationRepo>();
            services.AddTransient<IPickAndPlaceJobRunRepo, PickAndPlaceJobRunRepo>();
        }
    }
}
