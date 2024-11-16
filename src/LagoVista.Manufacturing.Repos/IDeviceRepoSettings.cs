using LagoVista.Core.Interfaces;

namespace LagoVista.Manufacturing.CloudRepos
{
    public interface IDeviceRepoSettings
    {
        IConnectionSettings DeviceDocDbStorage { get; set; }
        IConnectionSettings DeviceTableStorage { get; set; }

        bool ShouldConsolidateCollections { get; }
    }
}
