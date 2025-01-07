using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IStagingPlateSelectorViewModel : IMachineViewModelBase
    {

        string SelectedStagingPlateId { get; set; }
        string SelectedStagingPlateColId { get; set; }
        string SelectedStagingPlateRowId { get; set; }
        string Summary { get; set; }


        ObservableCollection<EntityHeader> StagingPlates { get; }
        ObservableCollection<EntityHeader> StagingPlateCols { get; }
        ObservableCollection<EntityHeader> StagingPlateRows { get; }
        MachineStagingPlate SelectedStagingPlate { get; set; }
    }
}
