using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IToolHeadViewModel : IMachineViewModelBase
    {
        MachineToolHead Current { get; set; }
    }
}
