// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 978f0efd2c151b037053ab961bf46b535cfecee2513fd9368ddaef30656b1377
// IndexVersion: 2
// --- END CODE INDEX META ---
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
