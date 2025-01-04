using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Vision
{
    public interface ICircleLocatedHandler
    {
        void CircleLocated(MVLocatedCircle circle);
        void CircleLocatorTimeout();
        void CircleLocatorAborted();
    }
}
