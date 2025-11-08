// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7f11659c27c83c37d7fbfe2c302db63cae7a682689ee1dcb8615079b49b7592a
// IndexVersion: 2
// --- END CODE INDEX META ---
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
