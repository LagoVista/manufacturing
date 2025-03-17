using LagoVista.Core.Interfaces;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Interfaces.Services;
using LagoVista.Manufacturing.Managers;
using LagoVista.Manufacturing.Services;

namespace LagoVista.Manufacturing
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IComponentManager, ComponentManager>();
            services.AddTransient<IAutoFeederManager, AutoFeederManager>();
            services.AddTransient<IStripFeederManager, StripFeederManager>();
            services.AddTransient<IComponentPackageManager, ComponentPackageManager>();
            services.AddTransient<IMachineManager, MachineManager>();
            services.AddTransient<IPickAndPlaceJobManager, PickAndPlaceJobManager>();
            services.AddTransient<IPartPackManager, PartPackManager>();
            services.AddTransient<ICircuitBoardManager, CircuitBoardManager>();
            services.AddTransient<IComponentOrderManager, ComponentOrderManager>();
            services.AddTransient<IGCodeMappingManager, GCodeMappingManager>();
            services.AddTransient<IDigiKeyLookupService, DigiKeyLookupService>();
            services.AddTransient<IPnPMachineNozzleTipManager, PnPMachineNozzleManager>();
            services.AddTransient<IStripFeederTemplateManager, StripFeederTemplateManager>();
            services.AddTransient<IAutoFeederTemplateManager, AutoFeederTemplateManager>();
            services.AddTransient<IInventoryLocationManager, InventoryLocationManager>();
            services.AddTransient<IGCodeProjectManager, GCodeProjectManager>();
            services.AddTransient<IPcbMillingProjectManager, PcbMillingProjectManager>();
            services.AddTransient<IGCodeBuilder, GCodeBuilder>();
            services.AddTransient<IAssemblyInstructionManager, AssemblyInstructionManager>();
        }
    }
}
