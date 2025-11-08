// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 126e61b72f52e43ea8eacf6403d007edd0f1ef7c332ec7b9e8119296745c76e1
// IndexVersion: 2
// --- END CODE INDEX META ---
using Emgu.Util;
using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IMachineRepo : IViewModel, INotifyPropertyChanged
    {
        event EventHandler<IMachine> MachineChanged;

        IMachine CurrentMachine { get; }
        Task<List<MachineSummary>> GetMachinesAsync();
        Task<InvokeResult> LoadCurrentMachineAsync();
        Task<InvokeResult> LoadMachineAsync(string id);
        Task<InvokeResult> SaveCurrentMachineAsync();
        bool HasValidMachine { get; }

        MachineSummary SelectedMachine { get; set; }
        RelayCommand LockSettingsCommand { get;  }
        RelayCommand UnlockSettingsCommand { get; }

        RelayCommand ClearWarningsCommand { get; }
    }
}
