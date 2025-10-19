// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: dc7e7cd06b68e3f1d34474a1f58e318db15885647ef08a5663ade83bbb756dc5
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IManualSendViewModel : IViewModel
    {
        string ManualCommandText { get; set; }
        RelayCommand ManualSendCommand { get; }

        void ShowPrevious();
        void ShowNext();
        void ManualSend();
    }
}
