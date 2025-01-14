using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using RingCentral;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

            StagingPlates = new ObservableCollection<EntityHeader>(MachineConfiguration.StagingPlates.Select(sp => EntityHeader.Create(sp.Id, sp.Key, sp.Name)));  
            StagingPlates.Insert(0, EntityHeader.Create(StringExtensions.NotSelectedId, StringExtensions.NotSelectedId, "-select staging plate-")); 
            SelectedStagingPlateId = StringExtensions.NotSelectedId;
            RaisePropertyChanged(nameof(StagingPlates));
        }

        public string SelectedStagingPlateId
        {
            get => _selectedStagingPlate == null ? StringExtensions.NotSelectedId : _selectedStagingPlate.Id;
            set
            {
                if (!value.HasValidId())
                {
                    _selectedStagingPlate = null;
                    StagingPlateRows.Clear();
                    StagingPlateRows.Add(EntityHeader.Create(StringExtensions.NotSelectedId, StringExtensions.NotSelectedId, "-select a staging plate first-"));
                    StagingPlateCols.Clear();
                    StagingPlateCols.Add(EntityHeader.Create(StringExtensions.NotSelectedId, StringExtensions.NotSelectedId, "-select a staging plate first-"));
                }
                else
                {
                    _selectedStagingPlate = MachineConfiguration.StagingPlates.FirstOrDefault(sp => sp.Id == value);
                    RaisePropertyChanged(nameof(SelectedStagingPlate));
                    SelectedStagingPlate = MachineConfiguration.StagingPlates.Single(sp => sp.Id == value);
                    StagingPlateRows.Clear();
                    StagingPlateRows.Add(EntityHeader.Create(StringExtensions.NotSelectedId, StringExtensions.NotSelectedId, "-select row-"));

                    for (var idx = 0; idx < SelectedStagingPlate.Size.Y / (SelectedStagingPlate.HoleSpacing / 2); ++idx)
                    {
                        StagingPlateRows.Add(EntityHeader.Create($"{Char.ConvertFromUtf32(idx + 65)}", $"{Char.ConvertFromUtf32(idx + 65)}", $"{Char.ConvertFromUtf32(idx + 65)}"));
                    }
                }
                
                SelectedStagingPlateRowId = StringExtensions.NotSelectedId;
                SelectedStagingPlateColId = StringExtensions.NotSelectedId;
                RaisePropertyChanged(nameof(SelectedStagingPlateId));
                RaisePropertyChanged(nameof(CanSelectStagingPlateRow));
            }
        }

        private void PopulateStagingPlateColumns()
        {
            StagingPlateCols.Clear();
            StagingPlateCols.Add(EntityHeader.Create(StringExtensions.NotSelectedId, StringExtensions.NotSelectedId, "-select column-"));

            var rowIdx = Convert.ToByte(SelectedStagingPlateRowId.ToCharArray()[0]) - 64;

            for (var col = SelectedStagingPlate.FirstUsableColumn; col <= SelectedStagingPlate.LastUsableColumn; col++)
            {
                if(col <=SelectedStagingPlate.LastUsableColumn)
                    StagingPlateCols.Add(EntityHeader.Create($"{col}", $"{col}", $"{col}"));
            }

            SelectedStagingPlateColId = StringExtensions.NotSelectedId;
        }

        private string _selectedStagingPlateRowId = StringExtensions.NotSelectedId;
        public string SelectedStagingPlateRowId
        {
            get => _selectedStagingPlateRowId;
            set
            {
                Summary = "-not set-";
                Set(ref _selectedStagingPlateRowId, value);
                if(value.HasValidId())
                    PopulateStagingPlateColumns();
                else
                {
                    StagingPlateCols.Clear();
                    StagingPlateCols.Add(EntityHeader.Create(StringExtensions.NotSelectedId, StringExtensions.NotSelectedId, "-select staging plate row first-"));
                    RaisePropertyChanged(nameof(StagingPlateCols));

                }
                RaisePropertyChanged(nameof(CanSelectStagingPlateCol));

                RaiseCanExecuteChanged();
            }
        }        

        private string _selectedStagingPlateColId = StringExtensions.NotSelectedId;
        public string SelectedStagingPlateColId
        {
            get => _selectedStagingPlateColId;
            set
            {
                Set(ref _selectedStagingPlateColId, value);

                if (!value.HasValidId())
                    Summary = "-not set-";
                else
                    Summary = $"{SelectedStagingPlate.Name} - {SelectedStagingPlateRowId}{SelectedStagingPlateColId}";
                RaiseCanExecuteChanged();
            }
        }

        public bool CanSelectStagingPlateRow => SelectedStagingPlateId.HasValidId();
        public bool CanSelectStagingPlateCol => SelectedStagingPlateRowId.HasValidId();

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

        private string _summary = "-not set-";
        public string Summary
        {
            get => _summary;
            set => Set(ref _summary, value);
        }

        public ObservableCollection<EntityHeader> StagingPlates { get; private set; } = new ObservableCollection<EntityHeader>();
        public ObservableCollection<EntityHeader> StagingPlateRows { get; private set; } = new ObservableCollection<EntityHeader>();
        public ObservableCollection<EntityHeader> StagingPlateCols { get; private set; } = new ObservableCollection<EntityHeader>();    
    }
}
