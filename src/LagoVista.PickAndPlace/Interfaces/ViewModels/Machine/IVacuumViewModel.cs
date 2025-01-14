using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IVacuumViewModel : IMachineViewModelBase
    {
        List<string> GetVacuumOnGCode(MachineToolHead toolHead);
        List<string> GetVacuumOffGCode(MachineToolHead toolHead);
        Task<InvokeResult> VacuumOnAsync(MachineToolHead toolHead);
        Task<InvokeResult> VacuumOffAsync(MachineToolHead toolHead);
        Task<InvokeResult<UInt16>> ReadVacuumAsync(MachineToolHead toolHead);
    }
}
