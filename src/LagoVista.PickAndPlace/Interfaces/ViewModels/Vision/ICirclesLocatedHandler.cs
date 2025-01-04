using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Vision
{
    public interface ICirclesLocatedHandler
    {
        void CirclesLocated(MVLocatedCircles circles);
        void CirclesLocatorTimeout();
        void CirclesLocatorAborted();
    }
}
