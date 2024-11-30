using LagoVista.Core.Interfaces;
using LagoVista.Manufacturing.Interfaces.Managers;
using LagoVista.Manufacturing.Managers;

namespace LagoVista.Manufacturing
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IComponentManager, ComponentManager>();
            services.AddTransient<IFeederManager, FeederManager>();
            services.AddTransient<IStripFeederManager, StripFeederManager>();
            services.AddTransient<IComponentPackageManager, ComponentPackageManager>();
            services.AddTransient<IMachineManager, MachineManager>();
            services.AddTransient<IPickAndPlaceJobManager, PickAndPlaceJobManager>();
            services.AddTransient<IPartPackManager, PartPackManager>();
            services.AddTransient<ICircuitBoardManager, CircuitBoardManager>();
            services.AddTransient<IComponentOrderManager, ComponentOrderManager>();
        }
    }
}
