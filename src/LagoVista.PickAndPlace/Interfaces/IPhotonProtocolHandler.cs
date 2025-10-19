// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: a0e89583c324fae1c34531e67025bd3de0b205d51282e7a38f701224eb30d9a8
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.PickAndPlace.LumenSupport;
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IPhotonProtocolHandler
    {
        PhotonResponse ParseResponse(string packet);
        PhotonCommand GenerateGCode(FeederCommands command, byte toAddress , byte[] p = null);
        PhotonCommand GenerateGCode(FeederCommands command, byte toAddress, string byteArray);
    }
}
