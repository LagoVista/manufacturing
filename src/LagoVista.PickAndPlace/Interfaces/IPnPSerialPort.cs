// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 03570845b170726592faa52bcfa51e54d489e6d7f99b041d58844b8248d62016
// IndexVersion: 0
// --- END CODE INDEX META ---
using LagoVista.Core.PlatformSupport;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace LagoVista.PickAndPlace.Interfaces
{
    public interface IPnPSerialPort : ISerialPort
    {
        Stream InputStream { get; }
        Stream OutputStream { get; }
    }
}
