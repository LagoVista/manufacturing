using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IHomingViewModel : IViewModel
    {
        string Status { get; }
        Task MachineHomeAsync();

        Task WorkSpaceHomeAsync();

    }
}
