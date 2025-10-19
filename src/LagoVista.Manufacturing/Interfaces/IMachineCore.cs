// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 5f54de421690b4c8ef01429b10bbaebf45addc3bd190abaf9d0cb0afdcf81509
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.Models.Drawing;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Interfaces
{
    public interface IMachineCore
    {
        Vector3 MachinePosition { get; }
        Vector3 WorkspacePosition { get; }

        LagoVista.Manufacturing.Models.Machine Settings { get; }
    }
}
