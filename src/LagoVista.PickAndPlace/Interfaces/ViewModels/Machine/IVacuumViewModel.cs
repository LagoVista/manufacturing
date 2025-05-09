﻿using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IVacuumViewModel : IMachineViewModelBase
    {
        Task<InvokeResult<long>> ReadVacuumAsync();

        Task<InvokeResult> CheckPartPresent(Component component, int timeoutMS, ulong? vacuumOverride);

        Task<InvokeResult> CheckNoPartPresent(Component component, int timeoutMS);

        long? Vacuum { get;  }
        double? AboveThreshold { get; }
        double? Threshold { get; }
        double? PercentAboveThreshold { get; }

        Component Component { get; }
        MachineToolHead ToolHead { get; }

        RelayCommand ReadVacuumCommand { get; }
        RelayCommand VacuumOnCommand { get; }
        RelayCommand VacuumOffCommand { get; }
    }
}
