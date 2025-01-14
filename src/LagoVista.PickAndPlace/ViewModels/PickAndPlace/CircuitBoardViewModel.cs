using LagoVista.Core.Validation;
using LagoVista.Manufacturing.Models;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Interfaces.ViewModels.PickAndPlace;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.ViewModels.PickAndPlace
{
    public class CircuitBoardViewModel : MachineViewModelBase, ICircuitBoardViewModel
    {
        public CircuitBoardViewModel(IMachineRepo machineRepo) : base(machineRepo)
        {

        }

        public List<string> GetPlacementGCode(PickAndPlaceJobPart part, PickAndPlaceJobPart placement)
        {
            throw new NotImplementedException();
        }

        public Task<InvokeResult> PlacePart(PickAndPlaceJobPart part, PickAndPlaceJobPart placement)
        {
            throw new NotImplementedException();
        }
    }
}
