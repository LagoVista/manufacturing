using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Vision
{
    public interface ICornerLocatedHandler
    {
        void CornerLocated(MVLocatedCorner circles);
        void CornerLocatorTimeout();
        void CornerLocatorAborted();
    }
}
