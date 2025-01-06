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
