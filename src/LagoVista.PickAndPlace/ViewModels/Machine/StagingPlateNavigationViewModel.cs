using LagoVista.Core.Commanding;
using LagoVista.Manufacturing.Util;
using LagoVista.PickAndPlace.Interfaces.ViewModels.Machine;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.ViewModels.Machine
{
    public class StagingPlateNavigationViewModel : StagingPlateSelectorViewModel, IStagingPlateNavigationViewModel
    {
        public StagingPlateNavigationViewModel(IMachineRepo repo) : base(repo)
        {
            GoToStagingPlateHoleCommand = CreatedMachineConnectedCommand(GoToStagingPlateHole, () => SelectedStagingPlateColId != "-1" && SelectedStagingPlateRowId != "-1");
        }

        private void GoToStagingPlateHole()
        {
            var point = StagingPlateUtils.ResolveStagePlateWorkSpaceLocation(SelectedStagingPlate, SelectedStagingPlateColId, SelectedStagingPlateRowId);
            Machine.SendSafeMoveHeight();
            Machine.GotoPoint(point);
            Machine.SetVisionProfile(Manufacturing.Models.CameraTypes.Position, VisionProfile.VisionProfile_StagingPlateHole);
        }

        public RelayCommand GoToStagingPlateHoleCommand { get; }

    }
}
