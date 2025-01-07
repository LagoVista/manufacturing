using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class StagingPlateSelectorViewModel : MachineViewModelBase, IStagingPlateSelectorViewModel
    {
        public StagingPlateSelectorViewModel(IMachineRepo repo) : base(repo)
        {

        }

        protected override void MachineChanged(IMachine machine)
        {
            base.MachineChanged(machine);

            StagingPlates = MachineConfiguration.StagingPlates.Select(sp => EntityHeader.Create(sp.Id, sp.Key, sp.Name)).ToList();  
            StagingPlates.Insert(0, EntityHeader.Create("-1", "-1", "-select staging plate-")); 
            SelectedStagingPlateId = "-1";
            RaisePropertyChanged(nameof(StagingPlates));
        }

        public string SelectedStagingPlateId
        {
            get => _selectedStagingPlate == null ? "-1" : _selectedStagingPlate.Id;
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

                    StagingPlateRows = new List<EntityHeader>();
                    StagingPlateRows.Add(EntityHeader.Create("-1", "-1", "-select row-"));

                    for (var idx = 0; idx < SelectedStagingPlate.Size.Y / (SelectedStagingPlate.HoleSpacing / 2); ++idx)
                    {
                        StagingPlateRows.Add(EntityHeader.Create($"{Char.ConvertFromUtf32(idx + 65)}", $"{Char.ConvertFromUtf32(idx + 65)}", $"{Char.ConvertFromUtf32(idx + 65)}"));
                    }

                    RaisePropertyChanged(nameof(StagingPlateRows));
                }

                SelectedStagingPlateRowId = "-1";
                SelectedStagingPlateColId = "-1";
                RaisePropertyChanged(nameof(CanSelectStagingPlateRow));
            }
        }

        private void PopulateStagingPlateColumns()
        {
            StagingPlateCols = new List<EntityHeader>();
            StagingPlateCols.Add(EntityHeader.Create("-1", "-1", "-select column-"));

            var rowIdx = Convert.ToByte(SelectedStagingPlateRowId.ToCharArray()[0]) - 64;

            SelectedStagingPlate.FirstUsableColumn = 5;
            SelectedStagingPlate.LastUsableColumn = 35;

            for (var idx = SelectedStagingPlate.FirstUsableColumn; idx <= SelectedStagingPlate.LastUsableColumn; idx += 2)
            {
                var col = idx + (rowIdx % 2 == 1 ? 1 : 0);
                if(col <=SelectedStagingPlate.LastUsableColumn)
                    StagingPlateCols.Add(EntityHeader.Create($"{col}", $"{col}", $"{col}"));
            }
            RaisePropertyChanged(nameof(StagingPlateCols));
            SelectedStagingPlateColId = "-1";
        }

        private string _selectedStagingPlateRowId = "-1";
        public string SelectedStagingPlateRowId
        {
            get => _selectedStagingPlateRowId;
            set
            {
                Set(ref _selectedStagingPlateRowId, value);
                if(value != "-1")
                    PopulateStagingPlateColumns();
                else
                {
                    StagingPlateCols = new List<EntityHeader>();
                    StagingPlateCols.Add(EntityHeader.Create("-1", "-1", "-select staging plate row first-"));
                    RaisePropertyChanged(nameof(StagingPlateCols));

                }
                RaisePropertyChanged(nameof(CanSelectStagingPlateCol));

                RaiseCanExecuteChanged();
            }
        }        

        private string _selectedStagingPlateColId = "-1";
        public string SelectedStagingPlateColId
        {
            get => _selectedStagingPlateColId;
            set
            {
                Set(ref _selectedStagingPlateColId, value);
                RaiseCanExecuteChanged();
            }
        }

        public bool CanSelectStagingPlateRow => SelectedStagingPlateId != "-1";
        public bool CanSelectStagingPlateCol => SelectedStagingPlateRowId != "-1";

        MachineStagingPlate _selectedStagingPlate;
        public MachineStagingPlate SelectedStagingPlate
        {
            get => _selectedStagingPlate;
            set
            {
                Set(ref _selectedStagingPlate, value);
                RaiseCanExecuteChanged();
            }
        }

        public List<EntityHeader> StagingPlates { get; private set; }

        public List<EntityHeader> StagingPlateRows { get; private set; }
        public List<EntityHeader> StagingPlateCols { get; private set; }
    }
}
