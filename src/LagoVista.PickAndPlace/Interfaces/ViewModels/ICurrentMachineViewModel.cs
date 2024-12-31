﻿using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface ICurrentMachineViewModel : IViewModel
    {
        IMachine Machine { get; }
        RelayCommand SaveCommand { get;}
    }
}
