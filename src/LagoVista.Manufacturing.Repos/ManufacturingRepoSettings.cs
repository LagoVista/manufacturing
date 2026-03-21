using LagoVista.Core.Interfaces;
using Microsoft.Extensions.Configuration;

namespace LagoVista.Manufacturing.Repos
{
    public class ManufacturingRepoSettings : IManufacturingRepoSettings
    {
        public IConnectionSettings ManufacturingDocDbStorage { get; }

        public IConnectionSettings ManufacturingTableStorage { get; }
    
        public ManufacturingRepoSettings(IConfiguration configuration)
        {
            ManufacturingDocDbStorage = configuration.CreateDefaultDBStorageSettings();
            ManufacturingTableStorage = configuration.CreateDefaultTableStorageSettings();
        }
    }
}
