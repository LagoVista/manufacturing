using LagoVista.Client.Core;
using LagoVista.Core.IOC;
using LagoVista.Core.Models.Drawing;
using LagoVista.PCB.Eagle.Models;
using LagoVista.PickAndPlace.Interfaces;
using LagoVista.PickAndPlace.Interfaces.ViewModels;
using LagoVista.PickAndPlace.ViewModels;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.App.ViewModels
{
    public partial class PnPJobViewModel : MachineVisionViewModelBase
    {
        private IRestClient _restClient;
        private bool _isEditing;
        private bool _isPaused;
        private bool _isDirty = false;
        
        private bool _isPlacingParts = false;
        private BOM _billOfMaterials;
        private int _partIndex = 0;


        public PnPJobViewModel(IMachine machine, IRestClient restClient) : base(machine)
        {
            _restClient = SLWIOC.Get<IRestClient>();
            StripFeederVM = new StripFeederViewModel(machine, this);

            VisionManagerVM = new VisionManagerViewModel(machine);
            LocatorVM = new LocatorViewModel(machine);

            ToolAlignmentVM = new ToolAlignmentViewModel(machine, LocatorVM, VisionManagerVM);

            NozzleTipCalibrationVM = new NozzleTipCalibrationViewModel(machine, LocatorVM, VisionManagerVM);
            FiducialVM = new FiducialViewModel(machine, LocatorVM);
            PartsVM = new PartsViewModel(machine, LocatorVM, restClient);
            HomingVM = new HomingViewModel(machine, VisionManagerVM, LocatorVM);
            AddCommands();
        }

        public override void CircleCentered(Point2D<double> point, double diameter)
        {
            switch (LocatorVM.LocatorState)
            {
                case MVLocatorState.WorkHome:
                    Machine.SetWorkspaceHome();
                    LocatorVM.SetLocatorState(MVLocatorState.Idle);
                    Status = "W/S Home Found";
                    break;
                case MVLocatorState.MachineFidicual:
                    SetNewHome();
                    break;
                case MVLocatorState.NozzleCalibration:
                    PerformBottomCameraCalibration(point, diameter, new Point2D<double>(0, 0));
                    break;
                default:
                    break;
            }
        }

        public override async Task IsClosingAsync()
        {
            await SaveJobAsync();
            await base.IsClosingAsync();
        }
    }
}
