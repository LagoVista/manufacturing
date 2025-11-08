// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 9e5ef402f3d13ebf0a0ef16c94176a782fe63937543ebc55cb7c55ecfb71504a
// IndexVersion: 2
// --- END CODE INDEX META ---
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
