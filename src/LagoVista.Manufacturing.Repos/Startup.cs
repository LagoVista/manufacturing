using LagoVista.Manufacturing.Interfaces.Repos;
using LagoVista.Manufacturing.Repo.Repos;
using LagoVista.Core.Interfaces;

namespace LagoVista.Manufacturing.Models
{
    public static class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IComponentRepo, ComponentRepo>();
            services.AddTransient<IStripFeederRepo, StripFeederRepo>();
            services.AddTransient<IComponentPackageRepo, ComponentPackageRepo>();
            services.AddTransient<IPartPackRepo, PartPackRepo>();
            services.AddTransient<IFeederRepo, FeederRepo>();
            services.AddTransient<IPickAndPlaceJobRepo, PickAndPlaceJobRepo>();
        }
    }
}
