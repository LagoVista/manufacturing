// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 97d453bf800f2199d540aff777fb436646cb50e36d229477bc59d361a088e6cb
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IProbeCompletedHandler
    {
        void ProbeCompleted(Vector3 probeLocation);
    }
}
