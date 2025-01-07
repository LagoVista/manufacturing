using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IStagingPlateSelectorViewModel : IMachineViewModelBase
    {

        string SelectedStagingPlateId { get; set; }
        string SelectedStagingPlateColId { get; set; }
        string SelectedStagingPlateRowId { get; set; }

        List<EntityHeader> StagingPlateCols { get; }
        List<EntityHeader> StagingPlateRows { get; }
        MachineStagingPlate SelectedStagingPlate { get; set; }
    }
}
