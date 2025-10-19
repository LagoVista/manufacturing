// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7809149639bb7854bbcb6250789e0127e157ba99caedb813be1ec33ee197553b
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public partial class SettingsViewModel
    {
        private void InitComamnds()
        {
            CancelCommand = new RelayCommand(Cancel);
        }

        public bool CanChangeMachineConfig
        {
            get { return !_machineRepo.CurrentMachine.Connected; }
        }


        public RelayCommand SaveCommand { get; private set; }
        public RelayCommand CancelCommand { get; private set; }
    }
}
