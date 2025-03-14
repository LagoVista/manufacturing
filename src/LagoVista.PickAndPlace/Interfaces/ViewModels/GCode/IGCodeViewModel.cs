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
