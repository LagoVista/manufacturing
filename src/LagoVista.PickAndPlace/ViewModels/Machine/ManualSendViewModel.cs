// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: c72c832c2f219b8187869a7f972993fa40958ea40321fa3341951d052920cdd3
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class ManualSendViewModel : ViewModelBase, IManualSendViewModel
    {
        ILogger _logger;
        IMachineRepo _machineRepo;
        private int _commandBufferLocation = 0;
        private List<string> _commandBuffer = new List<string>();

        public ManualSendViewModel(IMachineRepo machineRepo, ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _machineRepo = machineRepo ?? throw new ArgumentNullException(nameof(machineRepo));
            _machineRepo.PropertyChanged += _machineRepo_PropertyChanged;
            ManualSendCommand = new RelayCommand(ManualSend, CanManualSend);
        }

        private void _machineRepo_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            ManualSendCommand.RaiseCanExecuteChanged();
        }

        public void ManualSend()
        {
            _commandBuffer.Add(ManualCommandText);
            _commandBufferLocation = _commandBuffer.Count;
            if (!string.IsNullOrEmpty(ManualCommandText))
            {
                _machineRepo.CurrentMachine.SendCommand(ManualCommandText);
                ManualCommandText = string.Empty;
            }
        }

        public bool CanManualSend()
        {
            return _machineRepo.CurrentMachine.Connected && _machineRepo.CurrentMachine.Mode == OperatingMode.Manual;
        }

        public bool CanShowPrevious()
        {
            return _commandBuffer.Count > 0 && _commandBufferLocation > 0;
        }

        public bool CanShowNext()
        {
            return _commandBufferLocation < _commandBuffer.Count - 1;
        }

        public void ShowPrevious()
        {
            if (CanShowPrevious())
            {
                --_commandBufferLocation;
                ManualCommandText = _commandBuffer[_commandBufferLocation];
            }
        }

        public void ShowNext()
        {
            if (CanShowNext())
            {
                ++_commandBufferLocation;
                ManualCommandText = _commandBuffer[_commandBufferLocation];
            }
            else
            {
                ++_commandBufferLocation;
                _commandBufferLocation = Math.Min(_commandBufferLocation, _commandBuffer.Count);
                ManualCommandText = string.Empty;
            }
        }

        string _manualCommandText;
        public string ManualCommandText
        {
            get => _manualCommandText;
            set => Set(ref _manualCommandText, value);
        }

        public RelayCommand ManualSendCommand { get; }
    }
}
