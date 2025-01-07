using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
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

                    StagingPlateRows = new List<EntityHeader>();
                    StagingPlateCols = new List<EntityHeader>();

                    StagingPlateRows.Add(EntityHeader.Create("-1", "-1", "-select row-"));
                    StagingPlateCols.Add(EntityHeader.Create("-1", "-1", "-select column-"));

                    for (var idx = 0; idx < SelectedStagingPlate.Size.X / (SelectedStagingPlate.HoleSpacing / 2); ++idx)
                    {
                        StagingPlateCols.Add(EntityHeader.Create($"{idx + 1}", $"{idx + 1}", $"{idx + 1}"));
                    }

                    for (var idx = 0; idx < SelectedStagingPlate.Size.Y / (SelectedStagingPlate.HoleSpacing / 2); ++idx)
                    {
                        StagingPlateRows.Add(EntityHeader.Create($"{Char.ConvertFromUtf32(idx + 65)}", $"{Char.ConvertFromUtf32(idx + 65)}", $"{Char.ConvertFromUtf32(idx + 65)}"));
                    }

                    RaisePropertyChanged(nameof(StagingPlateRows));
                    RaisePropertyChanged(nameof(StagingPlateCols));
                }

                SelectedStagingPlateColId = "-1";
                SelectedStagingPlateRowId = "-1";
            }
        }

        private string _selectedStagingPlateRowId = "-1";
        public string SelectedStagingPlateRowId
        {
            get => _selectedStagingPlateRowId;
            set
            {
                Set(ref _selectedStagingPlateRowId, value);
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


        public List<EntityHeader> StagingPlateRows { get; private set; }
        public List<EntityHeader> StagingPlateCols { get; private set; }
    }
}
