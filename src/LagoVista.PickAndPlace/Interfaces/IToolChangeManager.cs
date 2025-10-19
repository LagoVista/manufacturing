// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a1971f156f7dd4880f0969a1d3e003e8d43455b2f8db6395bab0d8d5fbd021ea
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.GCode.Commands;
using System.Threading.Tasks;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IToolChangeManager
    {
        Task<bool> HandleToolChange(ToolChangeCommand cmd);
    }
}
