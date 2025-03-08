using LagoVista.Manufacturing.Models;
using LagoVista.PCB.Eagle.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.GCode
{
    public interface IGCodeViewModel : IMachineViewModelBase
    {
        PcbMillingProject Project { get; set; }

        Task AddProjectFileMRUAsync(string projectFile);
        Task<bool> OpenProjectAsync(string projectFile);
    }
}
