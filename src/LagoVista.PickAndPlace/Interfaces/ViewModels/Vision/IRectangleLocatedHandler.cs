using LagoVista.PickAndPlace.Models;

namespace LagoVista.PickAndPlace.Interfaces.ViewModels.Vision
{
    public interface IRectangleLocatedHandler
    {
        void RectangleLocated(MVLocatedRectangle rectangleLocated);
        void RectangleLocatorTimeout();
        void RectangleLocatorAborted();
    }
}
