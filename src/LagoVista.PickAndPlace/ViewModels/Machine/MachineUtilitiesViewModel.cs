using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using PdfSharpCore.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class MachineUtilitiesViewModel : MachineViewModelBase, IMachineUtilitiesViewModel
    {
        StagingPlateUtils _stagingPlateUtils;

        public MachineUtilitiesViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {         
            ReadLeftVacuumCommand = CreatedMachineConnectedCommand(ReadLeftVacuum);
            ReadRightVacuumCommand = CreatedMachineConnectedCommand(ReadRightVacuum);

            LeftVacuumOnCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.LeftVacuumPump = true);
            LeftVacuumOffCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.LeftVacuumPump = false);

            RightVacuumOnCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.RightVacuumPump = true);
            RightVacuumOffCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.RightVacuumPump = false);

            TopLightOnCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.TopLightOn = true);
            TopLightOffCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.TopLightOn = false);
            BottomLightOnCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.BottomLightOn = true);
            BottomLightOffCommand = CreatedMachineConnectedCommand(() => _machineRepo.CurrentMachine.BottomLightOn = false);
            GoToStagingPlateHoleCommand = CreatedMachineConnectedCommand(GoToStagingPlateHole, () => SelectedStagingPlateColId != "-1" && SelectedStagingPlateRowId != "-1");
        }       

        public async void ReadLeftVacuum()
        {
             var result = await _machineRepo.CurrentMachine.ReadLeftVacuumAsync();
            if(result.Successful)
            {
                LeftVacuum = result.Result;
                RaisePropertyChanged(nameof(LeftVacuum));
            }
        }

        public async void ReadRightVacuum()
        {
            var result = await _machineRepo.CurrentMachine.ReadRightVacuumAsync();
            if (result.Successful)
            {
                RightVacuum = result.Result;
                RaisePropertyChanged(nameof(RightVacuum));
            }
        }

        private void GoToStagingPlateHole()
        {
            var point = _stagingPlateUtils.ResolveStagePlateWorkSpaceLocation(SelectedStagingPlateColId, SelectedStagingPlateRowId);
            Debug.WriteLine(point);
        }

        public ulong LeftVacuum { get; private set; }

        public ulong RightVacuum { get; private set; }

        protected override void MachineChanged(IMachine machine)
        {
            _machineRepo.CurrentMachine.MachineConnected += CurrentMachine_MachineConnected;
            _machineRepo.CurrentMachine.MachineDisconnected += CurrentMachine_MachineDisconnected;
        }

        void RefreshCanExecute()
        {
            ReadLeftVacuumCommand.RaiseCanExecuteChanged();
            ReadRightVacuumCommand.RaiseCanExecuteChanged();
            LeftVacuumOnCommand.RaiseCanExecuteChanged();
            LeftVacuumOffCommand.RaiseCanExecuteChanged();
            RightVacuumOnCommand.RaiseCanExecuteChanged();
            RightVacuumOffCommand.RaiseCanExecuteChanged();
            TopLightOnCommand.RaiseCanExecuteChanged();
            TopLightOffCommand.RaiseCanExecuteChanged();
            BottomLightOnCommand.RaiseCanExecuteChanged();
            BottomLightOffCommand.RaiseCanExecuteChanged();
        }

        private void CurrentMachine_MachineDisconnected(object sender, EventArgs e)
        {
            RefreshCanExecute();
        }

        private void CurrentMachine_MachineConnected(object sender, EventArgs e)
        {
            RefreshCanExecute();
        }

        public RelayCommand ReadLeftVacuumCommand { get; }
        public RelayCommand ReadRightVacuumCommand { get; }

        public RelayCommand LeftVacuumOnCommand { get; }
        public RelayCommand LeftVacuumOffCommand { get; }
        public RelayCommand RightVacuumOnCommand { get; }
        public RelayCommand RightVacuumOffCommand { get; }

        public RelayCommand TopLightOnCommand { get; }
        public RelayCommand TopLightOffCommand { get; }


        public RelayCommand BottomLightOnCommand { get; }
        public RelayCommand BottomLightOffCommand { get; }

        public RelayCommand GoToStagingPlateHoleCommand { get; }

        public List<EntityHeader> StagingPlateRows { get; private set; }
        public List<EntityHeader> StagingPlateCols { get; private set; }

        private string _selectedStagingPlateRowId = "-1";
        public string SelectedStagingPlateRowId
        {
            get => _selectedStagingPlateRowId;
            set
            {
                Set(ref _selectedStagingPlateRowId, value);
                GoToStagingPlateHoleCommand.RaiseCanExecuteChanged();
            }
        }

        private string _selectedStagingPlateColId = "-1";
        public string SelectedStagingPlateColId
        {
            get => _selectedStagingPlateRowId;
            set 
            {
                Set(ref _selectedStagingPlateColId, value);
                GoToStagingPlateHoleCommand.RaiseCanExecuteChanged();
            }
         }

        public string SelectedStagingPlateId
        {
            get => _selectedStagingPlate?.Id;
            set
            {
                if (String.IsNullOrEmpty(value) || value == "-1")
                {
                    StagingPlateRows = new List<EntityHeader>();
                    StagingPlateCols = new List<EntityHeader>();

                    StagingPlateRows.Add(EntityHeader.Create("-1", "-1", "-select a staging plate first-"));
                    StagingPlateCols.Add(EntityHeader.Create("-1", "-1", "-select a staging plate first-"));
                }
                else
                {
                    SelectedStagingPlate = MachineConfiguration.StagingPlates.Single(sp => sp.Id == value);
                    _stagingPlateUtils = new StagingPlateUtils(SelectedStagingPlate);

                    StagingPlateRows = new List<EntityHeader>();
                    StagingPlateCols = new List<EntityHeader>();

                    StagingPlateRows.Add(EntityHeader.Create("-1", "-1", "-select row-"));
                    StagingPlateCols.Add(EntityHeader.Create("-1", "-1", "-select row-"));

                    for (var idx = 0; idx < SelectedStagingPlate.Size.X / (SelectedStagingPlate.HoleSpacing / 2); ++idx)
                    {
                        StagingPlateCols.Add(EntityHeader.Create($"{idx + 1}", $"{idx + 1}", $"{idx + 1}"));
                    }

                    for (var idx = 0; idx < SelectedStagingPlate.Size.Y / (SelectedStagingPlate.HoleSpacing / 2); ++idx)
                    {
                        StagingPlateRows.Add(EntityHeader.Create($"{idx + 1}", $"{idx + 1}", $"{Char.ConvertFromUtf32(idx + 65)}"));
                    }

                    RaisePropertyChanged(nameof(StagingPlateRows));
                    RaisePropertyChanged(nameof(StagingPlateCols));
                }

                SelectedStagingPlateColId = "-1";
                SelectedStagingPlateRowId = "-1";

            }
        }

        MachineStagingPlate _selectedStagingPlate;
        public MachineStagingPlate SelectedStagingPlate
        {
            get => _selectedStagingPlate;
            set => Set(ref _selectedStagingPlate, value);
        }
    }
}
