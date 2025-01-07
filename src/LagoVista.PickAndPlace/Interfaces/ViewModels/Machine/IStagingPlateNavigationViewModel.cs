using LagoVista.Core.Commanding;
using LagoVista.Core.Models;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Machine
{
    public interface IStagingPlateNavigationViewModel : IStagingPlateSelectorViewModel
    {
        RelayCommand GoToStagingPlateHoleCommand { get; }
    }
}
