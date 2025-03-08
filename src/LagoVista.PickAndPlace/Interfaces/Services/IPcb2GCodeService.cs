using LagoVista.Manufacturing.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces.Services
{
    public interface IPcb2GCodeService
    {
        void CreateGCode(PcbMillingProject project);
    }
}
