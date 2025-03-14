
using LagoVista.Core.Models.Drawing;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IProbeCompletedHandler
    {
        void ProbeCompleted(Vector3 probeLocation);
    }
}
