using LagoVista.PickAndPlace.LumenSupport;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IPhotonProtocolHandler
    {
        PhotonResponse ParseResponse(string packet);
        PhotonCommand GenerateGCode(FeederCommands command, byte toAddress = 0, byte[] p = null);
    }
}
