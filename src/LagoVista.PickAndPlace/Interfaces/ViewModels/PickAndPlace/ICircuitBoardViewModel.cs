using LagoVista.Core.Commanding;
using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace
{
    public interface ICircuitBoardViewModel : IMachineViewModelBase
    {
        List<string> GetPlacementGCode(PickAndPlaceJobPart part, PickAndPlaceJobPart placement);
        Task<InvokeResult> PlacePart(PickAndPlaceJobPart part, PickAndPlaceJobPart placement);
    }
}
