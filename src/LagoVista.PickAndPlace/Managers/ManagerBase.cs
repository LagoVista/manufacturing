// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: af2f4ccf9c26e0b12eb2916f70914312b319eba02f9ed40db211e9005ad762e4
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.PlatformSupport;
using LagoVista.PickAndPlace.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Managers
{
    public class ManagerBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        public ManagerBase(IMachine machine, ILogger logger)
        {
            Machine = machine;
            Logger = logger;
        }

        protected void RaisePropertyChanged([CallerMemberName]string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected IMachine Machine { get; private set; }
        protected ILogger Logger { get; private set; }
    }
}
