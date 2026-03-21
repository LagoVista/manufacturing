// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: eb4ffc86817dc042df642b417f6b52f0bec4505df5aee429c63ef8fdb841ede5
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Interfaces;
using LagoVista.Core.PlatformSupport;
using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Repo.Repos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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
            services.AddTransient<IPcbMillingProjectRepo, PcbMillingProjectRepo>();
            services.AddTransient<IGCodeProjectRepo, GCodeProjectRepo>();
            services.AddTransient<IAssemblyInstructionRepo, AssemblyInstructionRepo>();
            services.AddSingleton<IManufacturingRepoSettings, ManufacturingRepoSettings>();
        }
    }
}


namespace LagoVista.DependencyInjection
{
    public static class ManufacturingModule
    {
        public static void AddManufacturingModule(this IServiceCollection services, IConfigurationRoot configRoot, ILogger logger)
        {
            LagoVista.Manufacturing.Repos.Startup.ConfigureServices(services);
            LagoVista.Manufacturing.Startup.ConfigureServices(services);
            services.AddMetaDataHelper<CircuitBoard>();
        }
    }
}