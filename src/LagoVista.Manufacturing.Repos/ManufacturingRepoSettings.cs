using LagoVista.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Repos
{
    public interface IManufacturingRepoSettings
    {
        IConnectionSettings ManufacturingDocDbStorage { get; set; }
        IConnectionSettings ManufacturingTableStorage { get; set; }

        bool ShouldConsolidateCollections { get; }
    }
}
