using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels
{


    public class LocatorViewModel : ViewModelBase, ILocatorViewModel
    {
        IMachine _machine;
        public LocatorViewModel(IMachine machine)
        {
            _machine = machine;
            AbortMVLocatorCommand = new RelayCommand(() => SetLocatorState(MVLocatorState.Idle));
        }

        private MVLocatorState _mvLocatorState = MVLocatorState.Default;
        public MVLocatorState LocatorState
        {
            get => _mvLocatorState;
            private set => _mvLocatorState = value;
        }

        private string _status;
        public string Status
        {
            private set => Set(ref _status, value);
            get => _status;
        }

        public void SetLocatorState(MVLocatorState state)
        {
            LocatorState = state;
        }

        public RelayCommand AbortMVLocatorCommand { get; private set; }

    }
}
