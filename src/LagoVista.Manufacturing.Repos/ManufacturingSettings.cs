using LagoVista.Manufacturing.Interfaces;
using Microsoft.Extensions.Configuration;
using System;

namespace LagoVista.Manufacturing.Repos
{
    public class ManufacturingSettings : IManufacturingSettings
    {
        public string DigiKeyClientId { get; }
        public string DigiKeyClientSecret { get; }

        public ManufacturingSettings(IConfiguration configuration)
        {
            var digiKeySection = configuration.GetSection("DigiKey");
            DigiKeyClientId = digiKeySection["ClientId"];
            DigiKeyClientSecret = digiKeySection["ClientSecret"];
        }
    }
}
