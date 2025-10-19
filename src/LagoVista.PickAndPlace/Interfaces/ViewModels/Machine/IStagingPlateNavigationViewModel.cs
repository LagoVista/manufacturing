// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 319cda85af9c7ffa65a09eaf098336a082f0837d16e05434bd7a22c21e1f99f2
// IndexVersion: 0
// --- END CODE INDEX META ---
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
