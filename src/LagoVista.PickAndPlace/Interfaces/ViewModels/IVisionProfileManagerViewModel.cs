using LagoVista.Core.Models;
using LagoVista.Core.Validation;
using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels
{
    public interface IVisionProfileManagerViewModel
    {
        void LoadProfiles();
        InvokeResult SelectProfile(string profile);

        EntityHeader CurrentMVProfile { get; }

        VisionSettings TopCameraProfile { get; }
        VisionSettings BottomCameraProfile { get; }

        void SaveProfile();
    }
}
