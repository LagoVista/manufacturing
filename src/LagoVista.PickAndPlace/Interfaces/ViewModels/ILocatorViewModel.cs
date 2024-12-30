using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public enum MVLocatorState
    {
        Idle,
        MachineFidicual,
        BoardFidicual1,
        BoardFidicual2,
        Default,
        NozzleCalibration,
        WorkHome,
    }

    public interface ILocatorViewModel
    {
        MVLocatorState LocatorState { get; }
        string Status { get; }
        void SetLocatorState(MVLocatorState state);
    }
}
