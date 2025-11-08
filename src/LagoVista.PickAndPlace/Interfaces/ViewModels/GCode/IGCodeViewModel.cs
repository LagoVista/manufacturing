// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f3196a2acdb53eb62001e0b0ad622e9fb9a4113336a11e0b8b3d8d6a6c1d5371
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PcbFab;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.GCode
{
    public interface IGCodeViewModel : IMachineViewModelBase
    {
        IHeightMapManager HeightMapManager { get; }

        Task AddProjectFileMRUAsync(string projectFile);
        Task<bool> OpenProjectAsync(string projectFile);
    }
}
