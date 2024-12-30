using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface IHomingViewModel
    {
        string Status { get; }
        Task MachineHomeAsync();

        Task WorkSpaceHomeAsync();

    }
}
