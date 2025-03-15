using LagoVista.Core.Commanding;
using LagoVista.PickAndPlace.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IErrorsViewModel : IMachineViewModelBase
    {
        ObservableCollection<StatusMessage> Errors { get; }

        RelayCommand<StatusMessage> ClearCommand { get; }

        void ClearAll();
    }
}
