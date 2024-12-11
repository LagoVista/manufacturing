using LagoVista.Core.Interfaces;

namespace LagoVista.Manufacturing.Repos
{
    public interface IManufacturingRepoSettings
    {
        IConnectionSettings ManufacturingDocDbStorage { get; }
        IConnectionSettings ManufacturingTableStorage { get; }

        bool ShouldConsolidateCollections { get; }
    }
}
