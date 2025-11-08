// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f9d8e9fea1e26eecd25fc151e82d1bef68f653fcdc6a31e54be998f40491efdc
// IndexVersion: 2
// --- END CODE INDEX META ---
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
