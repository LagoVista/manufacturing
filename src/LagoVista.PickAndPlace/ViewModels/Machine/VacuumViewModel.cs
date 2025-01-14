using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class VacuumViewModel : MachineViewModelBase, IVacuumViewModel
    {
        public VacuumViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
        }

        public List<string> GetVacuumOffGCode(MachineToolHead toolHead)
        {
            throw new NotImplementedException();
        }

        public List<string> GetVacuumOnGCode(MachineToolHead toolHead)
        {
            throw new NotImplementedException();
        }

        public Task<InvokeResult<ushort>> ReadVacuumAsync(MachineToolHead toolHead)
        {
            throw new NotImplementedException();
        }

        public Task<InvokeResult> VacuumOffAsync(MachineToolHead toolHead)
        {
            throw new NotImplementedException();
        }

        public Task<InvokeResult> VacuumOnAsync(MachineToolHead toolHead)
        {
            throw new NotImplementedException();
        }
    }
}
