// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 0a8976f74b7751783def3effb991c7e3ec2c0d8e35f8bed2778f8484617e8944
// IndexVersion: 0
// --- END CODE INDEX META ---
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
