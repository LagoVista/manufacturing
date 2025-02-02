using LagoVista.Core.Commanding;
using LagoVista.Core.ViewModels;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Input;

namespace LagoVista.PickAndPlace.ViewModels
{
    public abstract class MachineViewModelBase:  ViewModelBase, IMachineViewModelBase
    {
        public MachineViewModelBase(IMachineRepo machineRepo) 
        {
            MachineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            Machine = MachineRepo.CurrentMachine;
            MachineRepo.MachineChanged += _machineRepo_MachineChanged;
            SaveMachineConfigurationCommand = new RelayCommand(() => MachineRepo.SaveCurrentMachineAsync(), () => MachineRepo.HasValidMachine);
            ReloadMachineCommand = new RelayCommand(() => MachineRepo.LoadCurrentMachineAsync(), () => MachineRepo.HasValidMachine);
            UnlockSettingsCommand = new RelayCommand(() => _machine.AreSettingsLocked = false);
            LockSettingsCommand = new RelayCommand(() => _machine.AreSettingsLocked = true);
        }

        private void _machineRepo_MachineChanged(object sender, IMachine e)
        {
            Machine = e;
            SaveMachineConfigurationCommand.RaiseCanExecuteChanged();
            RaisePropertyChanged(nameof(MachineConfiguration));
            Machine.MachineDisconnected += (e, m) => { RaiseCanExecuteChanged(); OnMachineDisconnected(); };
            Machine.MachineConnected += (e, m) => { RaiseCanExecuteChanged(); OnMachineConnected(); };
            Machine.SettingsLocked += (e, m) => RaiseCanExecuteChanged();
            Machine.SettingsUnlocked += (e, m) => RaiseCanExecuteChanged();
            Machine.PropertyChanged += Machine_PropertyChanged;

            MachineChanged(e);            
        }

        protected virtual void OnMachineConnected()
        {
        }


        protected virtual void OnMachineDisconnected()
        {
         
        }

        private void Machine_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(Machine.WasMachinOriginCalibrated) ||
                e.PropertyName == nameof(Machine.IsLocating) ||
                e.PropertyName == nameof(Machine.WasMachineHomed))
            {
                RaiseCanExecuteChanged();
            }
        }

        protected virtual void RaiseCanExecuteChanged()
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
            var cmd = (canExecute == null) ? new RelayCommand(execute, () => Machine != null && Machine.Connected && !Machine.IsLocating && Machine.WasMachineHomed && Machine.WasMachinOriginCalibrated) : 
                                             new RelayCommand(execute, () => Machine != null && Machine.Connected && !Machine.IsLocating && Machine.WasMachineHomed && Machine.WasMachinOriginCalibrated&& canExecute());
            RegisterCommandHandler(cmd);
            return cmd;
        }

        protected RelayCommand CreatedMachineConnectedSettingsCommand(Action execute, Func<bool> canExecute = null)
        { 
            var cmd = (canExecute == null) ? new RelayCommand(execute, () => Machine != null && Machine.Connected && !Machine.IsLocating && Machine.WasMachineHomed && Machine.WasMachinOriginCalibrated && !Machine.AreSettingsLocked) :
                                             new RelayCommand(execute, () => Machine != null && Machine.Connected && !Machine.IsLocating && !Machine.AreSettingsLocked && canExecute());
            RegisterCommandHandler(cmd);
            return cmd;
        }

        protected RelayCommand CreateCommand(Action execute, Func<bool> canExecute = null)
        {
            var cmd =  (canExecute == null) ? new RelayCommand(execute) :  new RelayCommand(execute, canExecute);
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

        protected bool SelectedIDHasValue(string id)
        {
            if(String.IsNullOrEmpty(id)) 
                return false;

            if (id == "-1")
                return false;

            return true;
        }



        public IMachineRepo MachineRepo { get; }
        public RelayCommand SaveMachineConfigurationCommand { get; }
        public RelayCommand ReloadMachineCommand { get; set; }
        public RelayCommand LockSettingsCommand { get; set; }
        public RelayCommand UnlockSettingsCommand { get; set; }   
        protected virtual void MachineChanged(IMachine machine) { }
    }
}
