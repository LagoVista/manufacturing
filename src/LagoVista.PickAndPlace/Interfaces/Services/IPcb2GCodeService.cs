// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: f4d890493a4e71718e556459e9ccbbf59820cad7b468cd0360f48d68803307bb
// IndexVersion: 0
// --- END CODE INDEX META ---
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
