// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: e38c89919a48f135a54754fec25cb34a9e62289b74dd985d16c7ee01efd12771
// IndexVersion: 0
// --- END CODE INDEX META ---
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
