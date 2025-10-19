// --- BEGIN CODE INDEX META (do not edit) ---
// ContentHash: 39d02e15c321a1000d336beebac15aaa8661a83cf4737a4700b94932828a86f1
// IndexVersion: 0
// --- END CODE INDEX META ---
using System;
using System.Collections.Generic;
using System.Text;

namespace LagoVista.Manufacturing.Interfaces
{
    public interface IManufacturingSettings
    {
        String DigiKeyClientId { get; set; }
        String DigiKeyClientSecret { get; set; }
    }
}
