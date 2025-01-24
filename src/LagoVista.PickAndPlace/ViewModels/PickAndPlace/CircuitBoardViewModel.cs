using LagoVista.Core.Commanding;
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

        public async Task<InvokeResult> AlignBoardAsync()
        {
            return InvokeResult.Success;
        }

        public Task<InvokeResult> InspectPartOnboardAsync(PickAndPlaceJobPart part, PickAndPlaceJobPlacement placement)
        {
            throw new NotImplementedException();
        }

        public Task<InvokeResult> PlacePartOnboardAsync(PickAndPlaceJobPart part, PickAndPlaceJobPlacement placement)
        {
            throw new NotImplementedException();
        }

        public Task<InvokeResult> PickPartFromBoardAsync(PickAndPlaceJobPart part, PickAndPlaceJobPlacement placement)
        {
            throw new NotImplementedException();
        }

        CircuitBoardRevision _board;
        public CircuitBoardRevision Board
        {
            get => _board;
            set => Set(ref _board, value);
        }

        public RelayCommand AlignCommand { get; }
    }
}
