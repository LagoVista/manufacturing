// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 7dd4c74dfb7d6478c3868810cbe6981fed064eae756d1d50c3ca317af103dc14
// IndexVersion: 0
// --- END CODE INDEX META ---
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
