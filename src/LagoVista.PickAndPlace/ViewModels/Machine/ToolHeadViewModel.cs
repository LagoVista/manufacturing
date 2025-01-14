using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class ToolHeadViewModel : MachineViewModelBase, IToolHeadViewmodel
    {
        public ToolHeadViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {
        }

        MachineToolHead _current;
        public MachineToolHead Current
        {
            get => _current;
            set => Set(ref _current, value);
        }
    }
}
