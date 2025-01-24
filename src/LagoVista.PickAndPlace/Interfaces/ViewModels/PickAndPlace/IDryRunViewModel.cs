using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface IDryRunViewModel : IMachineViewModelBase
    {

        public IJobManagementViewModel JobVM { get; }
        RelayCommand GoToPartOnBoard { get; }
    }
}
