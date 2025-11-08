// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 965662349dd0d66718eaefea29ae1175d9fb2d476576a37c1be15f214bc835f5
// IndexVersion: 2
// --- END CODE INDEX META ---
using LagoVista.Manufacturing.Models;
using System.Text;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IGCodeBuilder
    {
        void CreateGCode(GCodeProject project, StringBuilder bldr);
    }
}
