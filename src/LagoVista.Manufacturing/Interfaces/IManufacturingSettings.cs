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
