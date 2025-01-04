using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Repos;
using System;
using System.Collections.Generic;
using System.Threading;

namespace LagoVista.PickAndPlace.ViewModels
{
    public abstract class MachineViewModelBase:  ViewModelBase, IMachineViewModelBase
    {
        protected IMachineRepo _machineRepo;

        public MachineViewModelBase(IMachineRepo machineRepo) 
        {
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
         
            _machineRepo.MachineChanged += _machineRepo_MachineChanged;
            SaveMachineConfigurationCommand = new RelayCommand(() => _machineRepo.SaveCurrentMachineAsync(), () => _machineRepo.HasValidMachine);
            ReloadMachineCommand = new RelayCommand(() => _machineRepo.ReloadedAsync(), () => _machineRepo.HasValidMachine);
        }

        private void _machineRepo_MachineChanged(object sender, IMachine e)
        {
            Machine = e;
            SaveMachineConfigurationCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(MachineConfiguration));
            Machine.MachineDisconnected += (e,m) => RaiseCanExecuteChanged();
            Machine.MachineConnected += (e, m) => RaiseCanExecuteChanged();

            MachineChanged(e);
            
        }

        protected void RaiseCanExecuteChanged()
        {
            foreach(var handler in _registeredCommandHandlers)
            {
                handler.RaiseCanExecuteChanged();
            }
        }

        protected void RegisterCommandHandler(RelayCommand cmd)
        {
            _registeredCommandHandlers.Add(cmd);
        }

        protected RelayCommand CreatedMachineConnectedCommand(Action execute, Func<bool> canExecute = null)
        {
            var cmd = (canExecute == null) ? new RelayCommand(execute, () => Machine != null && Machine.Connected) : new RelayCommand(execute, () => Machine != null && Machine.Connected && canExecute());
            RegisterCommandHandler(cmd);
            return cmd;
        }

        protected RelayCommand CreatedCommand(Action execute, Func<bool> canExecute = null)
        {
            var cmd =  (canExecute == null) ? new RelayCommand(execute) :  new RelayCommand(execute);
            RegisterCommandHandler(cmd);
            return cmd;
        }

        private List<RelayCommand> _registeredCommandHandlers = new List<RelayCommand>();

        private IMachine _machine;
        public IMachine Machine
        {
            get => _machine;
            set => Set(ref _machine, value);
        }

        public Manufacturing.Models.Machine MachineConfiguration
        {
            get => _machine?.Settings;
        }

        protected IMachineRepo MachineRepo { get; }


        public RelayCommand SaveMachineConfigurationCommand { get; }
        public RelayCommand ReloadMachineCommand { get; set; }

        protected virtual void MachineChanged(IMachine machine) { }
    }
}
