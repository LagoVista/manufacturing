using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
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
