using LagoVista.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Repos
{
    public interface IManufacturingRepoSettings
    {
        IConnectionSettings ManufacturingDocDbStorage { get; }
        IConnectionSettings ManufacturingTableStorage { get; }

        bool ShouldConsolidateCollections { get; }
    }
}
