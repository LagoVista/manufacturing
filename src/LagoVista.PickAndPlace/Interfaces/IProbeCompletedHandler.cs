// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 965d6c46a65e7a7f0025203785ff8544f473450935ce87f921b81ce4b50d3588
// IndexVersion: 0
// --- END CODE INDEX META ---

using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IProbeCompletedHandler
    {
        void ProbeCompleted(Vector3 probeLocation);
    }
}
