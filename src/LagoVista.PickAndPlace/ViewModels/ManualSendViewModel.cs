using LagoVista.Core.Commanding;
using LagoVista.Core.PlatformSupport;
using LagoVista.Core.ViewModels;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.ViewModels
{
    public class ManualSendViewModel : ViewModelBase, IManualSendViewModel
    {
        ILogger _logger;
        IMachineRepo _machineRepo;
        private int _commandBufferLocation = 0;
        private List<String> _commandBuffer = new List<string>();

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
            if (!String.IsNullOrEmpty(ManualCommandText))
            {
                _machineRepo.CurrentMachine.SendCommand(ManualCommandText);
                ManualCommandText = String.Empty;
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
