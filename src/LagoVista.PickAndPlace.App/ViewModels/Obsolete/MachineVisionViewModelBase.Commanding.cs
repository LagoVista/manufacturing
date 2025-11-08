// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 3b86d8c5475d03f59bd0b4b318e206ef7726bc8f7d278018bc78e91bd23733de
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public abstract partial class MachineVisionViewModelBase
    {
        private bool CanPlay()
        {
            return true;
        }

        private bool CanStop()
        {
            return true;
        }

        public RelayCommand StartCaptureCommand { get; private set; }
        public RelayCommand StopCaptureCommand { get; private set; }
    }
}
