using LagoVista.Manufacturing.Models;
using System.Text;

namespace LagoVista.Manufacturing.Interfaces.Managers
{
    public interface IGCodeBuilder
    {
        void CreateGCode(GCodeProject project, StringBuilder bldr);
    }
}
